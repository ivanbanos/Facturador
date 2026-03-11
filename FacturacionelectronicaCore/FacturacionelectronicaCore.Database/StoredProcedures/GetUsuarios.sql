CREATE PROCEDURE [dbo].[GetUsuarios]
AS
BEGIN
	SELECT [Guid], [Nombre], [Usuario], [Contrasena] 
	FROM [dbo].Usuario
END
