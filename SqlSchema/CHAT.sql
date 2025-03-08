-- 創建 CHAT Schema
CREATE SCHEMA CHAT;
GO

-- 聊天訊息表
CREATE TABLE CHAT.Message (
    MessageId           BIGINT          IDENTITY,                      -- 訊息ID
    SenderId           INT             NOT NULL,                      -- 發送者ID
    ReceiverId         INT             NOT NULL,                      -- 接收者ID
    MessageType        NVARCHAR(10)    NOT NULL,                      -- 訊息類型(text, image, file, etc)
    Content           NVARCHAR(MAX)   NOT NULL,                      -- 訊息內容
    FileName          NVARCHAR(255)           NULL,                  -- 檔案名稱  
	FileSize				INT           NULL,                  -- 檔案大小
    MediaUrl          NVARCHAR(500)       NULL,                      -- 媒體文件URL
    IsRead            BIT             NOT NULL DEFAULT 0,            -- 是否已讀
    IsDelete          BIT             NOT NULL DEFAULT 0,            -- 是否已刪除
    Status            NVARCHAR(1)     NOT NULL,                      -- 狀態(A:正常 R:已收回)
    CreateDate        DATETIME        NOT NULL DEFAULT GETDATE(),    -- 發送時間
    UpdateDate        DATETIME        NOT NULL DEFAULT GETDATE(),    -- 更改時間
    CONSTRAINT PK_Message PRIMARY KEY (MessageId),
    CONSTRAINT FK_Message_Sender FOREIGN KEY (SenderId) REFERENCES BAS.UserInfo(UserId),
    CONSTRAINT FK_Message_Receiver FOREIGN KEY (ReceiverId) REFERENCES BAS.UserInfo(UserId)
);

-- 建立索引
CREATE INDEX IX_Message_Sender ON CHAT.Message(SenderId, CreateDate);
CREATE INDEX IX_Message_Receiver ON CHAT.Message(ReceiverId, CreateDate);

-- 群組訊息表
CREATE TABLE CHAT.GroupMessage (
    GroupMessageId     BIGINT          IDENTITY,                      -- 群組訊息ID
    GroupId           INT             NOT NULL,                      -- 群組ID
    SenderId          INT             NOT NULL,                      -- 發送者ID
    MessageType       NVARCHAR(10)    NOT NULL,                      -- 訊息類型
    Content           NVARCHAR(MAX)   NOT NULL,                      -- 訊息內容
    MediaUrl          NVARCHAR(500)       NULL,                      -- 媒體文件URL
    IsDeleted         BIT             NOT NULL DEFAULT 0,            -- 是否已刪除
    Status            NVARCHAR(1)     NOT NULL,                      -- 狀態(A:正常)
    CreateDate        DATETIME        NOT NULL DEFAULT GETDATE(),    -- 發送時間
    CONSTRAINT PK_GroupMessage PRIMARY KEY (GroupMessageId),
    CONSTRAINT FK_GroupMessage_Group FOREIGN KEY (GroupId) REFERENCES BAS.GroupInfo(GroupId),
    CONSTRAINT FK_GroupMessage_Sender FOREIGN KEY (SenderId) REFERENCES BAS.UserInfo(UserId)
);

-- 群組訊息讀取狀態表
CREATE TABLE CHAT.GroupMessageRead (
    GroupMessageReadId BIGINT          IDENTITY,                      -- ID
    GroupMessageId    BIGINT          NOT NULL,                      -- 群組訊息ID
    UserId           INT             NOT NULL,                      -- 用戶ID
    IsRead           BIT             NOT NULL DEFAULT 0,            -- 是否已讀
    ReadDate         DATETIME            NULL,                      -- 讀取時間
    CONSTRAINT PK_GroupMessageRead PRIMARY KEY (GroupMessageReadId),
    CONSTRAINT FK_GroupMessageRead_Message FOREIGN KEY (GroupMessageId) REFERENCES CHAT.GroupMessage(GroupMessageId),
    CONSTRAINT FK_GroupMessageRead_User FOREIGN KEY (UserId) REFERENCES BAS.UserInfo(UserId),
    CONSTRAINT AK_GroupMessageRead UNIQUE (GroupMessageId, UserId)
);

-- 建立索引
CREATE INDEX IX_GroupMessage_Group ON CHAT.GroupMessage(GroupId, CreateDate);
CREATE INDEX IX_GroupMessage_Sender ON CHAT.GroupMessage(SenderId, CreateDate);
CREATE INDEX IX_GroupMessageRead_Message ON CHAT.GroupMessageRead(GroupMessageId);
CREATE INDEX IX_GroupMessageRead_User ON CHAT.GroupMessageRead(UserId);