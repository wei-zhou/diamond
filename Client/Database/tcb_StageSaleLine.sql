CREATE TABLE [dbo].[tcb_StageSaleLine]
(
    [id] INT NOT NULL IDENTITY, 
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
    [line_status] INT, 
    [created] DATETIMEOFFSET NOT NULL, 
    [created_by] NVARCHAR(1024) NOT NULL, 
    [modified] DATETIMEOFFSET NOT NULL, 
    [modified_by] NVARCHAR(1024) NOT NULL, 
    CONSTRAINT [pk_tcb_StageSaleLine] PRIMARY KEY ([id]), 
)

GO
