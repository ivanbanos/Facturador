﻿CREATE TABLE [dbo].[Usuario]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
    [Guid] UNIQUEIDENTIFIER NOT NULL,
    [Nombre] NVARCHAR(50) NOT NULL,
    [Usuario] NVARCHAR(50) NOT NULL,
    [Contrasena] NVARCHAR(MAX) NOT NULL,
    [IdentificacionTercero] NVARCHAR(250) NULL,
    -- Perfil de los usuarios sin cliente: NULL = Administrador, 'Operador' = sin gestion de usuarios
    [Rol] NVARCHAR(30) NULL,
    [Activo] BIT NOT NULL CONSTRAINT [DF_Usuario_Activo] DEFAULT (1),
    [Eliminado] BIT NOT NULL CONSTRAINT [DF_Usuario_Eliminado] DEFAULT (0),
    [FechaEliminacion] DATETIME NULL
)
