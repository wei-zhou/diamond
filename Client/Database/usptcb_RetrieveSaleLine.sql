CREATE PROCEDURE [dbo].[usptcb_RetrieveSaleLine]
    @start DATE,
    @end DATE
AS
    DECLARE @start_date DATETIMEOFFSET,
            @end_date DATETIMEOFFSET
    SET @start_date = dbo.uftcb_ToDateTimeOffset(@start)
    SET @end_date = dbo.uftcb_ToDateTimeOffset(@end)

    DECLARE @source TABLE (
        [sale_id] UNIQUEIDENTIFIER, 
        [sale_number] NVARCHAR(1024), 
        [sale_day_number] INT, 
        [sale_total_number] INT, 
        [sale_sales_person] NVARCHAR(1024), 
        [customer_name] NVARCHAR(1024), 
        [sale_status] INT, 
        [sale_clerk] NVARCHAR(1024), 
        [sale_date] DATE,
        [line_id] UNIQUEIDENTIFIER, 
        [product_name] NVARCHAR(1024), 
        [product_description] NVARCHAR(1024), 
        [product_detail] XML, 
        [line_unit_price] MONEY, 
        [line_quantity] INT, 
        [line_status] INT 
    )

    INSERT @source
    SELECT
        p.id AS sale_id,
        p.sale_number AS sale_number,
        p.sale_day_number AS sale_day_number,
        p.sale_total_number AS sale_total_number,
        p.sales_person_name AS sale_sales_person,
        p.customer_name,
        p.[status] AS sale_status,
        p.created_by AS sale_clerk,
        SWITCHOFFSET(p.created, '+08:00') AS sale_date,
        t.id AS line_id, 
        t.product_name, 
        t.product_description, 
        t.product_detail, 
        t.unit_price AS line_unit_price, 
        t.quantity AS line_quantity, 
        t.[status] AS line_status
    FROM tc_sale_header AS p (NOLOCK)
        INNER JOIN tc_sale_line AS t (NOLOCK) ON p.id = t.sale_id
    WHERE p.created >= @start_date 
      AND p.created < @end_date 
      AND p.[status] = 1
      AND t.[status] = 1

    DECLARE @result TABLE (
        [sale_id] UNIQUEIDENTIFIER, 
        [sale_number] NVARCHAR(1024), 
        [sale_day_number] INT, 
        [sale_total_number] INT, 
        [sale_sales_person] NVARCHAR(1024), 
        [customer_name] NVARCHAR(1024), 
        [sale_status] INT, 
        [sale_clerk] NVARCHAR(1024), 
        [sale_date] DATETIME,
        [line_id] UNIQUEIDENTIFIER, 
        [product_name] NVARCHAR(1024), 
        [product_description] NVARCHAR(1024), 
        [product_detail] XML, 
        [line_unit_price] MONEY, 
        [line_status] INT 
    )
    DECLARE @sale_id UNIQUEIDENTIFIER, 
            @sale_number NVARCHAR(1024), 
            @sale_day_number INT, 
            @sale_total_number INT, 
            @sale_sales_person NVARCHAR(1024), 
            @customer_name NVARCHAR(1024), 
            @sale_status INT, 
            @sale_clerk NVARCHAR(1024), 
            @sale_date DATE,
            @line_id UNIQUEIDENTIFIER, 
            @product_name NVARCHAR(1024), 
            @product_description NVARCHAR(1024), 
            @product_detail XML, 
            @line_unit_price MONEY, 
            @line_quantity INT, 
            @line_status INT 
    DECLARE source_cursor CURSOR FOR 
    SELECT * FROM @source

    OPEN source_cursor

    FETCH NEXT FROM source_cursor 
    INTO @sale_id, 
         @sale_number, 
         @sale_day_number, 
         @sale_total_number, 
         @sale_sales_person, 
         @customer_name, 
         @sale_status, 
         @sale_clerk, 
         @sale_date, 
         @line_id, 
         @product_name, 
         @product_description, 
         @product_detail, 
         @line_unit_price, 
         @line_quantity, 
         @line_status

    WHILE @@FETCH_STATUS = 0
    BEGIN
        DECLARE @index INT
        SET @index = @line_quantity

        WHILE @index > 0
        BEGIN
            INSERT @result VALUES (
                @sale_id, 
                @sale_number, 
                @sale_day_number, 
                @sale_total_number, 
                @sale_sales_person, 
                @customer_name, 
                @sale_status, 
                @sale_clerk, 
                @sale_date, 
                @line_id, 
                @product_name, 
                @product_description, 
                @product_detail, 
                @line_unit_price, 
                @line_status
            )

            SET @index -= 1
        END

        FETCH NEXT FROM source_cursor 
        INTO @sale_id, 
             @sale_number, 
             @sale_day_number, 
             @sale_total_number, 
             @sale_sales_person, 
             @customer_name, 
             @sale_status, 
             @sale_clerk, 
             @sale_date, 
             @line_id, 
             @product_name, 
             @product_description, 
             @product_detail, 
             @line_unit_price, 
             @line_quantity, 
             @line_status
    END

    CLOSE source_cursor
    DEALLOCATE source_cursor

    SELECT 
        *,
        SYSDATETIMEOFFSET() AS created,
        SUSER_SNAME() AS created_by,
        SYSDATETIMEOFFSET() AS modified,
        SUSER_SNAME() AS modified_by
    FROM @result
RETURN 0
