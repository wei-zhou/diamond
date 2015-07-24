CREATE PROCEDURE [dbo].[usp_security_create_or_update_user]
    @id UNIQUEIDENTIFIER,
    @key NVARCHAR(1024),
    @name NVARCHAR(1024),
    @ip TINYINT,
    @role TINYINT,
    @is_locked BIT = 0,
    @lock_time_utc SMALLDATETIME = NULL
AS
    DECLARE @now SMALLDATETIME
    SET @now = GETUTCDATE()

    IF NOT EXISTS (SELECT 1 FROM t_security_user WHERE [id] = @id)
    BEGIN
        INSERT t_security_user ([id], [key], [name], [ip], [role], [is_locked], [lock_time_utc], [created], [modified])
        VALUES (@id, @key, @name, @ip, @role, @is_locked, @lock_time_utc, @now, @now)
    END
    ELSE
    BEGIN
        UPDATE t_security_user SET [key] = @key, [name] = @name, [ip] = @ip, [role] = @role, [is_locked] = @is_locked, [lock_time_utc] = @lock_time_utc, [modified] = @now
        WHERE [id] = @id AND ([key] <> @key OR [name] <> @name OR [ip] <> @ip OR [role] <> @role OR [is_locked] <> @is_locked OR [lock_time_utc] <> @lock_time_utc)
    END
RETURN 0
