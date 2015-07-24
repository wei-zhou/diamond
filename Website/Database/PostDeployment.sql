/*
Post-Deployment Script Template                            
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.        
 Use SQLCMD syntax to include a file in the post-deployment script.            
 Example:      :r .\myfile.sql                                
 Use SQLCMD syntax to reference a variable in the post-deployment script.        
 Example:      :setvar TableName MyTable                            
               SELECT * FROM [$(TableName)]                    
--------------------------------------------------------------------------------------
*/

BEGIN TRY
    BEGIN TRANSACTION

    EXEC usp_security_init_user

    COMMIT
END TRY
BEGIN CATCH
    DECLARE @error INT, @message VARCHAR(4000)
    SELECT @error = ERROR_NUMBER(), @message = ERROR_MESSAGE()
    ROLLBACK
    RAISERROR ('Post deployment SQL error: %d: %s', 16, 1, @error, @message)
END CATCH
