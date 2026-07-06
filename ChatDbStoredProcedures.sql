-- Run this script against TM_VGN_LOGIN_DB

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ChatMessages')
BEGIN
    CREATE TABLE ChatMessages (
        MessageId INT IDENTITY(1,1) PRIMARY KEY,
        SenderId VARCHAR(100) NOT NULL,
        ReceiverId VARCHAR(100) NOT NULL,
        Message NVARCHAR(MAX) NOT NULL,
        SentAt DATETIME NOT NULL DEFAULT GETDATE(),
        IsRead BIT NOT NULL DEFAULT 0
    );
END
GO

CREATE OR ALTER PROCEDURE usp_InitChatDatabase
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ChatMessages')
    BEGIN
        CREATE TABLE ChatMessages (
            MessageId INT IDENTITY(1,1) PRIMARY KEY,
            SenderId VARCHAR(100) NOT NULL,
            ReceiverId VARCHAR(100) NOT NULL,
            Message NVARCHAR(MAX) NOT NULL,
            SentAt DATETIME NOT NULL DEFAULT GETDATE(),
            IsRead BIT NOT NULL DEFAULT 0
        );
    END
END
GO

CREATE OR ALTER PROCEDURE usp_SaveChatMessage
    @SenderId VARCHAR(100),
    @ReceiverId VARCHAR(100),
    @Message NVARCHAR(MAX),
    @SentAt DATETIME
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO ChatMessages (SenderId, ReceiverId, Message, SentAt, IsRead)
    VALUES (@SenderId, @ReceiverId, @Message, @SentAt, 0);
END
GO

CREATE OR ALTER PROCEDURE usp_MarkMessagesAsRead
    @SenderId VARCHAR(100),
    @ReceiverId VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE ChatMessages 
    SET IsRead = 1 
    WHERE SenderId = @SenderId 
      AND ReceiverId = @ReceiverId 
      AND IsRead = 0;
END
GO

CREATE OR ALTER PROCEDURE usp_GetChatMessages
    @UserId1 VARCHAR(100),
    @UserId2 VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * 
    FROM ChatMessages 
    WHERE (SenderId = @UserId1 AND ReceiverId = @UserId2)
       OR (SenderId = @UserId2 AND ReceiverId = @UserId1)
    ORDER BY SentAt ASC;
END
GO

CREATE OR ALTER PROCEDURE usp_GetOnlineUsersForChat
    @CurrentUserId VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        u.UserId, 
        MAX(u.UserName) AS UserName,
        ISNULL((
            SELECT COUNT(*) 
            FROM ChatMessages c 
            WHERE c.SenderId = u.UserId 
              AND c.ReceiverId = @CurrentUserId 
              AND c.IsRead = 0
        ), 0) AS UnreadCount
    FROM tbl_UserSessionTracker u
    WHERE u.IsActive = 1 AND u.UserId != @CurrentUserId
    GROUP BY u.UserId;
END
GO
