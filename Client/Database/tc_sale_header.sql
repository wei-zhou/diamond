CREATE TABLE [dbo].[tc_sale_header]
(
    [id] UNIQUEIDENTIFIER NOT NULL, 
    [sale_number] VARCHAR(64) NOT NULL, 
    [sale_day_number] INT NOT NULL,
    [sale_total_number] INT NOT NULL,
    [sales_person_name] NVARCHAR(64) NOT NULL, 
    [customer_name] NVARCHAR(64) NOT NULL, 
    [status] TINYINT NOT NULL, 
    [created] DATETIMEOFFSET NOT NULL, 
    [created_by] NVARCHAR(64) NOT NULL, 
    [modified] DATETIMEOFFSET NOT NULL, 
    [modified_by] NVARCHAR(64) NOT NULL, 
    [row_version] TIMESTAMP NOT NULL, 
    CONSTRAINT [pk_tc_sale_header] PRIMARY KEY ([id]) 
)

GO


CREATE UNIQUE INDEX [uk_tc_sale_header_sale_total_number] ON [dbo].[tc_sale_header] ([sale_total_number])
