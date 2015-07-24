CREATE TABLE [dbo].[tc_customer_contact]
(
    [id] UNIQUEIDENTIFIER NOT NULL, 
    [sale_id] UNIQUEIDENTIFIER NOT NULL, 
    [method] TINYINT NOT NULL, 
    [value] NVARCHAR(1024) NOT NULL, 
    [created] DATETIMEOFFSET NOT NULL, 
    [created_by] NVARCHAR(64) NOT NULL, 
    [modified] DATETIMEOFFSET NOT NULL, 
    [modified_by] NVARCHAR(64) NOT NULL, 
    [row_version] TIMESTAMP NOT NULL, 
    CONSTRAINT [pk_tc_customer_contact] PRIMARY KEY ([id]), 
    CONSTRAINT [fk_tc_customer_contact_sale_header] FOREIGN KEY ([sale_id]) REFERENCES [tc_sale_header]([id]) ON DELETE CASCADE
)
