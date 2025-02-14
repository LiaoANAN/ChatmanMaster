CREATE TABLE BAS.UserInfo( /*用戶資料*/
 UserId						int					identity,
 UserCode					nvarchar(MAX)		NOT NULL,						--使用者辨別碼
 Email						nvarchar(45)		NOT NULL,						--信箱
 Password					nvarchar(45)		NOT NULL,						--密碼
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