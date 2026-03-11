CREATE PROCEDURE [dbo].[AgregarOActualizarEstacion]
	@estaciones [dbo].[EstacionType] READONLY
AS
BEGIN
		DECLARE @estadoActivoId INT
			SELECT @estadoActivoId = Id
			FROM Estados
			WHERE Texto = 'Activo'

		MERGE [dbo].[Estaciones] AS TARGET
		USING (select [Guid],[linea1],[linea2],[linea3],[linea4],[Direccion],
						[Nit],[Nombre],[Razon],[Telefono] FROM @estaciones)
			AS Source ([Guid],[linea1],[linea2],[linea3],[linea4],[Direccion],
						[Nit],[Nombre],[Razon],[Telefono])
		ON (Source.[Guid] = Target.[Guid])
		WHEN MATCHED THEN
			UPDATE SET Target.[linea1] = Source.Nombre,
			Target.[linea2] = Source.[linea2],
			Target.[linea3] = Source.[linea3],
			Target.[linea4] = Source.[linea4],
			Target.[Direccion] = Source.[Direccion],
			Target.[Nit] = Source.[Nit],
			Target.[Nombre] = Source.[Nombre],
			Target.[Razon] = Source.[Razon],
			Target.[Telefono] = Source.[Telefono]
		WHEN NOT MATCHED BY TARGET THEN
			INSERT ([Guid],[linea1],[linea2],[linea3],[linea4],[Direccion],
						[Nit],[Nombre],[Razon],[Telefono],[IdEstadoActual])
			VALUES(newID(),Source.[linea1],Source.[linea2],Source.[linea3],Source.[linea4],Source.[Direccion],
					Source.[Nit],Source.[Nombre],Source.[Razon],Source.[Telefono], @estadoActivoId);
	END
