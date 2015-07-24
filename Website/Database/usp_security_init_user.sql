CREATE PROCEDURE [dbo].[usp_security_init_user]
AS
    DECLARE @id UNIQUEIDENTIFIER

    SET @id = CONVERT(UNIQUEIDENTIFIER, '797C7DBE-99F1-4A98-ABA4-5139F364A76D')
    EXEC usp_security_create_or_update_user
         @id = @id,
         @key = N'A721E517A50A4634881522AA9484815B',
         @name = N'周巍',
         @ip = 1,
         @role = 1

    --SET @id = CONVERT(UNIQUEIDENTIFIER, '4D868101-F0BC-4E68-B62E-B08C9DA01BC2')
    --EXEC usp_security_create_or_update_user
    --     @id = @id,
    --     @key = N'',
    --     @name = N'',
    --     @ip = 2,
    --     @role = 2

    --SET @id = CONVERT(UNIQUEIDENTIFIER, '3241DFC0-CAB2-41EE-8EA9-052C552D0B0A')
    --EXEC usp_security_create_or_update_user
    --     @id = @id,
    --     @key = N'',
    --     @name = N'',
    --     @ip = 3,
    --     @role = 2

RETURN 0
