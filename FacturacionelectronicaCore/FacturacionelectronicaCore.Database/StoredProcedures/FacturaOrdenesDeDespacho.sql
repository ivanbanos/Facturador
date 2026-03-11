CREATE PROCEDURE [dbo].[FacturaOrdenesDeDespacho]
	@OrdenesDeDespacho [dbo].[EntityTableType] READONLY
AS
BEGIN
	DECLARE @tmpOrdenesDeDespacho TABLE([Id] INT NOT NULL,
										[Guid] UNIQUEIDENTIFIER NOT NULL, 
										IdFactura INT NULL,
										IdTercero INT NOT NULL,
										Combustible NVARCHAR(50),
										Cantidad Decimal(18,3),
										Precio Decimal(18,3),
										Total Decimal(18,3) NOT NULL,
										IdInterno NVARCHAR(50),
										Placa NVARCHAR(50),
										Kilometraje Decimal(18,3),
										IdEstadoActual Int NOT NULL,
										Surtidor NVARCHAR(50),
										Cara NVARCHAR(50),
										Manguera NVARCHAR(50),
										[SubTotal] DECIMAL(18, 3) NULL, 
										[FechaProximoMantenimiento] DATETIME NULL, 
										[Vendedor] NVARCHAR(50) NULL);

    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT;
	DECLARE @IdResolucion INT, @Consecutivo INT, @FacturaId INT, @estadoId int;
	BEGIN TRY 
		BEGIN TRAN
			INSERT INTO @tmpOrdenesDeDespacho ([Id], [Guid], IdFactura, IdTercero, Combustible, Cantidad, Precio, Total, IdInterno, Placa, 
											   Kilometraje, IdEstadoActual, Surtidor, Cara, Manguera, SubTotal, FechaProximoMantenimiento, Vendedor)
			SELECT OrdenesDeDespacho.[Id], OrdenesDeDespacho.[Guid], OrdenesDeDespacho.IdFactura, OrdenesDeDespacho.IdTercero, OrdenesDeDespacho.Combustible,
				   OrdenesDeDespacho.Cantidad, OrdenesDeDespacho.Precio, OrdenesDeDespacho.Total, OrdenesDeDespacho.IdInterno, OrdenesDeDespacho.Placa,
				   OrdenesDeDespacho.Kilometraje, OrdenesDeDespacho.IdEstadoActual, OrdenesDeDespacho.Surtidor, OrdenesDeDespacho.Cara, OrdenesDeDespacho.Manguera,
				   OrdenesDeDespacho.SubTotal, OrdenesDeDespacho.FechaProximoMantenimiento, OrdenesDeDespacho.Vendedor
			FROM dbo.OrdenesDeDespacho
			JOIN @OrdenesDeDespacho AS OrdenesDeDespachoType
				ON OrdenesDeDespacho.[Guid] = OrdenesDeDespachoType.[Guid]

			IF EXISTS(SELECT 1 FROM @tmpOrdenesDeDespacho WHERE IdFactura IS NOT NULL)
			BEGIN
				RAISERROR ('Una o varias ordenes de despacho ya tiene factura asociada favor verificar.', 16, 1);
				RETURN;
			END

			IF EXISTS((SELECT COUNT(*) FROM @tmpOrdenesDeDespacho GROUP BY IdTercero Having count(*) > 2))
			BEGIN
				RAISERROR ('Las ordenes de despacho seleccionadas pertenecen a mas de un tercero favor verificar.', 16, 1);
				RETURN;
			END

			SELECT @estadoId = Id
			FROM [dbo].Estados
			WHERE Estados.Texto = 'Pendiente'

			SELECT @IdResolucion = Resolucion.Id, @Consecutivo = Resolucion.ConsecutivoActual
			FROM dbo.Resolucion
			JOIN dbo.Estados
				ON Resolucion.Id = Resolucion.Id
			WHERE Estados.Texto = 'Activo'

			INSERT INTO dbo.Facturas ([Guid], [Consecutivo], IdTercero, Combustible, Cantidad, Precio, Total, IdInterno, Placa, Kilometraje, IdResolucion, IdEstadoActual,
									  Surtidor, Cara, Manguera, SubTotal, FechaProximoMantenimiento, Vendedor)

			SELECT NEWID(), @Consecutivo, IdTercero, NULL, NULL, NULL, SUM(Total), NULL, NULL, NULL, @IdResolucion, @estadoId, NULL, NULL, NULL,
				   SUM(SubTotal), NULL, NULL
			FROM @tmpOrdenesDeDespacho
			GROUP BY IdTercero, Total, IdInterno, Placa, IdInterno, Placa, Kilometraje, IdEstadoActual, SubTotal

			SELECT @FacturaId = @@IDENTITY;

			UPDATE dbo.OrdenesDeDespacho 
			SET OrdenesDeDespacho.IdFactura = @FacturaId
			FROM dbo.OrdenesDeDespacho
			JOIN @OrdenesDeDespacho AS OrdenesDeDespachoType
				ON OrdenesDeDespacho.[Guid] = OrdenesDeDespachoType.[Guid]

			UPDATE dbo.Resolucion
			SET Resolucion.ConsecutivoActual += 1
			WHERE Resolucion.Id = @IdResolucion

			SELECT Facturas.[Guid], Facturas.[Consecutivo], Terceros.[Guid] AS IdTercero, Facturas.Combustible, Facturas.Cantidad, Facturas.Precio,
				   Facturas.Total, Facturas.IdInterno, Facturas.Placa, Facturas.Kilometraje, Resolucion.[Guid] AS IdResolucion, Estados.Id AS IdEstadoActual,
				   Facturas.Surtidor, Facturas.Cara, Facturas.Manguera, Facturas.SubTotal, Facturas.FechaProximoMantenimiento, Facturas.Vendedor
			FROM dbo.Facturas
			JOIN dbo.Terceros
				ON Terceros.Id = Facturas.IdTercero
			JOIN dbo.Resolucion
				ON Resolucion.Id = Facturas.IdResolucion
			JOIN dbo.Estados
				ON Estados.Id = Facturas.IdEstadoActual
			WHERE Facturas.Id = @FacturaId
		COMMIT TRAN
	END TRY  
	BEGIN CATCH  

		IF(@@TRANCOUNT > 0)
		BEGIN
			ROLLBACK TRAN
		END

		SELECT @ErrorMessage = ERROR_MESSAGE(),@ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();  
		RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);  
	END CATCH;	
END
