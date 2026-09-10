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

/* ------------------------------------------------------------
   Permisos simples (hojas del Composite)
   El nombre de cada permiso coincide con el nombre del aspx que
   habilita. Estan predefinidos: no se crean ni se modifican desde
   la aplicacion, solo se combinan en familias y se asignan a roles.
   ------------------------------------------------------------ */

SET IDENTITY_INSERT [dbo].[Permiso] ON

INSERT INTO [dbo].[Permiso] ([CodPermiso], [Nombre], [Descripcion], [Tipo], [Activo]) VALUES
( 1, 'Dashboard',                          'Ver el dashboard principal',                    'Simple', 1),
( 2, 'Recomendaciones',                    'Ver las recomendaciones generadas',             'Simple', 1),
( 3, 'Predicciones',                       'Ver las predicciones de demanda',               'Simple', 1),
( 4, 'CompararPrecios',                    'Comparar precios con los competidores',         'Simple', 1),
( 5, 'Reportes',                           'Generar reportes del negocio',                  'Simple', 1),
( 6, 'ConsultarAutomatizaciones',          'Consultar las automatizaciones configuradas',   'Simple', 1),
( 7, 'HistorialAutomatizaciones',          'Ver el historial de automatizaciones',          'Simple', 1),
( 8, 'ProductosSincronizados',             'Ver el catalogo de productos sincronizados',    'Simple', 1),
( 9, 'HistorialPrecios',                   'Ver el historial de precios de un producto',    'Simple', 1),
(10, 'HistorialStock',                     'Ver el historial de stock de un producto',      'Simple', 1),
(11, 'HistorialVentas',                    'Ver el historial de ventas',                    'Simple', 1),
(12, 'ConsultarUsuarios',                  'Gestionar los usuarios del sistema',            'Simple', 1),
(13, 'ConsultarRoles',                     'Gestionar los roles del sistema',               'Simple', 1),
(14, 'ConsultarFamilias',                  'Gestionar las familias de permisos',            'Simple', 1),
(15, 'ImportarDatos',                      'Importar y sincronizar datos del cliente',      'Simple', 1),
(16, 'HistorialSincronizaciones',          'Ver el historial de sincronizaciones',          'Simple', 1),
(17, 'ConsultarCompetidores',              'Gestionar los competidores registrados',        'Simple', 1),
(18, 'MapearProductosCompetencia',         'Asociar productos con publicaciones rivales',   'Simple', 1),
(19, 'MonitoreoPublicacionesCompetidoras', 'Monitorear las publicaciones de la competencia', 'Simple', 1),
(20, 'ConsultarEventosSistema',            'Consultar la bitacora de eventos',              'Simple', 1),
(21, 'RealizarBackup',                     'Generar copias de seguridad',                   'Simple', 1),
(22, 'RenovarSuscripcion',                 'Gestionar la suscripcion del cliente',          'Simple', 1),

/* ------------------------------------------------------------
   Familias iniciales (composites)
   Agrupan permisos por modulo. "Acceso total" es una familia de
   familias: muestra que el arbol admite mas de un nivel.
   Se pueden crear otras desde el CU-005-026.
   ------------------------------------------------------------ */
(23, 'Modulo Analisis',         'Pantallas de analisis del negocio',            'Familia', 1),
(24, 'Modulo Catalogo',         'Pantallas del catalogo sincronizado',          'Familia', 1),
(25, 'Modulo Automatizaciones', 'Pantallas de automatizaciones',                'Familia', 1),
(26, 'Modulo Competencia',      'Pantallas de gestion de competencia',          'Familia', 1),
(27, 'Modulo Integraciones',    'Pantallas de importacion y sincronizacion',    'Familia', 1),
(28, 'Modulo Seguridad',        'Pantallas de usuarios, roles y familias',      'Familia', 1),
(29, 'Modulo Auditoria',        'Pantallas de bitacora y backup',               'Familia', 1),
(30, 'Acceso total',            'Todas las pantallas del sistema',              'Familia', 1)

SET IDENTITY_INSERT [dbo].[Permiso] OFF
GO


/* ------------------------------------------------------------
   Permiso_Componente: la relacion recursiva que arma el arbol.
   CodPadre siempre es una familia; CodHijo puede ser un permiso
   simple u otra familia.
   ------------------------------------------------------------ */

INSERT INTO [dbo].[Permiso_Componente] ([CodPadre], [CodHijo]) VALUES
-- Modulo Analisis
(23,  1), (23,  2), (23,  3), (23,  4), (23,  5),
-- Modulo Catalogo
(24,  8), (24,  9), (24, 10), (24, 11),
-- Modulo Automatizaciones
(25,  6), (25,  7),
-- Modulo Competencia
(26, 17), (26, 18), (26, 19),
-- Modulo Integraciones
(27, 15), (27, 16),
-- Modulo Seguridad
(28, 12), (28, 13), (28, 14),
-- Modulo Auditoria
(29, 20), (29, 21),
-- Acceso total: contiene a las demas familias y un permiso suelto
(30, 23), (30, 24), (30, 25), (30, 26), (30, 27), (30, 28), (30, 29), (30, 22)
GO


/* ------------------------------------------------------------
   Rol_Permiso: el rol se asocia a cualquier componente, sea un
   permiso simple o una familia. No distingue entre los dos.
   ------------------------------------------------------------ */

INSERT INTO [dbo].[Rol_Permiso] ([CodRol], [CodPermiso]) VALUES
-- WebMaster: una sola familia que lo alcanza todo
(1, 30),
-- Administrador: varias familias mas un permiso simple suelto
(2, 23), (2, 24), (2, 25), (2, 26), (2, 27), (2, 28), (2, 20),
-- Analista: solo consulta
(3, 23), (3, 24),
-- Usuario: apenas el dashboard
(4,  1)
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

/* ============================================================
   002. Analisis - CU-002-004 Visualizar Dashboard Principal
   ------------------------------------------------------------
   La facturacion se calcula sobre ItemVenta y no sobre
   Venta.MontoTotal, para que el filtro por categoria no sume de
   mas cuando una venta tiene productos de varias categorias.
   Las ventas canceladas quedan fuera de todas las metricas.
   ============================================================ */

CREATE PROCEDURE [dbo].[TraerResumenDashboard]
    @FechaInicio DATETIME = NULL,
    @FechaFin DATETIME = NULL,
    @Categoria NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ISNULL(SUM(IV.Cantidad * IV.PrecioVenta), 0) AS FacturacionTotal,
        COUNT(DISTINCT V.IdVenta)                    AS CantidadVentas,
        ISNULL(SUM(IV.Cantidad), 0)                  AS UnidadesVendidas
    FROM [dbo].[Venta] V
    INNER JOIN [dbo].[ItemVenta] IV ON IV.IdVenta = V.IdVenta
    INNER JOIN [dbo].[Producto] P ON P.IdProducto = IV.IdProducto
    WHERE V.Estado <> 'Cancelada'
      AND (@FechaInicio IS NULL OR V.Fecha >= @FechaInicio)
      AND (@FechaFin IS NULL OR V.Fecha < DATEADD(DAY, 1, @FechaFin))
      AND (@Categoria IS NULL OR P.Categoria = @Categoria)
END
GO

CREATE PROCEDURE [dbo].[TraerVentasPorCategoria]
    @FechaInicio DATETIME = NULL,
    @FechaFin DATETIME = NULL,
    @Categoria NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ISNULL(P.Categoria, N'Sin categoria')     AS Categoria,
        SUM(IV.Cantidad)                          AS Unidades,
        SUM(IV.Cantidad * IV.PrecioVenta)         AS Monto
    FROM [dbo].[Venta] V
    INNER JOIN [dbo].[ItemVenta] IV ON IV.IdVenta = V.IdVenta
    INNER JOIN [dbo].[Producto] P ON P.IdProducto = IV.IdProducto
    WHERE V.Estado <> 'Cancelada'
      AND (@FechaInicio IS NULL OR V.Fecha >= @FechaInicio)
      AND (@FechaFin IS NULL OR V.Fecha < DATEADD(DAY, 1, @FechaFin))
      AND (@Categoria IS NULL OR P.Categoria = @Categoria)
    GROUP BY ISNULL(P.Categoria, N'Sin categoria')
    ORDER BY Unidades DESC
END
GO

CREATE PROCEDURE [dbo].[TraerTopProductos]
    @FechaInicio DATETIME = NULL,
    @FechaFin DATETIME = NULL,
    @Categoria NVARCHAR(100) = NULL,
    @Cantidad INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (@Cantidad)
        P.IdProducto,
        P.Codigo,
        P.Nombre,
        ISNULL(P.Categoria, N'')          AS Categoria,
        SUM(IV.Cantidad)                  AS Unidades,
        SUM(IV.Cantidad * IV.PrecioVenta) AS Monto
    FROM [dbo].[Venta] V
    INNER JOIN [dbo].[ItemVenta] IV ON IV.IdVenta = V.IdVenta
    INNER JOIN [dbo].[Producto] P ON P.IdProducto = IV.IdProducto
    WHERE V.Estado <> 'Cancelada'
      AND (@FechaInicio IS NULL OR V.Fecha >= @FechaInicio)
      AND (@FechaFin IS NULL OR V.Fecha < DATEADD(DAY, 1, @FechaFin))
      AND (@Categoria IS NULL OR P.Categoria = @Categoria)
    GROUP BY P.IdProducto, P.Codigo, P.Nombre, P.Categoria
    ORDER BY Unidades DESC
END
GO

CREATE PROCEDURE [dbo].[TraerProductosBajoStock]
    @Categoria NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- No depende del rango de fechas: es la foto actual del stock
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
      AND P.StockMinimo IS NOT NULL
      AND P.Stock <= P.StockMinimo
      AND (@Categoria IS NULL OR P.Categoria = @Categoria)
    ORDER BY (P.Stock - P.StockMinimo) ASC, P.Nombre ASC
END
GO

/* ============================================================
   002. Analisis - CU-002-007 Comparar Precios con Competidores
   ------------------------------------------------------------
   La comparacion se arma sobre los mapeos de ProductoCompetencia:
   cada fila es un producto propio asociado a una publicacion de
   un competidor. El precio de la competencia es el ultimo valor
   registrado en HistorialPrecioCompetencia.
   La diferencia porcentual se calcula contra el precio del
   competidor: positiva significa que el producto propio esta mas
   caro que la publicacion de la competencia.
   Los mapeos que todavia no tienen ninguna consulta de precio se
   siguen mostrando, con el precio de la competencia en NULL.
   ============================================================ */

CREATE PROCEDURE [dbo].[TraerListaCompetidores]
AS
BEGIN
    SET NOCOUNT ON;

    -- Solo los competidores vigentes: son los que se pueden comparar.
    -- El listado completo lo resuelve el CU-007-032 con sus propios filtros.
    SELECT
        C.IdCompetencia,
        C.Nombre,
        C.Marketplace,
        ISNULL(C.Descripcion, N'') AS Descripcion,
        C.Estado
    FROM [dbo].[Competencia] C
    WHERE C.Estado = N'Activo'
    ORDER BY C.Nombre ASC
END
GO

CREATE PROCEDURE [dbo].[TraerComparacionPrecios]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        PC.IdProductoCompetencia,
        P.IdProducto,
        P.Codigo                 AS CodigoProducto,
        P.Nombre                 AS NombreProducto,
        ISNULL(P.Categoria, N'') AS Categoria,
        C.IdCompetencia,
        C.Nombre                 AS NombreCompetidor,
        C.Marketplace,
        PC.URL,
        P.Precio                 AS PrecioPropio,
        UP.PrecioCompetencia,
        UP.FechaConsulta,
        CASE
            WHEN UP.PrecioCompetencia IS NULL OR UP.PrecioCompetencia = 0 THEN NULL
            ELSE CONVERT(DECIMAL(10,2), ((P.Precio - UP.PrecioCompetencia) / UP.PrecioCompetencia) * 100)
        END AS DiferenciaPorcentaje
    FROM [dbo].[ProductoCompetencia] PC
    INNER JOIN [dbo].[Producto] P ON P.IdProducto = PC.IdProducto
    INNER JOIN [dbo].[Competencia] C ON C.IdCompetencia = PC.IdCompetencia
    OUTER APPLY (
        SELECT TOP (1)
            H.PrecioCompetencia,
            H.FechaConsulta
        FROM [dbo].[HistorialPrecioCompetencia] H
        WHERE H.IdProductoCompetencia = PC.IdProductoCompetencia
        ORDER BY H.FechaConsulta DESC
    ) UP
    WHERE P.BorradoLogico = 0
      AND PC.Estado = N'Activo'
    ORDER BY P.Nombre ASC, C.Nombre ASC
END
GO

CREATE PROCEDURE [dbo].[FiltrarComparacionPrecios]
    @IdProducto INT = NULL,
    @Categoria NVARCHAR(100) = NULL,
    @IdCompetencia INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        PC.IdProductoCompetencia,
        P.IdProducto,
        P.Codigo                 AS CodigoProducto,
        P.Nombre                 AS NombreProducto,
        ISNULL(P.Categoria, N'') AS Categoria,
        C.IdCompetencia,
        C.Nombre                 AS NombreCompetidor,
        C.Marketplace,
        PC.URL,
        P.Precio                 AS PrecioPropio,
        UP.PrecioCompetencia,
        UP.FechaConsulta,
        CASE
            WHEN UP.PrecioCompetencia IS NULL OR UP.PrecioCompetencia = 0 THEN NULL
            ELSE CONVERT(DECIMAL(10,2), ((P.Precio - UP.PrecioCompetencia) / UP.PrecioCompetencia) * 100)
        END AS DiferenciaPorcentaje
    FROM [dbo].[ProductoCompetencia] PC
    INNER JOIN [dbo].[Producto] P ON P.IdProducto = PC.IdProducto
    INNER JOIN [dbo].[Competencia] C ON C.IdCompetencia = PC.IdCompetencia
    OUTER APPLY (
        SELECT TOP (1)
            H.PrecioCompetencia,
            H.FechaConsulta
        FROM [dbo].[HistorialPrecioCompetencia] H
        WHERE H.IdProductoCompetencia = PC.IdProductoCompetencia
        ORDER BY H.FechaConsulta DESC
    ) UP
    WHERE P.BorradoLogico = 0
      AND PC.Estado = N'Activo'
      AND (@IdProducto IS NULL OR P.IdProducto = @IdProducto)
      AND (@Categoria IS NULL OR P.Categoria = @Categoria)
      AND (@IdCompetencia IS NULL OR C.IdCompetencia = @IdCompetencia)
    ORDER BY P.Nombre ASC, C.Nombre ASC
END
GO

CREATE PROCEDURE [dbo].[TraerComparacionPrecioPorId]
    @IdProductoCompetencia INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        PC.IdProductoCompetencia,
        P.IdProducto,
        P.Codigo                 AS CodigoProducto,
        P.Nombre                 AS NombreProducto,
        ISNULL(P.Categoria, N'') AS Categoria,
        C.IdCompetencia,
        C.Nombre                 AS NombreCompetidor,
        C.Marketplace,
        PC.URL,
        P.Precio                 AS PrecioPropio,
        UP.PrecioCompetencia,
        UP.FechaConsulta,
        CASE
            WHEN UP.PrecioCompetencia IS NULL OR UP.PrecioCompetencia = 0 THEN NULL
            ELSE CONVERT(DECIMAL(10,2), ((P.Precio - UP.PrecioCompetencia) / UP.PrecioCompetencia) * 100)
        END AS DiferenciaPorcentaje
    FROM [dbo].[ProductoCompetencia] PC
    INNER JOIN [dbo].[Producto] P ON P.IdProducto = PC.IdProducto
    INNER JOIN [dbo].[Competencia] C ON C.IdCompetencia = PC.IdCompetencia
    OUTER APPLY (
        SELECT TOP (1)
            H.PrecioCompetencia,
            H.FechaConsulta
        FROM [dbo].[HistorialPrecioCompetencia] H
        WHERE H.IdProductoCompetencia = PC.IdProductoCompetencia
        ORDER BY H.FechaConsulta DESC
    ) UP
    WHERE PC.IdProductoCompetencia = @IdProductoCompetencia
END
GO

CREATE PROCEDURE [dbo].[TraerHistorialPrecioCompetencia]
    @IdProductoCompetencia INT
AS
BEGIN
    SET NOCOUNT ON;

    -- El historial guarda el precio propio vigente al momento de cada consulta,
    -- asi la comparacion refleja como estaban los dos precios ese dia
    SELECT
        H.IdPrecioCompetencia,
        H.IdProductoCompetencia,
        H.PrecioCompetencia,
        H.PrecioPropio,
        H.FechaConsulta,
        CASE
            WHEN H.PrecioCompetencia = 0 THEN NULL
            ELSE CONVERT(DECIMAL(10,2), ((H.PrecioPropio - H.PrecioCompetencia) / H.PrecioCompetencia) * 100)
        END AS DiferenciaPorcentaje
    FROM [dbo].[HistorialPrecioCompetencia] H
    WHERE H.IdProductoCompetencia = @IdProductoCompetencia
    ORDER BY H.FechaConsulta ASC
END
GO

/* ============================================================
   005. Gestion de Usuarios y Permisos
   ------------------------------------------------------------
   CU-005-017 Consultar Usuarios
   CU-005-018 Alta Usuario
   CU-005-019 Baja Usuario (logica: Activo = 0)
   CU-005-020 Modificar Usuario

   Los procedimientos de consulta no devuelven la columna Clave:
   la contrasenia encriptada solo sale de la base en ValidarUsuario,
   que es el unico lugar que necesita compararla.
   ============================================================ */

CREATE PROCEDURE [dbo].[TraerListaRoles]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        R.CodRol,
        R.Nombre,
        R.Activo
    FROM [dbo].[Rol] R
    WHERE R.Activo = 1
    ORDER BY R.CodRol ASC
END
GO

CREATE PROCEDURE [dbo].[TraerListaUsuarios]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        U.NombreUsuario,
        U.Nombre,
        U.Apellido,
        U.Email,
        U.CodRol,
        U.Bloqueado,
        U.Activo,
        U.ContFallidos,
        R.Nombre AS NombreRol
    FROM [dbo].[Usuario] U
    INNER JOIN [dbo].[Rol] R ON U.CodRol = R.CodRol
    ORDER BY U.NombreUsuario ASC
END
GO

CREATE PROCEDURE [dbo].[FiltrarUsuarios]
    @Texto VARCHAR(100) = NULL,
    @CodRol INT = NULL,
    @Activo BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- @Texto busca en usuario, nombre, apellido y email a la vez
    SELECT
        U.NombreUsuario,
        U.Nombre,
        U.Apellido,
        U.Email,
        U.CodRol,
        U.Bloqueado,
        U.Activo,
        U.ContFallidos,
        R.Nombre AS NombreRol
    FROM [dbo].[Usuario] U
    INNER JOIN [dbo].[Rol] R ON U.CodRol = R.CodRol
    WHERE (@Texto IS NULL
           OR U.NombreUsuario LIKE '%' + @Texto + '%'
           OR U.Nombre LIKE '%' + @Texto + '%'
           OR U.Apellido LIKE '%' + @Texto + '%'
           OR U.Email LIKE '%' + @Texto + '%')
      AND (@CodRol IS NULL OR U.CodRol = @CodRol)
      AND (@Activo IS NULL OR U.Activo = @Activo)
    ORDER BY U.NombreUsuario ASC
END
GO

CREATE PROCEDURE [dbo].[TraerUsuarioPorNombre]
    @NombreUsuario VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        U.NombreUsuario,
        U.Nombre,
        U.Apellido,
        U.Email,
        U.CodRol,
        U.Bloqueado,
        U.Activo,
        U.ContFallidos,
        R.Nombre AS NombreRol
    FROM [dbo].[Usuario] U
    INNER JOIN [dbo].[Rol] R ON U.CodRol = R.CodRol
    WHERE U.NombreUsuario = @NombreUsuario
END
GO

CREATE PROCEDURE [dbo].[ExisteUsuario]
    @NombreUsuario VARCHAR(50) = NULL,
    @Email VARCHAR(100) = NULL,
    @NombreUsuarioExcluido VARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Alternativa 5.1 del CU-005-018 y 4.1 del CU-005-020 [#ERR021].
    -- @NombreUsuarioExcluido deja fuera al usuario que se esta editando,
    -- para que sus propios datos no se cuenten como duplicados.
    SELECT COUNT(*) AS Cantidad
    FROM [dbo].[Usuario] U
    WHERE (@NombreUsuarioExcluido IS NULL OR U.NombreUsuario <> @NombreUsuarioExcluido)
      AND ((@NombreUsuario IS NOT NULL AND U.NombreUsuario = @NombreUsuario)
           OR (@Email IS NOT NULL AND U.Email = @Email))
END
GO

CREATE PROCEDURE [dbo].[AltaUsuario]
    @NombreUsuario VARCHAR(50),
    @Nombre VARCHAR(50),
    @Apellido VARCHAR(50),
    @Email VARCHAR(100),
    @Clave VARCHAR(64),
    @CodRol INT
AS
BEGIN
    SET NOCOUNT ON;

    -- El usuario nace activo, desbloqueado y sin intentos fallidos
    INSERT INTO [dbo].[Usuario]
        ([NombreUsuario], [Nombre], [Apellido], [Email], [Clave], [CodRol], [Bloqueado], [Activo], [ContFallidos])
    VALUES
        (@NombreUsuario, @Nombre, @Apellido, @Email, @Clave, @CodRol, 0, 1, 0)
END
GO

CREATE PROCEDURE [dbo].[ModificarUsuario]
    @NombreUsuario VARCHAR(50),
    @Nombre VARCHAR(50),
    @Apellido VARCHAR(50),
    @Email VARCHAR(100),
    @CodRol INT,
    @Bloqueado BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [dbo].[Usuario]
    SET [Nombre] = @Nombre,
        [Apellido] = @Apellido,
        [Email] = @Email,
        [CodRol] = @CodRol,
        [Bloqueado] = @Bloqueado,
        -- Al desbloquear se reinicia el contador: si quedara en el limite el
        -- usuario se volveria a bloquear con el primer intento fallido
        [ContFallidos] = CASE WHEN @Bloqueado = 0 THEN 0 ELSE [ContFallidos] END
    WHERE [NombreUsuario] = @NombreUsuario
END
GO

CREATE PROCEDURE [dbo].[ModificarEstadoUsuario]
    @NombreUsuario VARCHAR(50),
    @Activo BIT
AS
BEGIN
    SET NOCOUNT ON;

    -- Baja y reactivacion logicas del CU-005-019. El usuario nunca se borra
    -- fisicamente porque la bitacora de eventos lo referencia.
    UPDATE [dbo].[Usuario]
    SET [Activo] = @Activo
    WHERE [NombreUsuario] = @NombreUsuario
END
GO

/* ============================================================
   005. Gestion de Usuarios y Permisos - Roles y Familias
   ------------------------------------------------------------
   CU-005-021 a CU-005-024  Roles
   CU-005-025 a CU-005-028  Familias

   Roles y familias se guardan con sus componentes en una sola
   llamada: el alta y la modificacion reciben los codigos elegidos
   en una lista separada por comas y resuelven todo dentro de una
   transaccion, para que no pueda quedar un rol sin permisos o una
   familia a medio armar.
   ============================================================ */

CREATE PROCEDURE [dbo].[FiltrarRoles]
    @Nombre VARCHAR(50) = NULL,
    @Activo BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- CantidadComponentes es lo que muestra el listado del CU-005-021
    SELECT
        R.CodRol,
        R.Nombre,
        R.Activo,
        (SELECT COUNT(*) FROM [dbo].[Rol_Permiso] RP WHERE RP.CodRol = R.CodRol) AS CantidadComponentes,
        (SELECT COUNT(*) FROM [dbo].[Usuario] U WHERE U.CodRol = R.CodRol AND U.Activo = 1) AS UsuariosActivos
    FROM [dbo].[Rol] R
    WHERE (@Nombre IS NULL OR R.Nombre LIKE '%' + @Nombre + '%')
      AND (@Activo IS NULL OR R.Activo = @Activo)
    ORDER BY R.Nombre ASC
END
GO

CREATE PROCEDURE [dbo].[TraerRolPorId]
    @CodRol INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        R.CodRol,
        R.Nombre,
        R.Activo,
        (SELECT COUNT(*) FROM [dbo].[Rol_Permiso] RP WHERE RP.CodRol = R.CodRol) AS CantidadComponentes,
        (SELECT COUNT(*) FROM [dbo].[Usuario] U WHERE U.CodRol = R.CodRol AND U.Activo = 1) AS UsuariosActivos
    FROM [dbo].[Rol] R
    WHERE R.CodRol = @CodRol
END
GO

CREATE PROCEDURE [dbo].[ExisteRol]
    @Nombre VARCHAR(50),
    @CodRolExcluido INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Alternativa 4.2 de los CU-005-022 y CU-005-024 [#ERR038]
    SELECT COUNT(*) AS Cantidad
    FROM [dbo].[Rol] R
    WHERE R.Nombre = @Nombre
      AND (@CodRolExcluido IS NULL OR R.CodRol <> @CodRolExcluido)
END
GO

CREATE PROCEDURE [dbo].[TraerComponentesDeRol]
    @CodRol INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Hijos directos del rol: pueden ser permisos simples o familias
    SELECT
        P.CodPermiso,
        P.Nombre,
        ISNULL(P.Descripcion, '') AS Descripcion,
        P.Tipo,
        P.Activo
    FROM [dbo].[Rol_Permiso] RP
    INNER JOIN [dbo].[Permiso] P ON P.CodPermiso = RP.CodPermiso
    WHERE RP.CodRol = @CodRol
    ORDER BY P.Tipo DESC, P.Nombre ASC
END
GO

CREATE PROCEDURE [dbo].[AltaRol]
    @Nombre VARCHAR(50),
    @Componentes VARCHAR(1000)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRANSACTION

    BEGIN TRY
        INSERT INTO [dbo].[Rol] ([Nombre], [Activo]) VALUES (@Nombre, 1)

        DECLARE @CodRol INT = SCOPE_IDENTITY()

        INSERT INTO [dbo].[Rol_Permiso] ([CodRol], [CodPermiso])
        SELECT @CodRol, CONVERT(INT, LTRIM(RTRIM(S.value)))
        FROM STRING_SPLIT(@Componentes, ',') S
        WHERE LTRIM(RTRIM(S.value)) <> ''

        COMMIT TRANSACTION

        SELECT @CodRol AS CodRol
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION
        THROW
    END CATCH
END
GO

CREATE PROCEDURE [dbo].[ModificarRol]
    @CodRol INT,
    @Nombre VARCHAR(50),
    @Componentes VARCHAR(1000)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRANSACTION

    BEGIN TRY
        UPDATE [dbo].[Rol] SET [Nombre] = @Nombre WHERE [CodRol] = @CodRol

        -- Los componentes se reemplazan enteros: es mas simple y mas seguro
        -- que ir comparando cual se agrego y cual se quito
        DELETE FROM [dbo].[Rol_Permiso] WHERE [CodRol] = @CodRol

        INSERT INTO [dbo].[Rol_Permiso] ([CodRol], [CodPermiso])
        SELECT @CodRol, CONVERT(INT, LTRIM(RTRIM(S.value)))
        FROM STRING_SPLIT(@Componentes, ',') S
        WHERE LTRIM(RTRIM(S.value)) <> ''

        COMMIT TRANSACTION
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION
        THROW
    END CATCH
END
GO

CREATE PROCEDURE [dbo].[ModificarEstadoRol]
    @CodRol INT,
    @Activo BIT
AS
BEGIN
    SET NOCOUNT ON;

    -- Baja logica del CU-005-023: el rol no se borra porque los usuarios
    -- que lo tuvieron asignado lo siguen referenciando
    UPDATE [dbo].[Rol] SET [Activo] = @Activo WHERE [CodRol] = @CodRol
END
GO

CREATE PROCEDURE [dbo].[ContarUsuariosActivosPorRol]
    @CodRol INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Alternativa 2.1 del CU-005-023 [#ERR039]
    SELECT COUNT(*) AS Cantidad
    FROM [dbo].[Usuario] U
    WHERE U.CodRol = @CodRol
      AND U.Activo = 1
END
GO


/* ------------------------------------------------------------
   Permisos y familias
   ------------------------------------------------------------ */

CREATE PROCEDURE [dbo].[TraerListaPermisos]
    @Tipo VARCHAR(10) = NULL,
    @Activo BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- @Tipo NULL trae hojas y familias juntas, que es como las consume
    -- el selector de componentes
    SELECT
        P.CodPermiso,
        P.Nombre,
        ISNULL(P.Descripcion, '') AS Descripcion,
        P.Tipo,
        P.Activo
    FROM [dbo].[Permiso] P
    WHERE (@Tipo IS NULL OR P.Tipo = @Tipo)
      AND (@Activo IS NULL OR P.Activo = @Activo)
    ORDER BY P.Tipo DESC, P.Nombre ASC
END
GO

CREATE PROCEDURE [dbo].[FiltrarFamilias]
    @Nombre VARCHAR(80) = NULL,
    @Activo BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        P.CodPermiso,
        P.Nombre,
        ISNULL(P.Descripcion, '') AS Descripcion,
        P.Tipo,
        P.Activo,
        (SELECT COUNT(*) FROM [dbo].[Permiso_Componente] PC WHERE PC.CodPadre = P.CodPermiso) AS CantidadComponentes
    FROM [dbo].[Permiso] P
    WHERE P.Tipo = 'Familia'
      AND (@Nombre IS NULL OR P.Nombre LIKE '%' + @Nombre + '%')
      AND (@Activo IS NULL OR P.Activo = @Activo)
    ORDER BY P.Nombre ASC
END
GO

CREATE PROCEDURE [dbo].[TraerPermisoPorId]
    @CodPermiso INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        P.CodPermiso,
        P.Nombre,
        ISNULL(P.Descripcion, '') AS Descripcion,
        P.Tipo,
        P.Activo
    FROM [dbo].[Permiso] P
    WHERE P.CodPermiso = @CodPermiso
END
GO

CREATE PROCEDURE [dbo].[TraerHijosDeFamilia]
    @CodPadre INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        P.CodPermiso,
        P.Nombre,
        ISNULL(P.Descripcion, '') AS Descripcion,
        P.Tipo,
        P.Activo
    FROM [dbo].[Permiso_Componente] PC
    INNER JOIN [dbo].[Permiso] P ON P.CodPermiso = PC.CodHijo
    WHERE PC.CodPadre = @CodPadre
    ORDER BY P.Tipo DESC, P.Nombre ASC
END
GO

CREATE PROCEDURE [dbo].[ExistePermisoConNombre]
    @Nombre VARCHAR(80),
    @CodPermisoExcluido INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- El nombre es unico en toda la tabla Permiso, asi que una familia
    -- tampoco puede llamarse igual que un permiso simple [#ERR040]
    SELECT COUNT(*) AS Cantidad
    FROM [dbo].[Permiso] P
    WHERE P.Nombre = @Nombre
      AND (@CodPermisoExcluido IS NULL OR P.CodPermiso <> @CodPermisoExcluido)
END
GO

CREATE PROCEDURE [dbo].[AltaFamilia]
    @Nombre VARCHAR(80),
    @Descripcion VARCHAR(150) = NULL,
    @Componentes VARCHAR(1000)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRANSACTION

    BEGIN TRY
        INSERT INTO [dbo].[Permiso] ([Nombre], [Descripcion], [Tipo], [Activo])
        VALUES (@Nombre, @Descripcion, 'Familia', 1)

        DECLARE @CodPermiso INT = SCOPE_IDENTITY()

        INSERT INTO [dbo].[Permiso_Componente] ([CodPadre], [CodHijo])
        SELECT @CodPermiso, CONVERT(INT, LTRIM(RTRIM(S.value)))
        FROM STRING_SPLIT(@Componentes, ',') S
        WHERE LTRIM(RTRIM(S.value)) <> ''

        COMMIT TRANSACTION

        SELECT @CodPermiso AS CodPermiso
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION
        THROW
    END CATCH
END
GO

CREATE PROCEDURE [dbo].[ModificarFamilia]
    @CodPermiso INT,
    @Nombre VARCHAR(80),
    @Descripcion VARCHAR(150) = NULL,
    @Componentes VARCHAR(1000)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRANSACTION

    BEGIN TRY
        UPDATE [dbo].[Permiso]
        SET [Nombre] = @Nombre,
            [Descripcion] = @Descripcion
        WHERE [CodPermiso] = @CodPermiso
          AND [Tipo] = 'Familia'

        DELETE FROM [dbo].[Permiso_Componente] WHERE [CodPadre] = @CodPermiso

        INSERT INTO [dbo].[Permiso_Componente] ([CodPadre], [CodHijo])
        SELECT @CodPermiso, CONVERT(INT, LTRIM(RTRIM(S.value)))
        FROM STRING_SPLIT(@Componentes, ',') S
        WHERE LTRIM(RTRIM(S.value)) <> ''

        COMMIT TRANSACTION
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION
        THROW
    END CATCH
END
GO

CREATE PROCEDURE [dbo].[ModificarEstadoFamilia]
    @CodPermiso INT,
    @Activo BIT
AS
BEGIN
    SET NOCOUNT ON;

    -- Baja logica del CU-005-027: la familia no se borra para no perder
    -- el arbol que quedo registrado
    UPDATE [dbo].[Permiso]
    SET [Activo] = @Activo
    WHERE [CodPermiso] = @CodPermiso
      AND [Tipo] = 'Familia'
END
GO

CREATE PROCEDURE [dbo].[ContarRolesConComponente]
    @CodPermiso INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Alternativa 2.1 del CU-005-027 [#ERR042]
    SELECT COUNT(*) AS Cantidad
    FROM [dbo].[Rol_Permiso] RP
    WHERE RP.CodPermiso = @CodPermiso
END
GO

CREATE PROCEDURE [dbo].[ContarFamiliasQueContienen]
    @CodPermiso INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Alternativa 2.2 del CU-005-027 [#ERR043]
    SELECT COUNT(*) AS Cantidad
    FROM [dbo].[Permiso_Componente] PC
    WHERE PC.CodHijo = @CodPermiso
END
GO

CREATE PROCEDURE [dbo].[TraerRelacionesPermisos]
AS
BEGIN
    SET NOCOUNT ON;

    -- Todas las aristas del arbol de una sola vez. Armar el arbol en memoria
    -- a partir de esta foto evita una consulta por cada familia que se abre.
    SELECT
        PC.CodPadre,
        PC.CodHijo
    FROM [dbo].[Permiso_Componente] PC
END
GO
