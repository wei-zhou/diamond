CREATE TABLE [dbo].[tcb_DimEmployee]
(
    [id] INT IDENTITY NOT NULL,
    [name] NVARCHAR(1024),
    [created] DATETIMEOFFSET NOT NULL, 
    [created_by] NVARCHAR(1024) NOT NULL, 
    [modified] DATETIMEOFFSET NOT NULL, 
    [modified_by] NVARCHAR(1024) NOT NULL, 
    CONSTRAINT [pk_tcb_DimEmployee] PRIMARY KEY ([id]), 
)

GO

CREATE UNIQUE INDEX [uk_tcb_DimEmployee_name] ON [dbo].[tcb_DimEmployee] ([name])
