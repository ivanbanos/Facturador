CREATE TYPE [dbo].[UsuarioType]
	AS TABLE
	(
		Guid uniqueidentifier NOT NULL,

        Nombre varchar(50) NOT NULL,

        Usuario nvarchar(50) NOT NULL,

        Contrasena nvarchar(MAX) NOT NULL
	)
