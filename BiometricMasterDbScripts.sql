-- ============================================================
-- DATABASE  : TM_VGN_LOGIN_DB
-- PURPOSE   : Biometric Master – store employee fingerprint data
--             captured via eSSL / built-in sensors
-- ============================================================

USE TM_VGN_LOGIN_DB;
GO

-- ──────────────────────────────────────────────────────────────
-- 1.  TABLE  :  tbl_BiometricMaster
-- ──────────────────────────────────────────────────────────────
IF NOT EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'[dbo].[tbl_BiometricMaster]')
    AND   type      = N'U'
)
BEGIN
    CREATE TABLE [dbo].[tbl_BiometricMaster]
    (
        [BiometricId]      INT           IDENTITY(1,1) NOT NULL,
        [UserId]           NVARCHAR(50)  NOT NULL,
        [UserName]         NVARCHAR(200) NULL,
        [DepartmentId]     NVARCHAR(50)  NULL,
        [DepartmentName]   NVARCHAR(200) NULL,
        [FingerIndex]      INT           NOT NULL DEFAULT 0,   -- 0=Right Thumb, 1=Right Index…
        [FingerLabel]      NVARCHAR(50)  NULL,                 -- human-readable e.g. "Right Thumb"
        [FingerprintData]  NVARCHAR(MAX) NULL,                 -- Base-64 template (eSSL / WebAuthn)
        [ScanQuality]      INT           NULL,                 -- 0-100 quality score if available
        [DeviceType]       NVARCHAR(100) NULL,                 -- 'eSSL','WebAuthn-Platform','Other'
        [DeviceName]       NVARCHAR(200) NULL,
        [IsActive]         BIT           NOT NULL DEFAULT 1,
        [EnrolledOn]       DATETIME      NOT NULL DEFAULT GETDATE(),
        [EnrolledBy]       NVARCHAR(50)  NULL,
        [UpdatedOn]        DATETIME      NULL,
        [UpdatedBy]        NVARCHAR(50)  NULL,
        CONSTRAINT [PK_tbl_BiometricMaster] PRIMARY KEY CLUSTERED ([BiometricId] ASC)
    );

    -- Index to speed up per-user lookups
    CREATE NONCLUSTERED INDEX [IX_BiometricMaster_UserId]
    ON [dbo].[tbl_BiometricMaster] ([UserId]);

    PRINT 'Table [tbl_BiometricMaster] created successfully.';
END
ELSE
BEGIN
    PRINT 'Table [tbl_BiometricMaster] already exists – skipping creation.';
END
GO

-- ──────────────────────────────────────────────────────────────
-- 2.  STORED PROCEDURE  :  usp_BiometricMaster
--     Flags:
--       SAVE        – insert / update fingerprint record
--       LOAD_ALL    – load all enrolled records (admin)
--       LOAD_USER   – load records for a specific UserId
--       DELETE      – soft-delete a biometric record
--       CHECK       – check if a UserId already has a template
-- ──────────────────────────────────────────────────────────────
IF EXISTS (
    SELECT 1 FROM sys.objects
    WHERE object_id = OBJECT_ID(N'[dbo].[usp_BiometricMaster]')
    AND   type      IN (N'P', N'PC')
)
    DROP PROCEDURE [dbo].[usp_BiometricMaster];
GO

CREATE PROCEDURE [dbo].[usp_BiometricMaster]
(
    @Flag            NVARCHAR(50)   = NULL,
    @BiometricId     INT            = NULL,
    @UserId          NVARCHAR(50)   = NULL,
    @UserName        NVARCHAR(200)  = NULL,
    @DepartmentId    NVARCHAR(50)   = NULL,
    @DepartmentName  NVARCHAR(200)  = NULL,
    @FingerIndex     INT            = 0,
    @FingerLabel     NVARCHAR(50)   = NULL,
    @FingerprintData NVARCHAR(MAX)  = NULL,
    @ScanQuality     INT            = NULL,
    @DeviceType      NVARCHAR(100)  = NULL,
    @DeviceName      NVARCHAR(200)  = NULL,
    @EnrolledBy      NVARCHAR(50)   = NULL,
    @OutResult       INT            OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;
    SET @OutResult = 0;

    -- ── SAVE (Upsert) ─────────────────────────────────────────
    IF @Flag = 'SAVE'
    BEGIN
        -- Check for existing record with same UserId + FingerIndex
        IF EXISTS (
            SELECT 1 FROM [dbo].[tbl_BiometricMaster]
            WHERE  UserId      = @UserId
            AND    FingerIndex = @FingerIndex
            AND    IsActive    = 1
        )
        BEGIN
            -- UPDATE existing template
            UPDATE [dbo].[tbl_BiometricMaster]
            SET    UserName        = @UserName,
                   DepartmentId    = @DepartmentId,
                   DepartmentName  = @DepartmentName,
                   FingerLabel     = @FingerLabel,
                   FingerprintData = @FingerprintData,
                   ScanQuality     = @ScanQuality,
                   DeviceType      = @DeviceType,
                   DeviceName      = @DeviceName,
                   UpdatedOn       = GETDATE(),
                   UpdatedBy       = @EnrolledBy
            WHERE  UserId          = @UserId
            AND    FingerIndex     = @FingerIndex
            AND    IsActive        = 1;

            SET @OutResult = 2;  -- 2 = Updated
        END
        ELSE
        BEGIN
            -- INSERT new template
            INSERT INTO [dbo].[tbl_BiometricMaster]
            (UserId, UserName, DepartmentId, DepartmentName,
             FingerIndex, FingerLabel, FingerprintData,
             ScanQuality, DeviceType, DeviceName, EnrolledBy)
            VALUES
            (@UserId, @UserName, @DepartmentId, @DepartmentName,
             @FingerIndex, @FingerLabel, @FingerprintData,
             @ScanQuality, @DeviceType, @DeviceName, @EnrolledBy);

            SET @OutResult = 1;  -- 1 = Inserted
        END
    END

    -- ── LOAD_ALL ──────────────────────────────────────────────
    ELSE IF @Flag = 'LOAD_ALL'
    BEGIN
        SELECT
            B.BiometricId,
            B.UserId,
            B.UserName,
            B.DepartmentId,
            B.DepartmentName,
            B.FingerIndex,
            B.FingerLabel,
            B.ScanQuality,
            B.DeviceType,
            B.DeviceName,
            B.IsActive,
            CONVERT(VARCHAR(20), B.EnrolledOn, 106)  AS EnrolledOn,
            B.EnrolledBy,
            CONVERT(VARCHAR(20), B.UpdatedOn, 106)   AS UpdatedOn,
            B.UpdatedBy
        FROM   [dbo].[tbl_BiometricMaster] B
        WHERE  B.IsActive = 1
        ORDER  BY B.DepartmentName, B.UserName, B.FingerIndex;

        SET @OutResult = 1;
    END

    -- ── LOAD_USER ─────────────────────────────────────────────
    ELSE IF @Flag = 'LOAD_USER'
    BEGIN
        SELECT
            B.BiometricId,
            B.UserId,
            B.UserName,
            B.DepartmentId,
            B.DepartmentName,
            B.FingerIndex,
            B.FingerLabel,
            B.ScanQuality,
            B.DeviceType,
            B.DeviceName,
            B.IsActive,
            CONVERT(VARCHAR(20), B.EnrolledOn, 106) AS EnrolledOn,
            B.EnrolledBy
        FROM   [dbo].[tbl_BiometricMaster] B
        WHERE  B.UserId   = @UserId
        AND    B.IsActive  = 1
        ORDER  BY B.FingerIndex;

        SET @OutResult = 1;
    END

    -- ── DELETE (soft delete) ──────────────────────────────────
    ELSE IF @Flag = 'DELETE'
    BEGIN
        UPDATE [dbo].[tbl_BiometricMaster]
        SET    IsActive  = 0,
               UpdatedOn = GETDATE(),
               UpdatedBy = @EnrolledBy
        WHERE  BiometricId = @BiometricId;

        SET @OutResult = 1;
    END

    -- ── CHECK ─────────────────────────────────────────────────
    ELSE IF @Flag = 'CHECK'
    BEGIN
        SELECT COUNT(*) AS EnrolledCount
        FROM   [dbo].[tbl_BiometricMaster]
        WHERE  UserId   = @UserId
        AND    IsActive = 1;

        SET @OutResult = 1;
    END

    ELSE
    BEGIN
        SET @OutResult = -1;  -- Unknown flag
    END
END
GO

PRINT 'Stored Procedure [usp_BiometricMaster] created successfully.';
GO
