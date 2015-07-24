CREATE PROCEDURE [dbo].[usptcb_CreateSaleStatus]
    @status INT
AS
    IF NOT EXISTS (SELECT 1 FROM tcb_DimSaleStatus WHERE [value] = @status)
    BEGIN
        INSERT tcb_DimSaleStatus 
        (
            value, 
            chinese_text, 
            created, 
            created_by, 
            modified, 
            modified_by
        )
        VALUES
        (
            @status,
            CASE
                WHEN @status = 1 THEN N'正常'
                WHEN @status = 2 THEN N'错误'
                WHEN @status = 3 THEN N'退货'
                ELSE NULL
            END,
            SYSDATETIMEOFFSET(),
            SUSER_SNAME(),
            SYSDATETIMEOFFSET(),
            SUSER_SNAME()
        )
    END
RETURN 0
