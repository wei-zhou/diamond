CREATE TABLE [dbo].[tc_product_diamond_import]
(
    [id] UNIQUEIDENTIFIER NOT NULL, 
    [count] INT NOT NULL,
    [created] DATETIMEOFFSET NOT NULL, 
    [created_by] NVARCHAR(64) NOT NULL, 
    [modified] DATETIMEOFFSET NOT NULL, 
    [modified_by] NVARCHAR(64) NOT NULL, 
    [row_version] TIMESTAMP NOT NULL, 
    CONSTRAINT [pk_tc_product_diamond_import] PRIMARY KEY ([id])
)
