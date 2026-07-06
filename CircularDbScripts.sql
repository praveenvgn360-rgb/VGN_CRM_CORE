-- Run this script against TM_VGN_LOGIN_DB

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Tbl_Circulars')
BEGIN
    CREATE TABLE Tbl_Circulars (
        CircularId INT IDENTITY(1,1) PRIMARY KEY,
        Title NVARCHAR(200) NOT NULL,
        Description NVARCHAR(MAX) NOT NULL,
        PostedBy VARCHAR(100) NOT NULL,
        PostedDate DATETIME NOT NULL DEFAULT GETDATE(),
        ExpiryDate DATETIME NULL,
        IsActive BIT NOT NULL DEFAULT 1
    );
END
ELSE
BEGIN
    IF NOT EXISTS (
        SELECT * FROM sys.columns 
        WHERE object_id = OBJECT_ID('Tbl_Circulars') AND name = 'ExpiryDate'
    )
    BEGIN
        ALTER TABLE Tbl_Circulars ADD ExpiryDate DATETIME NULL;
    END
END
GO

CREATE OR ALTER PROCEDURE usp_GetCirculars
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        CircularId,
        Title,
        Description,
        PostedBy,
        PostedDate,
        ExpiryDate
    FROM Tbl_Circulars
    WHERE IsActive = 1 
      AND (ExpiryDate IS NULL OR ExpiryDate >= CAST(GETDATE() AS DATE))
    ORDER BY PostedDate DESC;
END
GO

CREATE OR ALTER PROCEDURE usp_AddCircular
    @Title NVARCHAR(200),
    @Description NVARCHAR(MAX),
    @PostedBy VARCHAR(100),
    @ExpiryDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Tbl_Circulars (Title, Description, PostedBy, PostedDate, ExpiryDate, IsActive)
    VALUES (@Title, @Description, @PostedBy, GETDATE(), @ExpiryDate, 1);
END
GO
