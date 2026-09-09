USE [master]
GO

IF DB_ID(N'Strategic') IS NULL
BEGIN
    CREATE DATABASE [Strategic]
END
GO


USE [Strategic]
GO

CREATE TABLE [dbo].[Rol](
    [CodRol] INT IDENTITY(1,1) NOT NULL,
    [Nombre] VARCHAR(50) NOT NULL,
    [Activo] BIT NOT NULL CONSTRAINT [DF_Rol_Activo] DEFAULT(1),
    CONSTRAINT [PK_Rol] PRIMARY KEY CLUSTERED ([CodRol] ASC),
    CONSTRAINT [UQ_Rol_Nombre] UNIQUE ([Nombre])
)
GO

CREATE TABLE [dbo].[Permiso](
    [CodPermiso] INT IDENTITY(1,1) NOT NULL,
    [Nombre] VARCHAR(80) NOT NULL,
    [Descripcion] VARCHAR(150) NULL,
    [Tipo] VARCHAR(10) NOT NULL,
    [Activo] BIT NOT NULL CONSTRAINT [DF_Permiso_Activo] DEFAULT(1),
    CONSTRAINT [PK_Permiso] PRIMARY KEY CLUSTERED ([CodPermiso] ASC),
    CONSTRAINT [UQ_Permiso_Nombre] UNIQUE ([Nombre]),
    CONSTRAINT [CK_Permiso_Tipo] CHECK ([Tipo] IN ('Simple', 'Familia'))
)
GO

CREATE TABLE [dbo].[Usuario](
    [NombreUsuario] VARCHAR(50) NOT NULL,
    [Nombre] VARCHAR(50) NOT NULL,
    [Apellido] VARCHAR(50) NOT NULL,
    [Email] VARCHAR(100) NOT NULL,
    [Clave] VARCHAR(64) NOT NULL,
    [CodRol] INT NOT NULL,
    [Bloqueado] BIT NOT NULL CONSTRAINT [DF_Usuario_Bloqueado] DEFAULT(0),
    [Activo] BIT NOT NULL CONSTRAINT [DF_Usuario_Activo] DEFAULT(1),
    [ContFallidos] SMALLINT NOT NULL CONSTRAINT [DF_Usuario_ContFallidos] DEFAULT(0),
    CONSTRAINT [PK_Usuario] PRIMARY KEY CLUSTERED ([NombreUsuario] ASC),
    CONSTRAINT [UQ_Usuario_Email] UNIQUE ([Email]),
    CONSTRAINT [FK_Usuario_Rol] FOREIGN KEY ([CodRol]) REFERENCES [dbo].[Rol]([CodRol])
)
GO

CREATE TABLE [dbo].[Rol_Permiso](
    [CodRol] INT NOT NULL,
    [CodPermiso] INT NOT NULL,
    CONSTRAINT [PK_Rol_Permiso] PRIMARY KEY CLUSTERED ([CodRol] ASC, [CodPermiso] ASC),
    CONSTRAINT [FK_RolPermiso_Rol] FOREIGN KEY ([CodRol]) REFERENCES [dbo].[Rol]([CodRol]),
    CONSTRAINT [FK_RolPermiso_Permiso] FOREIGN KEY ([CodPermiso]) REFERENCES [dbo].[Permiso]([CodPermiso])
)
GO

CREATE TABLE [dbo].[Permiso_Componente](
    [CodPadre] INT NOT NULL,
    [CodHijo] INT NOT NULL,
    CONSTRAINT [PK_Permiso_Componente] PRIMARY KEY CLUSTERED ([CodPadre] ASC, [CodHijo] ASC),
    CONSTRAINT [FK_PermisoComponente_Padre] FOREIGN KEY ([CodPadre]) REFERENCES [dbo].[Permiso]([CodPermiso]),
    CONSTRAINT [FK_PermisoComponente_Hijo] FOREIGN KEY ([CodHijo]) REFERENCES [dbo].[Permiso]([CodPermiso]),
    CONSTRAINT [CK_PermisoComponente_NoAutoReferencia] CHECK ([CodPadre] <> [CodHijo])
)
GO

CREATE TABLE [dbo].[Eventos](
    [CodEvento] BIGINT IDENTITY(1,1) NOT NULL,
    [NombreUsuario] VARCHAR(50) NULL,
    [Modulo] VARCHAR(50) NOT NULL,
    [Evento] VARCHAR(100) NOT NULL,
    [Criticidad] SMALLINT NOT NULL,
    [Fecha] VARCHAR(11) NOT NULL,
    [Hora] VARCHAR(5) NOT NULL,
    CONSTRAINT [PK_Eventos] PRIMARY KEY CLUSTERED ([CodEvento] ASC),
    CONSTRAINT [FK_Eventos_Usuario] FOREIGN KEY ([NombreUsuario]) REFERENCES [dbo].[Usuario]([NombreUsuario])
)
GO

CREATE TABLE [dbo].[DigitoVerificador](
    [Tabla] NVARCHAR(50) NOT NULL,
    [DVH] VARCHAR(64) NULL,
    [DVV] VARCHAR(64) NULL,
    CONSTRAINT [PK_DigitoVerificador] PRIMARY KEY CLUSTERED ([Tabla] ASC)
)
GO

/* ============================================================
   Tablas de negocio
   ============================================================ */

CREATE TABLE [dbo].[Producto](
    [IdProducto] INT IDENTITY(1,1) NOT NULL,
    [Codigo] NVARCHAR(50) NOT NULL,
    [Nombre] NVARCHAR(255) NOT NULL,
    [Estado] NVARCHAR(20) NOT NULL,
    [Precio] DECIMAL(10,2) NOT NULL,
    [Stock] INT NOT NULL CONSTRAINT [DF_Producto_Stock] DEFAULT(0),
    [StockMaximo] INT NULL,
    [StockMinimo] INT NULL,
    [Categoria] NVARCHAR(100) NULL,
    [BorradoLogico] BIT NOT NULL CONSTRAINT [DF_Producto_BorradoLogico] DEFAULT(0),
    [FechaSincronizacion] DATETIME NULL,
    [Marca] NVARCHAR(100) NULL,
    CONSTRAINT [PK_Producto] PRIMARY KEY CLUSTERED ([IdProducto] ASC),
    CONSTRAINT [UQ_Producto_Codigo] UNIQUE ([Codigo])
)
GO

CREATE TABLE [dbo].[Venta](
    [IdVenta] INT IDENTITY(1,1) NOT NULL,
    [NroVenta] NVARCHAR(50) NOT NULL,
    [Fecha] DATETIME NOT NULL,
    [MontoTotal] DECIMAL(10,2) NOT NULL,
    [Estado] NVARCHAR(20) NOT NULL,
    CONSTRAINT [PK_Venta] PRIMARY KEY CLUSTERED ([IdVenta] ASC),
    CONSTRAINT [UQ_Venta_NroVenta] UNIQUE ([NroVenta])
)
GO

CREATE TABLE [dbo].[ItemVenta](
    [IdItemVenta] INT IDENTITY(1,1) NOT NULL,
    [IdVenta] INT NOT NULL,
    [IdProducto] INT NOT NULL,
    [Cantidad] INT NOT NULL,
    [PrecioVenta] DECIMAL(10,2) NOT NULL,
    CONSTRAINT [PK_ItemVenta] PRIMARY KEY CLUSTERED ([IdItemVenta] ASC),
    CONSTRAINT [FK_ItemVenta_Venta] FOREIGN KEY ([IdVenta]) REFERENCES [dbo].[Venta]([IdVenta]),
    CONSTRAINT [FK_ItemVenta_Producto] FOREIGN KEY ([IdProducto]) REFERENCES [dbo].[Producto]([IdProducto])
)
GO

CREATE TABLE [dbo].[HistorialPrecioProducto](
    [IdHistorialPrecio] INT IDENTITY(1,1) NOT NULL,
    [IdProducto] INT NOT NULL,
    [Precio] DECIMAL(10,2) NOT NULL,
    [Fecha] DATETIME NOT NULL,
    CONSTRAINT [PK_HistorialPrecioProducto] PRIMARY KEY CLUSTERED ([IdHistorialPrecio] ASC),
    CONSTRAINT [FK_HistorialPrecioProducto_Producto] FOREIGN KEY ([IdProducto]) REFERENCES [dbo].[Producto]([IdProducto])
)
GO

CREATE TABLE [dbo].[HistorialStock](
    [IdHistorialStock] INT IDENTITY(1,1) NOT NULL,
    [IdProducto] INT NOT NULL,
    [Stock] INT NOT NULL,
    [Fecha] DATETIME NOT NULL,
    CONSTRAINT [PK_HistorialStock] PRIMARY KEY CLUSTERED ([IdHistorialStock] ASC),
    CONSTRAINT [FK_HistorialStock_Producto] FOREIGN KEY ([IdProducto]) REFERENCES [dbo].[Producto]([IdProducto])
)
GO

CREATE TABLE [dbo].[Recomendacion](
    [IdRecomendacion] INT IDENTITY(1,1) NOT NULL,
    [IdProducto] INT NOT NULL,
    [Tipo] NVARCHAR(20) NOT NULL,
    [Titulo] NVARCHAR(255) NOT NULL,
    [Descripcion] NVARCHAR(1000) NOT NULL,
    [AccionSugerida] NVARCHAR(255) NULL,
    [Prioridad] NVARCHAR(10) NOT NULL,
    [Estado] NVARCHAR(20) NOT NULL CONSTRAINT [DF_Recomendacion_Estado] DEFAULT('Activa'),
    [FechaGeneracion] DATETIME NOT NULL CONSTRAINT [DF_Recomendacion_FechaGeneracion] DEFAULT(GETDATE()),
    CONSTRAINT [PK_Recomendacion] PRIMARY KEY CLUSTERED ([IdRecomendacion] ASC),
    CONSTRAINT [FK_Recomendacion_Producto] FOREIGN KEY ([IdProducto]) REFERENCES [dbo].[Producto]([IdProducto])
)
GO

CREATE TABLE [dbo].[Competencia](
    [IdCompetencia] INT IDENTITY(1,1) NOT NULL,
    [Nombre] NVARCHAR(255) NOT NULL,
    [Marketplace] NVARCHAR(50) NOT NULL,
    [Descripcion] NVARCHAR(500) NULL,
    [Estado] NVARCHAR(20) NOT NULL,
    CONSTRAINT [PK_Competencia] PRIMARY KEY CLUSTERED ([IdCompetencia] ASC)
)
GO

CREATE TABLE [dbo].[ProductoCompetencia](
    [IdProductoCompetencia] INT IDENTITY(1,1) NOT NULL,
    [IdProducto] INT NOT NULL,
    [IdCompetencia] INT NOT NULL,
    [URL] NVARCHAR(500) NOT NULL,
    [Estado] NVARCHAR(20) NOT NULL,
    [FechaAlta] DATETIME NOT NULL CONSTRAINT [DF_ProductoCompetencia_FechaAlta] DEFAULT(GETDATE()),
    [FechaUltimaVerificacion] DATETIME NULL,
    CONSTRAINT [PK_ProductoCompetencia] PRIMARY KEY CLUSTERED ([IdProductoCompetencia] ASC),
    CONSTRAINT [FK_ProductoCompetencia_Producto] FOREIGN KEY ([IdProducto]) REFERENCES [dbo].[Producto]([IdProducto]),
    CONSTRAINT [FK_ProductoCompetencia_Competencia] FOREIGN KEY ([IdCompetencia]) REFERENCES [dbo].[Competencia]([IdCompetencia])
)
GO

CREATE TABLE [dbo].[HistorialPrecioCompetencia](
    [IdPrecioCompetencia] INT IDENTITY(1,1) NOT NULL,
    [IdProductoCompetencia] INT NOT NULL,
    [PrecioCompetencia] DECIMAL(10,2) NOT NULL,
    [PrecioPropio] DECIMAL(10,2) NOT NULL,
    [FechaConsulta] DATETIME NOT NULL CONSTRAINT [DF_HistorialPrecioCompetencia_FechaConsulta] DEFAULT(GETDATE()),
    CONSTRAINT [PK_HistorialPrecioCompetencia] PRIMARY KEY CLUSTERED ([IdPrecioCompetencia] ASC),
    CONSTRAINT [FK_HistorialPrecioCompetencia_ProductoCompetencia] FOREIGN KEY ([IdProductoCompetencia]) REFERENCES [dbo].[ProductoCompetencia]([IdProductoCompetencia])
)
GO

CREATE TABLE [dbo].[CatalogoAutomatizacion](
    [IdCatalogoAutomatizacion] INT IDENTITY(1,1) NOT NULL,
    [Nombre] NVARCHAR(255) NOT NULL,
    [Tipo] NVARCHAR(50) NOT NULL,
    [Descripcion] NVARCHAR(500) NULL,
    CONSTRAINT [PK_CatalogoAutomatizacion] PRIMARY KEY CLUSTERED ([IdCatalogoAutomatizacion] ASC),
    CONSTRAINT [UQ_CatalogoAutomatizacion_Tipo] UNIQUE ([Tipo])
)
GO

CREATE TABLE [dbo].[Automatizacion](
    [IdAutomatizacion] INT IDENTITY(1,1) NOT NULL,
    [IdCatalogoAutomatizacion] INT NOT NULL,
    [Parametros] NVARCHAR(MAX) NOT NULL,
    [Estado] NVARCHAR(20) NOT NULL,
    [FechaAlta] DATETIME NOT NULL CONSTRAINT [DF_Automatizacion_FechaAlta] DEFAULT(GETDATE()),
    [FechaUltimaEjecucion] DATETIME NULL,
    CONSTRAINT [PK_Automatizacion] PRIMARY KEY CLUSTERED ([IdAutomatizacion] ASC),
    CONSTRAINT [FK_Automatizacion_CatalogoAutomatizacion] FOREIGN KEY ([IdCatalogoAutomatizacion]) REFERENCES [dbo].[CatalogoAutomatizacion]([IdCatalogoAutomatizacion])
)
GO

CREATE TABLE [dbo].[HistorialAutomatizacion](
    [IdHistorial] INT IDENTITY(1,1) NOT NULL,
    [IdProducto] INT NOT NULL,
    [IdAutomatizacion] INT NOT NULL,
    [AccionEjecutada] NVARCHAR(255) NOT NULL,
    [ValorAnterior] NVARCHAR(100) NULL,
    [ValorNuevo] NVARCHAR(100) NULL,
    [Estado] NVARCHAR(20) NOT NULL,
    [FechaEjecucion] DATETIME NOT NULL CONSTRAINT [DF_HistorialAutomatizacion_FechaEjecucion] DEFAULT(GETDATE()),
    CONSTRAINT [PK_HistorialAutomatizacion] PRIMARY KEY CLUSTERED ([IdHistorial] ASC),
    CONSTRAINT [FK_HistorialAutomatizacion_Producto] FOREIGN KEY ([IdProducto]) REFERENCES [dbo].[Producto]([IdProducto]),
    CONSTRAINT [FK_HistorialAutomatizacion_Automatizacion] FOREIGN KEY ([IdAutomatizacion]) REFERENCES [dbo].[Automatizacion]([IdAutomatizacion])
)
GO

CREATE TABLE [dbo].[ConfiguracionIntegracion](
    [IdConfiguracion] INT IDENTITY(1,1) NOT NULL,
    [TipoArchivo] NVARCHAR(20) NOT NULL,
    [FechaConfiguracion] DATETIME NOT NULL CONSTRAINT [DF_ConfiguracionIntegracion_FechaConfiguracion] DEFAULT(GETDATE()),
    CONSTRAINT [PK_ConfiguracionIntegracion] PRIMARY KEY CLUSTERED ([IdConfiguracion] ASC),
    CONSTRAINT [CK_ConfiguracionIntegracion_TipoArchivo] CHECK ([TipoArchivo] IN ('Productos', 'Ventas', 'Stock'))
)
GO

CREATE TABLE [dbo].[MapeoColumna](
    [IdMapeo] INT IDENTITY(1,1) NOT NULL,
    [IdConfiguracion] INT NOT NULL,
    [ColumnaStrategic] NVARCHAR(100) NOT NULL,
    [ColumnaCliente] NVARCHAR(100) NOT NULL,
    CONSTRAINT [PK_MapeoColumna] PRIMARY KEY CLUSTERED ([IdMapeo] ASC),
    CONSTRAINT [FK_MapeoColumna_ConfiguracionIntegracion] FOREIGN KEY ([IdConfiguracion]) REFERENCES [dbo].[ConfiguracionIntegracion]([IdConfiguracion])
)
GO

CREATE TABLE [dbo].[HistorialSincronizacion](
    [IdSincronizacion] INT IDENTITY(1,1) NOT NULL,
    [IdConfiguracion] INT NOT NULL,
    [TipoArchivo] NVARCHAR(20) NOT NULL,
    [RegistrosImportados] INT NOT NULL CONSTRAINT [DF_HistorialSincronizacion_RegistrosImportados] DEFAULT(0),
    [RegistrosActualizados] INT NOT NULL CONSTRAINT [DF_HistorialSincronizacion_RegistrosActualizados] DEFAULT(0),
    [RegistrosError] INT NOT NULL CONSTRAINT [DF_HistorialSincronizacion_RegistrosError] DEFAULT(0),
    [Estado] NVARCHAR(20) NOT NULL,
    [Detalle] NVARCHAR(2000) NULL,
    [FechaEjecucion] DATETIME NOT NULL CONSTRAINT [DF_HistorialSincronizacion_FechaEjecucion] DEFAULT(GETDATE()),
    CONSTRAINT [PK_HistorialSincronizacion] PRIMARY KEY CLUSTERED ([IdSincronizacion] ASC),
    CONSTRAINT [FK_HistorialSincronizacion_ConfiguracionIntegracion] FOREIGN KEY ([IdConfiguracion]) REFERENCES [dbo].[ConfiguracionIntegracion]([IdConfiguracion]),
    CONSTRAINT [CK_HistorialSincronizacion_TipoArchivo] CHECK ([TipoArchivo] IN ('Productos', 'Ventas', 'Stock'))
)
GO

CREATE TABLE [dbo].[Suscripcion](
    [IdSuscripcion] INT IDENTITY(1,1) NOT NULL,
    [FechaInicio] DATETIME NOT NULL,
    [FechaVencimiento] DATETIME NOT NULL,
    [Estado] NVARCHAR(20) NOT NULL,
    [Plan] NVARCHAR(50) NOT NULL,
    [Monto] DECIMAL(10,2) NOT NULL,
    [NumeroOperacion] NVARCHAR(100) NULL,
    CONSTRAINT [PK_Suscripcion] PRIMARY KEY CLUSTERED ([IdSuscripcion] ASC)
)
GO

SET IDENTITY_INSERT [dbo].[Rol] ON
INSERT INTO [dbo].[Rol] ([CodRol], [Nombre], [Activo]) VALUES
(1, 'WebMaster', 1),
(2, 'Administrador', 1),
(3, 'Analista', 1),
(4, 'Usuario', 1)
SET IDENTITY_INSERT [dbo].[Rol] OFF
GO

SET IDENTITY_INSERT [dbo].[Permiso] ON
INSERT INTO [dbo].[Permiso] ([CodPermiso], [Nombre], [Descripcion], [Tipo], [Activo]) VALUES
(1, 'SEGURIDAD_LOGIN', 'Permite iniciar sesion', 'Simple', 1),
(2, 'SEGURIDAD_USUARIOS', 'Permite gestionar usuarios', 'Simple', 1),
(3, 'SEGURIDAD_ROLES', 'Permite gestionar roles', 'Simple', 1),
(4, 'SEGURIDAD_FAMILIA_ADMIN', 'Familia inicial de permisos de seguridad', 'Familia', 1)
SET IDENTITY_INSERT [dbo].[Permiso] OFF
GO

INSERT INTO [dbo].[Permiso_Componente] ([CodPadre], [CodHijo]) VALUES
(4, 1),
(4, 2),
(4, 3)
GO

INSERT INTO [dbo].[Rol_Permiso] ([CodRol], [CodPermiso]) VALUES
(1, 4),
(2, 4),
(3, 1),
(4, 1)
GO

INSERT INTO [dbo].[Usuario] ([NombreUsuario], [Nombre], [Apellido], [Email], [Clave], [CodRol], [Bloqueado], [Activo], [ContFallidos]) VALUES
('Admin', 'Admin', 'Strategic', 'admin@strategic.local', '3b612c75a7b5048a435fb6ec81e52ff92d6d795a8b5a9c17070f6a63c97a53b2', 1, 0, 1, 0)
GO

SET IDENTITY_INSERT [dbo].[CatalogoAutomatizacion] ON
INSERT INTO [dbo].[CatalogoAutomatizacion] ([IdCatalogoAutomatizacion], [Nombre], [Tipo], [Descripcion]) VALUES
(1, 'Ajustar precio', 'AjustarPrecio', 'Ajusta el precio del producto cuando la diferencia con el competidor supera el umbral configurado'),
(2, 'Pausar venta', 'PausarVenta', 'Pausa la publicacion del producto cuando el stock disponible llega a cero'),
(3, 'Alerta de stock', 'AlertaStock', 'Genera una alerta cuando el stock del producto queda por debajo del stock minimo definido')
SET IDENTITY_INSERT [dbo].[CatalogoAutomatizacion] OFF
GO

INSERT INTO [dbo].[DigitoVerificador] ([Tabla], [DVH], [DVV]) VALUES
('Usuario', NULL, NULL),
('Rol', NULL, NULL),
('Permiso', NULL, NULL),
('Rol_Permiso', NULL, NULL),
('Permiso_Componente', NULL, NULL),
('Eventos', NULL, NULL),
('Producto', NULL, NULL),
('Competencia', NULL, NULL),
('Automatizacion', NULL, NULL)
GO

CREATE PROCEDURE [dbo].[ValidarUsuario]
    @NombreUsuario VARCHAR(50),
    @Email VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        U.NombreUsuario,
        U.Nombre,
        U.Apellido,
        U.Email,
        U.Clave,
        U.CodRol,
        U.Bloqueado,
        U.Activo,
        U.ContFallidos,
        R.Nombre AS NombreRol
    FROM [dbo].[Usuario] U
    INNER JOIN [dbo].[Rol] R ON U.CodRol = R.CodRol
    WHERE U.NombreUsuario = @NombreUsuario
       OR U.Email = @Email
END
GO

CREATE PROCEDURE [dbo].[ModificarBloquearUsuario]
    @NombreUsuario VARCHAR(50),
    @Bloqueado BIT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[Usuario] SET [Bloqueado] = @Bloqueado WHERE [NombreUsuario] = @NombreUsuario
END
GO

CREATE PROCEDURE [dbo].[ModificarContFallido]
    @NombreUsuario VARCHAR(50),
    @ContFallidos SMALLINT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[Usuario] SET [ContFallidos] = @ContFallidos WHERE [NombreUsuario] = @NombreUsuario
END
GO

CREATE PROCEDURE [dbo].[RegistrarEvento]
    @NombreUsuario VARCHAR(50),
    @Modulo VARCHAR(50),
    @Evento VARCHAR(100),
    @Criticidad SMALLINT,
    @Fecha VARCHAR(11),
    @Hora VARCHAR(5)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO [dbo].[Eventos] ([NombreUsuario], [Modulo], [Evento], [Criticidad], [Fecha], [Hora])
    VALUES (@NombreUsuario, @Modulo, @Evento, @Criticidad, @Fecha, @Hora)
END
GO

CREATE PROCEDURE [dbo].[TraerPermisosPorRol]
    @CodRol INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT P.CodPermiso, P.Nombre, P.Descripcion, P.Tipo, P.Activo
    FROM [dbo].[Rol_Permiso] RP
    INNER JOIN [dbo].[Permiso] P ON RP.CodPermiso = P.CodPermiso
    WHERE RP.CodRol = @CodRol
      AND P.Activo = 1
END
GO
CREATE PROCEDURE [dbo].[TraerListaEventos]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        E.CodEvento,
        E.NombreUsuario,
        E.Modulo,
        E.Evento,
        E.Criticidad,
        E.Fecha,
        E.Hora
    FROM [dbo].[Eventos] E
    ORDER BY E.CodEvento DESC
END
GO

CREATE PROCEDURE [dbo].[FiltrarEventos]
    @NombreUsuario VARCHAR(50) = NULL,
    @Modulo VARCHAR(50) = NULL,
    @Evento VARCHAR(100) = NULL,
    @FechaInicio VARCHAR(11) = NULL,
    @FechaFin VARCHAR(11) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        E.CodEvento,
        E.NombreUsuario,
        E.Modulo,
        E.Evento,
        E.Criticidad,
        E.Fecha,
        E.Hora
    FROM [dbo].[Eventos] E
    WHERE (@NombreUsuario IS NULL OR E.NombreUsuario LIKE '%' + @NombreUsuario + '%')
      AND (@Modulo IS NULL OR E.Modulo = @Modulo)
      AND (@Evento IS NULL OR E.Evento LIKE '%' + @Evento + '%')
      AND (@FechaInicio IS NULL OR E.Fecha >= @FechaInicio)
      AND (@FechaFin IS NULL OR E.Fecha <= @FechaFin)
    ORDER BY E.CodEvento DESC
END
GO

/* ============================================================
   004. Catalogo Sincronizado
   ============================================================ */

CREATE PROCEDURE [dbo].[TraerListaProductos]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        P.IdProducto,
        P.Codigo,
        P.Nombre,
        P.Estado,
        P.Precio,
        P.Stock,
        P.StockMaximo,
        P.StockMinimo,
        P.Categoria,
        P.BorradoLogico,
        P.FechaSincronizacion,
        P.Marca
    FROM [dbo].[Producto] P
    WHERE P.BorradoLogico = 0
    ORDER BY P.Nombre ASC
END
GO

CREATE PROCEDURE [dbo].[FiltrarProductos]
    @Nombre NVARCHAR(255) = NULL,
    @Categoria NVARCHAR(100) = NULL,
    @Estado NVARCHAR(20) = NULL,
    @PrecioMinimo DECIMAL(10,2) = NULL,
    @PrecioMaximo DECIMAL(10,2) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        P.IdProducto,
        P.Codigo,
        P.Nombre,
        P.Estado,
        P.Precio,
        P.Stock,
        P.StockMaximo,
        P.StockMinimo,
        P.Categoria,
        P.BorradoLogico,
        P.FechaSincronizacion,
        P.Marca
    FROM [dbo].[Producto] P
    WHERE P.BorradoLogico = 0
      AND (@Nombre IS NULL OR P.Nombre LIKE '%' + @Nombre + '%' OR P.Codigo LIKE '%' + @Nombre + '%')
      AND (@Categoria IS NULL OR P.Categoria = @Categoria)
      AND (@Estado IS NULL OR P.Estado = @Estado)
      AND (@PrecioMinimo IS NULL OR P.Precio >= @PrecioMinimo)
      AND (@PrecioMaximo IS NULL OR P.Precio <= @PrecioMaximo)
    ORDER BY P.Nombre ASC
END
GO

CREATE PROCEDURE [dbo].[TraerProductoPorId]
    @IdProducto INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        P.IdProducto,
        P.Codigo,
        P.Nombre,
        P.Estado,
        P.Precio,
        P.Stock,
        P.StockMaximo,
        P.StockMinimo,
        P.Categoria,
        P.BorradoLogico,
        P.FechaSincronizacion,
        P.Marca
    FROM [dbo].[Producto] P
    WHERE P.IdProducto = @IdProducto
END
GO

CREATE PROCEDURE [dbo].[TraerHistorialPrecios]
    @IdProducto INT,
    @FechaInicio DATETIME = NULL,
    @FechaFin DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        H.IdHistorialPrecio,
        H.IdProducto,
        P.Nombre AS NombreProducto,
        H.Precio,
        H.Fecha
    FROM [dbo].[HistorialPrecioProducto] H
    INNER JOIN [dbo].[Producto] P ON H.IdProducto = P.IdProducto
    WHERE H.IdProducto = @IdProducto
      AND (@FechaInicio IS NULL OR H.Fecha >= @FechaInicio)
      AND (@FechaFin IS NULL OR H.Fecha < DATEADD(DAY, 1, @FechaFin))
    ORDER BY H.Fecha ASC
END
GO

CREATE PROCEDURE [dbo].[TraerHistorialStock]
    @IdProducto INT,
    @FechaInicio DATETIME = NULL,
    @FechaFin DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        H.IdHistorialStock,
        H.IdProducto,
        P.Nombre AS NombreProducto,
        H.Stock,
        H.Fecha
    FROM [dbo].[HistorialStock] H
    INNER JOIN [dbo].[Producto] P ON H.IdProducto = P.IdProducto
    WHERE H.IdProducto = @IdProducto
      AND (@FechaInicio IS NULL OR H.Fecha >= @FechaInicio)
      AND (@FechaFin IS NULL OR H.Fecha < DATEADD(DAY, 1, @FechaFin))
    ORDER BY H.Fecha ASC
END
GO

CREATE PROCEDURE [dbo].[TraerListaVentas]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        V.IdVenta,
        V.NroVenta,
        V.Fecha,
        V.MontoTotal,
        V.Estado,
        ISNULL((SELECT COUNT(*) FROM [dbo].[ItemVenta] IV WHERE IV.IdVenta = V.IdVenta), 0) AS CantidadItems,
        ISNULL((SELECT SUM(IV.Cantidad) FROM [dbo].[ItemVenta] IV WHERE IV.IdVenta = V.IdVenta), 0) AS UnidadesVendidas
    FROM [dbo].[Venta] V
    ORDER BY V.Fecha DESC
END
GO

CREATE PROCEDURE [dbo].[FiltrarVentas]
    @FechaInicio DATETIME = NULL,
    @FechaFin DATETIME = NULL,
    @IdProducto INT = NULL,
    @Categoria NVARCHAR(100) = NULL,
    @Estado NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        V.IdVenta,
        V.NroVenta,
        V.Fecha,
        V.MontoTotal,
        V.Estado,
        ISNULL((SELECT COUNT(*) FROM [dbo].[ItemVenta] IV WHERE IV.IdVenta = V.IdVenta), 0) AS CantidadItems,
        ISNULL((SELECT SUM(IV.Cantidad) FROM [dbo].[ItemVenta] IV WHERE IV.IdVenta = V.IdVenta), 0) AS UnidadesVendidas
    FROM [dbo].[Venta] V
    WHERE (@FechaInicio IS NULL OR V.Fecha >= @FechaInicio)
      AND (@FechaFin IS NULL OR V.Fecha < DATEADD(DAY, 1, @FechaFin))
      AND (@Estado IS NULL OR V.Estado = @Estado)
      AND (@IdProducto IS NULL OR EXISTS (
              SELECT 1
              FROM [dbo].[ItemVenta] IV
              WHERE IV.IdVenta = V.IdVenta
                AND IV.IdProducto = @IdProducto))
      AND (@Categoria IS NULL OR EXISTS (
              SELECT 1
              FROM [dbo].[ItemVenta] IV
              INNER JOIN [dbo].[Producto] P ON IV.IdProducto = P.IdProducto
              WHERE IV.IdVenta = V.IdVenta
                AND P.Categoria = @Categoria))
    ORDER BY V.Fecha DESC
END
GO
