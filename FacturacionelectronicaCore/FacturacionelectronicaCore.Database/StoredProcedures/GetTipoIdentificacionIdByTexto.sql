CREATE PROCEDURE [dbo].[GetTipoIdentificacionIdByTexto]
	@Texto nvarchar(200)
AS
BEGIN
	DECLARE @TipoIdentificacionID int
	select @TipoIdentificacionID = [Id]
	from [TipoIdentificacion]
	where Texto = @Texto
	If @TipoIdentificacionID is null
	Begin
	Insert into [TipoIdentificacion] ([Guid],Texto)
	values(NewID(), @Texto)

	select @TipoIdentificacionID = @@IDENTITY
	End
	SELECT @TipoIdentificacionID as TipoIdentificacionID
END
