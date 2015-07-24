CREATE TABLE [dbo].[tcb_DimDate]
(
    [date] DATETIME NOT NULL,
    [date_name] NVARCHAR(1024),
    [year] DATETIME,
    [year_name] NVARCHAR(1024),
    [month] DATETIME,
    [month_name] NVARCHAR(1024),
    [day_of_year] INT,
    [day_of_year_name] NVARCHAR(1024),
    [day_of_month] INT,
    [day_of_month_name] NVARCHAR(1024),
    [month_of_year] INT,
    [month_of_year_name] NVARCHAR(1024),
    CONSTRAINT [pk_tcb_DimDate] PRIMARY KEY ([date])
)
