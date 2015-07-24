﻿CREATE FUNCTION [dbo].[uftcb_ToDateTimeOffset]
(
    @date DATE
)
RETURNS DATETIMEOFFSET
AS
BEGIN
    RETURN TODATETIMEOFFSET(@date, '+08:00')
END
