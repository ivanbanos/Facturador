CREATE PROCEDURE dbo.[SaveOrUpdateUser]
	@Usuario dbo.[UsuarioType] readonly
AS
	BEGIN
		MERGE [dbo].[Usuario] AS Target
		USING (select [Guid], Nombre, Usuario, Contrasena FROM @Usuario)
			AS Source ([Guid],Nombre,Usuario,Contrasena)
		ON (Source.[Guid] = Target.[Guid])
		WHEN MATCHED THEN
			UPDATE SET Target.Nombre = Source.Nombre,
			Target.Usuario = Source.Usuario,
			Target.Contrasena = Source.Contrasena
		WHEN NOT MATCHED BY TARGET THEN
			INSERT ([Guid], [Nombre], [Usuario], [Contrasena])
			VALUES(Source.[Guid], Source.Nombre, Source.Usuario, Source.Contrasena);
	END
