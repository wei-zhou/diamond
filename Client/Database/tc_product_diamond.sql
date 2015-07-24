CREATE TABLE [dbo].[tc_product_diamond]
(
    [id] UNIQUEIDENTIFIER NOT NULL, 
    [report_type] TINYINT NULL, 
    [report_number] NVARCHAR(128) NULL, 
    [caret] DECIMAL NULL, 
    [clarity] TINYINT NULL, 
    [color] TINYINT NULL, 
    [cut] TINYINT NULL, 
    [polish] TINYINT NULL, 
    [symmetry] TINYINT NULL, 
    [fluorescence] NVARCHAR(128) NULL, 
    [comment] NVARCHAR(512) NULL, 
    [index] INT NOT NULL,
    [import_id] UNIQUEIDENTIFIER NOT NULL,
    [vendor] NVARCHAR(64) NULL,
    [status] TINYINT NOT NULL, 
    [cost] MONEY NOT NULL, 
    [sale_price] MONEY NOT NULL, 
    [created] DATETIMEOFFSET NOT NULL, 
    [created_by] NVARCHAR(64) NOT NULL, 
    [modified] DATETIMEOFFSET NOT NULL, 
    [modified_by] NVARCHAR(64) NOT NULL, 
    [row_version] TIMESTAMP NOT NULL, 
    CONSTRAINT [pk_tc_product_diamond] PRIMARY KEY ([id]), 
    CONSTRAINT [fk_tc_product_diamond_import] FOREIGN KEY ([import_id]) REFERENCES [tc_product_diamond_import]([id])
)

GO

CREATE INDEX [uk_tc_product_diamond_index] ON [dbo].[tc_product_diamond] ([index])
