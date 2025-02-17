CREATE TABLE BAS.UserInfo( /*用戶資料*/
 UserId						int					identity,
 UserName					nvarchar(10)		NOT NULL,						--用戶暱稱
 Email						nvarchar(45)		NOT NULL,						--信箱
 Password					nvarchar(MAX)		NOT NULL,						--密碼
 Gender						nvarchar(1)				NULL,						--性別 M/F
 Birthday					datetime				NULL,						--生日(西元格式: 1911-01-01)
 [Status]					nvarchar(1)			NOT NULL,						--狀態碼
 CreateDate					datetime			NOT NULL	DEFAULT GETDATE(),	--新增日期
 UpdateDate					datetime				NULL,						--修改日期
 CreateUserId				int					NOT NULL,						--新增人員
 UpdateUserId				int						NULL,						--修改人員
 CONSTRAINT PK_UserInfo PRIMARY KEY (UserId),
 CONSTRAINT AK1_UserInfo UNIQUE (Email),
)

-- 好友關係表
CREATE TABLE BAS.FriendRelation (
    FriendRelationId     int             IDENTITY,
    UserId              int             NOT NULL,                         -- 用戶ID
    FriendId            int             NOT NULL,                         -- 好友ID
    Status              nvarchar(1)     NOT NULL,                         -- 狀態(P:待處理, A:已接受, R:已拒絕, B:已封鎖)
    CreateDate          datetime        NOT NULL    DEFAULT GETDATE(),    -- 新增日期
    UpdateDate          datetime            NULL,                         -- 修改日期
    CreateUserId        int             NOT NULL,                         -- 新增人員
    UpdateUserId        int                 NULL,                         -- 修改人員
    CONSTRAINT PK_FriendRelation PRIMARY KEY (FriendRelationId),
    CONSTRAINT FK_FriendRelation_User FOREIGN KEY (UserId) REFERENCES BAS.UserInfo(UserId),
    CONSTRAINT FK_FriendRelation_Friend FOREIGN KEY (FriendId) REFERENCES BAS.UserInfo(UserId),
    CONSTRAINT AK1_FriendRelation UNIQUE (UserId, FriendId)
);

-- 群組基本資料表
CREATE TABLE BAS.GroupInfo (
    GroupId             int             IDENTITY,
    GroupName           nvarchar(100)   NOT NULL,                         -- 群組名稱
    Description         nvarchar(500)       NULL,                         -- 群組描述
    Avatar             nvarchar(MAX)        NULL,                         -- 群組頭像
    MaxMembers         int             NOT NULL    DEFAULT 100,           -- 最大成員數
    Status             nvarchar(1)     NOT NULL,                         -- 狀態(A:正常, D:已解散)
    CreateDate         datetime        NOT NULL    DEFAULT GETDATE(),    -- 新增日期
    UpdateDate         datetime            NULL,                         -- 修改日期
    CreateUserId       int             NOT NULL,                         -- 新增人員(群組創建者)
    UpdateUserId       int                 NULL,                         -- 修改人員
    CONSTRAINT PK_GroupInfo PRIMARY KEY (GroupId)
);

-- 群組成員表
CREATE TABLE BAS.GroupMember (
    GroupMemberId       int             IDENTITY,
    GroupId            int             NOT NULL,                         -- 群組ID
    UserId             int             NOT NULL,                         -- 用戶ID
    Role               nvarchar(1)     NOT NULL,                         -- 角色(O:擁有者, A:管理員, M:成員)
    Status             nvarchar(1)     NOT NULL,                         -- 狀態(A:正常, L:已離開, K:被踢出)
    CreateDate         datetime        NOT NULL    DEFAULT GETDATE(),    -- 新增日期
    UpdateDate         datetime            NULL,                         -- 修改日期
    CreateUserId       int             NOT NULL,                         -- 新增人員
    UpdateUserId       int                 NULL,                         -- 修改人員
    CONSTRAINT PK_GroupMember PRIMARY KEY (GroupMemberId),
    CONSTRAINT FK_GroupMember_Group FOREIGN KEY (GroupId) REFERENCES BAS.GroupInfo(GroupId),
    CONSTRAINT FK_GroupMember_User FOREIGN KEY (UserId) REFERENCES BAS.UserInfo(UserId),
    CONSTRAINT AK1_GroupMember UNIQUE (GroupId, UserId)
);

-- 好友申請表
CREATE TABLE BAS.FriendRequest (
    FriendRequestId     int             IDENTITY,
    SenderId           int             NOT NULL,                         -- 發送者ID
    ReceiverId         int             NOT NULL,                         -- 接收者ID
    Message            nvarchar(200)       NULL,                         -- 申請訊息
    Status             nvarchar(1)     NOT NULL,                         -- 狀態(P:待處理, A:已接受, R:已拒絕)
    CreateDate         datetime        NOT NULL    DEFAULT GETDATE(),    -- 新增日期
    UpdateDate         datetime            NULL,                         -- 修改日期
    CONSTRAINT PK_FriendRequest PRIMARY KEY (FriendRequestId),
    CONSTRAINT FK_FriendRequest_Sender FOREIGN KEY (SenderId) REFERENCES BAS.UserInfo(UserId),
    CONSTRAINT FK_FriendRequest_Receiver FOREIGN KEY (ReceiverId) REFERENCES BAS.UserInfo(UserId)
);

-- 群組邀請表
CREATE TABLE BAS.GroupInvitation (
    GroupInvitationId   int             IDENTITY,
    GroupId            int             NOT NULL,                         -- 群組ID
    InviterId          int             NOT NULL,                         -- 邀請者ID
    InviteeId          int             NOT NULL,                         -- 被邀請者ID
    Message            nvarchar(200)       NULL,                         -- 邀請訊息
    Status             nvarchar(1)     NOT NULL,                         -- 狀態(P:待處理, A:已接受, R:已拒絕)
    CreateDate         datetime        NOT NULL    DEFAULT GETDATE(),    -- 新增日期
    UpdateDate         datetime            NULL,                         -- 修改日期
    CONSTRAINT PK_GroupInvitation PRIMARY KEY (GroupInvitationId),
    CONSTRAINT FK_GroupInvitation_Group FOREIGN KEY (GroupId) REFERENCES BAS.GroupInfo(GroupId),
    CONSTRAINT FK_GroupInvitation_Inviter FOREIGN KEY (InviterId) REFERENCES BAS.UserInfo(UserId),
    CONSTRAINT FK_GroupInvitation_Invitee FOREIGN KEY (InviteeId) REFERENCES BAS.UserInfo(UserId)
);