CREATE PROCEDURE [dbo].[usptcb_CleanupStageSaleLine]
AS
    TRUNCATE TABLE tcb_StageSaleLine
RETURN 0
