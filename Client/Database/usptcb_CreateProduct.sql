CREATE PROCEDURE [dbo].[usptcb_CreateProduct]
    @product_name NVARCHAR(1024)
AS
    IF NOT EXISTS (SELECT 1 FROM tcb_DimProduct WHERE [name] = @product_name)
    BEGIN
        INSERT tcb_DimProduct
        (
            name,
            created, 
            created_by, 
            modified, 
            modified_by
        )
        VALUES
        (
            @product_name,
            SYSDATETIMEOFFSET(),
            SUSER_SNAME(),
            SYSDATETIMEOFFSET(),
            SUSER_SNAME()
        )
    END
RETURN 0
