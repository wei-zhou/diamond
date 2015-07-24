CREATE TABLE [dbo].[tc_employee]
(
    [id] UNIQUEIDENTIFIER NOT NULL, 
    [login_name] VARCHAR(64) NOT NULL, 
    [full_name] NVARCHAR(64) NOT NULL, 
    [password] VARCHAR(64) NOT NULL, 
    [role] TINYINT NOT NULL, 
    [is_locked] BIT NOT NULL, 
    [lock_time] DATETIMEOFFSET NULL, 
    [created] DATETIMEOFFSET NOT NULL, 
    [created_by] NVARCHAR(64) NOT NULL, 
    [modified] DATETIMEOFFSET NOT NULL, 
    [modified_by] NVARCHAR(64) NOT NULL, 
    [row_version] TIMESTAMP NOT NULL, 
    CONSTRAINT [pk_tc_employee] PRIMARY KEY ([id]) 
)

GO

CREATE UNIQUE INDEX [uk_tc_employee_login_name] ON [dbo].[tc_employee] ([login_name])

GO

CREATE UNIQUE INDEX [uk_tc_employee_full_name] ON [dbo].[tc_employee] ([full_name])
