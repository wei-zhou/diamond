CREATE PROCEDURE [dbo].[usptc_create_or_update_employee]
    @id UNIQUEIDENTIFIER, 
    @login_name VARCHAR(64), 
    @full_name NVARCHAR(64), 
    @password VARCHAR(64), 
    @role TINYINT, 
    @is_locked BIT = 0, 
    @lock_time DATETIMEOFFSET = NULL
AS
    DECLARE @now DATETIMEOFFSET
    SET @now = GETUTCDATE()
    DECLARE @user NVARCHAR(64)
    SET @user = N'system'

    IF NOT EXISTS (SELECT 1 FROM tc_employee WHERE [id] = @id)
    BEGIN
        INSERT tc_employee ([id], [login_name], [full_name], [password], [role], [is_locked], [lock_time], [created], [created_by], [modified], [modified_by])
        VALUES (@id, @login_name, @full_name, @password, @role, @is_locked, @lock_time, @now, @user, @now, @user)
    END
    ELSE
    BEGIN
        UPDATE tc_employee SET [login_name] = @login_name, [full_name] = @full_name, [password] = @password, [role] = @role, [is_locked] = @is_locked, [lock_time] = @lock_time, [modified] = @now, [modified_by] = @user
        WHERE [id] = @id AND ([login_name] <> @login_name OR [full_name] <> @full_name OR [password] <> @password OR [role] <> @role OR [is_locked] <> @is_locked OR [lock_time] <> @lock_time)
    END
RETURN 0
