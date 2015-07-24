CREATE TABLE [dbo].[t_security_user]
(
    [id] UNIQUEIDENTIFIER NOT NULL, 
    [key] NVARCHAR(1024) NOT NULL, 
    [name] NVARCHAR(1024) NOT NULL, 
    [ip] TINYINT NOT NULL, 
    [role] TINYINT NOT NULL, 
    [is_locked] BIT NOT NULL, 
    [lock_time_utc] SMALLDATETIME NULL, 
    [created] SMALLDATETIME NOT NULL, 
    [modified] SMALLDATETIME NOT NULL, 
    [row_version] TIMESTAMP NOT NULL, 
    CONSTRAINT [pk_t_security_user] PRIMARY KEY NONCLUSTERED ([id]) 
)

GO

CREATE UNIQUE CLUSTERED INDEX [ux_t_security_user_key_ip] ON [dbo].[t_security_user] ([key], [ip])

GO

CREATE UNIQUE INDEX [ux_t_security_user_name_ip] ON [dbo].[t_security_user] ([name], [ip])
