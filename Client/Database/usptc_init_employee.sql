CREATE PROCEDURE [dbo].[usptc_init_employee]
AS
    DECLARE @id UNIQUEIDENTIFIER

    SET @id = CONVERT(UNIQUEIDENTIFIER, '802E99E5-CF53-42BC-94E1-FE4028EF2C9F')
    EXEC usptc_create_or_update_employee
         @id = @id,
         @login_name = 'weizhou',
         @full_name = N'周巍',
         @password = 'zss@140926_zw',
         @role = 3

    SET @id = CONVERT(UNIQUEIDENTIFIER, '07E5153A-02F7-496C-93A2-C3726917B914')
    EXEC usptc_create_or_update_employee
         @id = @id,
         @login_name = 'lvleiyan',
         @full_name = N'吕雷燕',
         @password = 'lly140926zss',
         @role = 2

    SET @id = CONVERT(UNIQUEIDENTIFIER, '2932D778-CBE8-4E25-9F6A-644AC4853E1A')
    EXEC usptc_create_or_update_employee
         @id = @id,
         @login_name = 'jiyanqin',
         @full_name = N'季艳琴',
         @password = 'jyq123abc',
         @role = 1

    SET @id = CONVERT(UNIQUEIDENTIFIER, 'AFAD81F0-43DE-4AA6-8D73-BE67CE703BE8')
    EXEC usptc_create_or_update_employee
         @id = @id,
         @login_name = 'guyanpin',
         @full_name = N'顾雅萍',
         @password = 'gyp123abc',
         @role = 1
RETURN 0
