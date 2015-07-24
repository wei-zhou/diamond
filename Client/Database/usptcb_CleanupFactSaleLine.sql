CREATE PROCEDURE [dbo].[usptcb_CleanupFactSaleLine]
AS
    TRUNCATE TABLE tcb_FactSaleLine
RETURN 0
