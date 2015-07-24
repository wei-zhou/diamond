CREATE PROCEDURE [dbo].[usptcb_CreateSaleLine]
    @sale_id UNIQUEIDENTIFIER,
    @sale_number NVARCHAR(1024),
    @sale_day_number INT,
    @sale_total_number INT,
    @sale_sales_person NVARCHAR(1024),
    @customer_name NVARCHAR(1024),
    @sale_status INT,
    @sale_clerk NVARCHAR(1024),
    @sale_date DATETIME,
    @line_id UNIQUEIDENTIFIER, 
    @product_name NVARCHAR(1024), 
    @product_description NVARCHAR(1024), 
    @product_detail XML, 
    @line_unit_price MONEY, 
    @line_status INT
AS
    INSERT tcb_FactSaleLine
    (
        sale_id,
        sale_number,
        sale_day_number,
        sale_total_number,
        sale_sales_person_id,
        customer_name,
        sale_status_id,
        sale_clerk_id,
        sale_date,
        line_id,
        product_id,
        product_description,
        product_detail,
        line_unit_price,
        line_status_id,
        created,
        created_by,
        modified,
        modified_by
    )
    VALUES
    (
        @sale_id,
        @sale_number,
        @sale_day_number,
        @sale_total_number,
        (SELECT id FROM tcb_DimEmployee WHERE name = @sale_sales_person),
        @customer_name,
        (SELECT id FROM tcb_DimSaleStatus WHERE [value] = @sale_status),
        (SELECT id FROM tcb_DimEmployee WHERE name = @sale_clerk),
        @sale_date,
        @line_id, 
        (SELECT id FROM tcb_DimProduct WHERE name = @product_name),
        @product_description, 
        @product_detail, 
        @line_unit_price, 
        (SELECT id FROM tcb_DimSaleStatus WHERE [value] = @line_status),
        SYSDATETIMEOFFSET(),
        SUSER_SNAME(),
        SYSDATETIMEOFFSET(),
        SUSER_SNAME()
    )
RETURN 0
