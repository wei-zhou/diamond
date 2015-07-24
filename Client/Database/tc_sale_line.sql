CREATE TABLE [dbo].[tc_sale_line]
(
    [id] UNIQUEIDENTIFIER NOT NULL, 
    [sale_id] UNIQUEIDENTIFIER NOT NULL, 
    [product_name] NVARCHAR(64) NOT NULL, 
    [product_description] NVARCHAR(1024) NULL, 
    [product_detail] XML([dbo].[xsdtc_dynamic_properties]) NULL, 
    [unit_price] MONEY NOT NULL, 
    [quantity] INT NOT NULL, 
    [status] TINYINT NOT NULL, 
    [created] DATETIMEOFFSET NOT NULL, 
    [created_by] NVARCHAR(64) NOT NULL, 
    [modified] DATETIMEOFFSET NOT NULL, 
    [modified_by] NVARCHAR(64) NOT NULL, 
    [row_version] TIMESTAMP NOT NULL, 
    CONSTRAINT [pk_tc_sale_line] PRIMARY KEY ([id]), 
    CONSTRAINT [fk_tc_sale_line_sale_header] FOREIGN KEY ([sale_id]) REFERENCES [tc_sale_header]([id]) ON DELETE CASCADE
)

GO

CREATE INDEX [uk_tc_sale_line_sale_id_product_name] ON [dbo].[tc_sale_line] ([sale_id], [product_name])
