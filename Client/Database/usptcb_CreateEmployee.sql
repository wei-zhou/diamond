CREATE PROCEDURE [dbo].[usptcb_CreateEmployee]
    @employee_name NVARCHAR(1024)
AS
    IF NOT EXISTS (SELECT 1 FROM tcb_DimEmployee WHERE [name] = @employee_name)
    BEGIN
        INSERT tcb_DimEmployee
        (
            name,
            created, 
            created_by, 
            modified, 
            modified_by
        )
        VALUES
        (
            @employee_name,
            SYSDATETIMEOFFSET(),
            SUSER_SNAME(),
            SYSDATETIMEOFFSET(),
            SUSER_SNAME()
        )
    END
RETURN 0
