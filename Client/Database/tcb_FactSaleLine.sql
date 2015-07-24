CREATE TABLE [dbo].[tcb_FactSaleLine]
(
    [id] INT NOT NULL IDENTITY, 
    [sale_id] UNIQUEIDENTIFIER, 
    [sale_number] NVARCHAR(1024), 
    [sale_day_number] INT, 
    [sale_total_number] INT, 
    [sale_sales_person_id] INT, 
    [customer_name] NVARCHAR(1024), 
    [sale_status_id] INT, 
    [sale_clerk_id] INT, 
    [sale_date] DATETIME,
    [line_id] UNIQUEIDENTIFIER, 
    [product_id] INT, 
    [product_description] NVARCHAR(1024), 
    [product_detail] XML, 
    [line_unit_price] MONEY, 
    [line_status_id] INT, 
    [created] DATETIMEOFFSET NOT NULL, 
    [created_by] NVARCHAR(1024) NOT NULL, 
    [modified] DATETIMEOFFSET NOT NULL, 
    [modified_by] NVARCHAR(1024) NOT NULL, 
    CONSTRAINT [pk_tcb_FactSaleLine] PRIMARY KEY ([id]), 
    CONSTRAINT [fk_tcb_FactSaleLine_sales_person] FOREIGN KEY ([sale_sales_person_id]) REFERENCES [tcb_DimEmployee]([id]), 
    CONSTRAINT [fk_tcb_FactSaleLine_clerk] FOREIGN KEY ([sale_clerk_id]) REFERENCES [tcb_DimEmployee]([id]), 
    CONSTRAINT [fk_tcb_FactSaleLine_sale_status] FOREIGN KEY ([sale_status_id]) REFERENCES [tcb_DimSaleStatus]([id]), 
    CONSTRAINT [fk_tcb_FactSaleLine_line_status] FOREIGN KEY ([line_status_id]) REFERENCES [tcb_DimSaleStatus]([id]), 
    CONSTRAINT [fk_tcb_FactSaleLine_product] FOREIGN KEY ([product_id]) REFERENCES [tcb_DimProduct]([id]), 
    CONSTRAINT [fk_tcb_FactSaleLine_sale_date] FOREIGN KEY ([sale_date]) REFERENCES [tcb_DimDate]([date])
)

GO
