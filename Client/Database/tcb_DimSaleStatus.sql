CREATE TABLE [dbo].[tcb_DimSaleStatus]
(
    [id] INT IDENTITY NOT NULL,
    [value] INT,
    [chinese_text] NVARCHAR(1024),
    [created] DATETIMEOFFSET NOT NULL, 
    [created_by] NVARCHAR(1024) NOT NULL, 
    [modified] DATETIMEOFFSET NOT NULL, 
    [modified_by] NVARCHAR(1024) NOT NULL, 
    CONSTRAINT [pk_tcb_DimSaleStatus] PRIMARY KEY ([id]), 
)

GO

CREATE UNIQUE INDEX [uk_tcb_DimSaleStatus_value] ON [dbo].[tcb_DimSaleStatus] ([value])

GO

CREATE UNIQUE INDEX [uk_tcb_DimSaleStatus_chinese_text] ON [dbo].[tcb_DimSaleStatus] ([chinese_text])
