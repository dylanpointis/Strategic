/* ============================================================
   Strategic - Datos de prueba
   ------------------------------------------------------------
   Carga un juego minimo de datos para probar las pantallas.
   Se puede volver a ejecutar cuantas veces haga falta: primero
   borra los registros que el propio script crea y despues los
   vuelve a insertar.

   Al sumar casos de uso nuevos, agregar los DELETE correspondientes
   en el bloque de limpieza respetando el orden de las claves foraneas
   (primero las tablas hijas, despues las padres).

   Cubre:
     004. Catalogo Sincronizado
          CU-004-013 Ver Productos Sincronizados
          CU-004-014 Ver Historial de Precios
          CU-004-015 Ver Historial de Stock
          CU-004-016 Ver Historial de Ventas
     002. Analisis
          CU-002-007 Comparar Precios con Competidores
     005. Gestion de Usuarios y Permisos
          CU-005-017 Consultar Usuarios
          CU-005-018 Alta Usuario
          CU-005-019 Baja Usuario
          CU-005-020 Modificar Usuario
   ============================================================ */

USE [Strategic]
GO

SET NOCOUNT ON
GO

/* ------------------------------------------------------------
   Limpieza de los datos de prueba anteriores
   ------------------------------------------------------------ */

DELETE FROM [dbo].[Eventos]
WHERE [NombreUsuario] IN (N'lgarcia', N'mfernandez', N'svega', N'rlopez', N'jperez')
GO

DELETE FROM [dbo].[Usuario]
WHERE [NombreUsuario] IN (N'lgarcia', N'mfernandez', N'svega', N'rlopez', N'jperez')
GO

DELETE FROM [dbo].[ItemVenta]
WHERE [IdVenta] IN (SELECT [IdVenta] FROM [dbo].[Venta] WHERE [NroVenta] LIKE 'V-2026-%')
GO

DELETE FROM [dbo].[Venta] WHERE [NroVenta] LIKE 'V-2026-%'
GO

DELETE FROM [dbo].[HistorialPrecioProducto]
WHERE [IdProducto] IN (SELECT [IdProducto] FROM [dbo].[Producto] WHERE [Codigo] LIKE 'SKU-1%')
GO

DELETE FROM [dbo].[HistorialStock]
WHERE [IdProducto] IN (SELECT [IdProducto] FROM [dbo].[Producto] WHERE [Codigo] LIKE 'SKU-1%')
GO

DELETE FROM [dbo].[HistorialPrecioCompetencia]
WHERE [IdProductoCompetencia] IN (
    SELECT PC.[IdProductoCompetencia]
    FROM [dbo].[ProductoCompetencia] PC
    INNER JOIN [dbo].[Producto] P ON P.[IdProducto] = PC.[IdProducto]
    WHERE P.[Codigo] LIKE 'SKU-1%')
GO

DELETE FROM [dbo].[ProductoCompetencia]
WHERE [IdProducto] IN (SELECT [IdProducto] FROM [dbo].[Producto] WHERE [Codigo] LIKE 'SKU-1%')
GO

DELETE FROM [dbo].[Producto] WHERE [Codigo] LIKE 'SKU-1%'
GO

DELETE FROM [dbo].[Competencia]
WHERE [Nombre] IN (N'TecnoStore Online', N'MegaCompu', N'Gamer Point', N'Insumos del Sur')
GO


/* ------------------------------------------------------------
   Usuario
   Usuarios de prueba para el modulo 005. La clave de cada uno es
   la que genera el alta del CU-005-018: nombre.apellido en
   minusculas y sin acentos, guardada encriptada con SHA256.
   Por ejemplo, lgarcia entra con la clave lucia.garcia.
   Casos preparados a proposito:
     - rlopez esta dado de baja, para probar la reactivacion
     - jperez esta bloqueado por intentos fallidos, para probar
       el desbloqueo desde Modificar Usuario
     - lgarcia es el segundo administrador: sin el, el sistema no
       deja dar de baja al usuario Admin
   ------------------------------------------------------------ */

-- Usuario: lgarcia     Clave: 'lucia.garcia'      Rol: Administrador
-- Usuario: mfernandez  Clave: 'martin.fernandez'  Rol: Analista
-- Usuario: svega       Clave: 'sofia.vega'        Rol: Usuario
-- Usuario: rlopez      Clave: 'ramiro.lopez'      Rol: Analista (dado de baja)
-- Usuario: jperez      Clave: 'juan.perez'        Rol: Usuario (bloqueado)
INSERT INTO [dbo].[Usuario]
    ([NombreUsuario], [Nombre], [Apellido], [Email], [Clave], [CodRol], [Bloqueado], [Activo], [ContFallidos])
VALUES
    (N'lgarcia',    N'Lucía',  N'García',    N'lucia.garcia@strategic.local',    'c9258789b1901050d66382d62c2aa13c13cdbacee2f0e327f8ce89988e6e3747', 2, 0, 1, 0),
    (N'mfernandez', N'Martín', N'Fernández', N'martin.fernandez@strategic.local', '2ebc59dfdd0d2bf245ccf856f53de38dbd8556261d4e26b8397df10ba451c81a', 3, 0, 1, 0),
    (N'svega',      N'Sofía',  N'Vega',      N'sofia.vega@strategic.local',      'b153f79f48d794b70efc5519c53f1f20afc5e6d5be1ef41ec8845a76f82313d9', 4, 0, 1, 0),
    (N'rlopez',     N'Ramiro', N'López',     N'ramiro.lopez@strategic.local',    '32fa5f6205b3b0b6b6569ef2db23f41e1124e798573d3f3cd2c4c01907cb4fbe', 3, 0, 0, 0),
    (N'jperez',     N'Juan',   N'Pérez',     N'juan.perez@strategic.local',      'c15f3831e1a4a86046b22fc49f728fafe6e83f9782588dc1701ae5a1a971ef82', 4, 1, 1, 3)
GO


/* ------------------------------------------------------------
   Producto
   Los identificadores se fijan a mano para poder referenciarlos
   desde el resto de las tablas del script.
   SKU-1006 queda con borrado logico para verificar que no aparece
   en el listado de productos sincronizados.
   ------------------------------------------------------------ */

SET IDENTITY_INSERT [dbo].[Producto] ON

INSERT INTO [dbo].[Producto]
    ([IdProducto], [Codigo], [Nombre], [Estado], [Precio], [Stock], [StockMaximo], [StockMinimo], [Categoria], [BorradoLogico], [FechaSincronizacion], [Marca])
VALUES
    (1, N'SKU-1001', N'Teclado mecánico RGB 87 teclas', N'Activo',   45000.00,  12, 100,  5, N'Periféricos', 0, '2026-09-05T03:00:00', N'Redragon'),
    (2, N'SKU-1002', N'Mouse inalámbrico 2.4 GHz',      N'Activo',   18500.50,   3,  80, 10, N'Periféricos', 0, '2026-09-05T03:00:00', N'Logitech'),
    (3, N'SKU-1003', N'Monitor 27" 144 Hz',             N'Activo',  320000.00,   7,  20,  2, N'Monitores',   0, '2026-09-05T03:00:00', N'Samsung'),
    (4, N'SKU-1004', N'Auriculares Bluetooth ANC',      N'Pausado',  62000.00,   0,  50,  4, N'Audio',       0, '2026-09-05T03:00:00', N'JBL'),
    (5, N'SKU-1005', N'Notebook 14" Core i5 16GB',      N'Activo',  890000.00,   4,  15,  2, N'Notebooks',   0, '2026-09-05T03:00:00', N'Lenovo'),
    (6, N'SKU-1006', N'Webcam HD (discontinuada)',      N'Inactivo', 25000.00,   0, NULL, NULL, N'Periféricos', 1, '2026-07-01T03:00:00', N'Genius')

SET IDENTITY_INSERT [dbo].[Producto] OFF
GO


/* ------------------------------------------------------------
   HistorialPrecioProducto
   SKU-1004 se deja sin historial a proposito, para poder ver el
   mensaje "El producto no posee historial de precios".
   ------------------------------------------------------------ */

INSERT INTO [dbo].[HistorialPrecioProducto] ([IdProducto], [Precio], [Fecha])
VALUES
    (1,  40000.00, '2026-07-01T09:00:00'),
    (1,  42500.00, '2026-07-20T09:00:00'),
    (1,  44000.00, '2026-08-15T09:00:00'),
    (1,  45000.00, '2026-09-01T09:00:00'),

    (2,  17000.00, '2026-07-05T09:00:00'),
    (2,  17800.00, '2026-08-01T09:00:00'),
    (2,  18500.50, '2026-09-02T09:00:00'),

    (3, 305000.00, '2026-07-10T09:00:00'),
    (3, 312000.00, '2026-08-10T09:00:00'),
    (3, 320000.00, '2026-09-01T09:00:00'),

    (5, 875000.00, '2026-08-01T09:00:00'),
    (5, 890000.00, '2026-09-01T09:00:00')
GO


/* ------------------------------------------------------------
   HistorialStock
   ------------------------------------------------------------ */

INSERT INTO [dbo].[HistorialStock] ([IdProducto], [Stock], [Fecha])
VALUES
    (1, 40, '2026-07-01T09:00:00'),
    (1, 25, '2026-07-20T09:00:00'),
    (1, 18, '2026-08-15T09:00:00'),
    (1, 12, '2026-09-05T09:00:00'),

    (2, 30, '2026-07-05T09:00:00'),
    (2, 15, '2026-08-01T09:00:00'),
    (2,  3, '2026-09-05T09:00:00'),

    (3, 15, '2026-07-10T09:00:00'),
    (3, 10, '2026-08-10T09:00:00'),
    (3,  7, '2026-09-05T09:00:00'),

    (4,  6, '2026-08-05T09:00:00'),
    (4,  0, '2026-09-05T09:00:00'),

    (5,  8, '2026-08-01T09:00:00'),
    (5,  4, '2026-09-05T09:00:00')
GO


/* ------------------------------------------------------------
   Venta e ItemVenta
   El MontoTotal de cada venta coincide con la suma de sus items.
   ------------------------------------------------------------ */

SET IDENTITY_INSERT [dbo].[Venta] ON

INSERT INTO [dbo].[Venta] ([IdVenta], [NroVenta], [Fecha], [MontoTotal], [Estado])
VALUES
    (1, N'V-2026-0001', '2026-07-12T10:15:00',  85000.00, N'Completada'),
    (2, N'V-2026-0002', '2026-07-12T16:40:00',  17000.00, N'Completada'),
    (3, N'V-2026-0003', '2026-08-03T11:20:00', 322800.00, N'Completada'),
    (4, N'V-2026-0004', '2026-08-19T09:05:00', 875000.00, N'Completada'),
    (5, N'V-2026-0005', '2026-08-19T19:30:00', 132000.00, N'Cancelada'),
    (6, N'V-2026-0006', '2026-09-02T15:45:00',  99001.00, N'Pendiente')

SET IDENTITY_INSERT [dbo].[Venta] OFF
GO

INSERT INTO [dbo].[ItemVenta] ([IdVenta], [IdProducto], [Cantidad], [PrecioVenta])
VALUES
    (1, 1, 2,  42500.00),

    (2, 2, 1,  17000.00),

    (3, 3, 1, 305000.00),
    (3, 2, 1,  17800.00),

    (4, 5, 1, 875000.00),

    (5, 1, 3,  44000.00),

    (6, 2, 2,  18500.50),
    (6, 4, 1,  62000.00)
GO


/* ------------------------------------------------------------
   Competencia
   Competidores registrados con el CU-007-033 Alta Competidor.
   "Insumos del Sur" queda inactivo para verificar que no aparece
   en el filtro de competidores de la comparacion de precios.
   ------------------------------------------------------------ */

SET IDENTITY_INSERT [dbo].[Competencia] ON

INSERT INTO [dbo].[Competencia]
    ([IdCompetencia], [Nombre], [Marketplace], [Descripcion], [Estado])
VALUES
    (1, N'TecnoStore Online', N'MercadoLibre', N'Tienda oficial de periféricos y accesorios', N'Activo'),
    (2, N'MegaCompu',         N'MercadoLibre', N'Mayorista de notebooks y monitores',        N'Activo'),
    (3, N'Gamer Point',       N'MercadoLibre', N'Tienda especializada en gaming',            N'Activo'),
    (4, N'Insumos del Sur',   N'MercadoLibre', N'Competidor que dejó de operar',             N'Inactivo')

SET IDENTITY_INSERT [dbo].[Competencia] OFF
GO


/* ------------------------------------------------------------
   ProductoCompetencia
   Cada fila es un producto propio asociado a una publicacion de
   la competencia, como los que se cargan con el CU-007-036
   Mapear Productos con Competencia.
   Casos preparados a proposito:
     - el mapeo 7 se muestra sin precio de la competencia porque
       todavia no tiene ninguna consulta registrada
     - el mapeo 8 esta finalizado (no se monitorea mas) y el 9 apunta
       al producto con borrado logico: los dos quedan fuera de la
       comparacion de precios
   El Estado usa el vocabulario de CU-007-037 (Activa / Pausada /
   Finalizada / No encontrada / Error), no Activo/Inactivo.
   ------------------------------------------------------------ */

SET IDENTITY_INSERT [dbo].[ProductoCompetencia] ON

INSERT INTO [dbo].[ProductoCompetencia]
    ([IdProductoCompetencia], [IdProducto], [IdCompetencia], [URL], [Estado], [FechaAlta], [FechaUltimaVerificacion])
VALUES
    (1, 1, 1, N'https://articulo.mercadolibre.com.ar/MLA-1101-teclado-mecanico-rgb-87-teclas-_JM',  N'Activa',     '2026-07-10T10:00:00', '2026-09-05T06:00:00'),
    (2, 1, 3, N'https://articulo.mercadolibre.com.ar/MLA-1102-teclado-gamer-mecanico-rgb-_JM',      N'Activa',     '2026-07-10T10:05:00', '2026-09-05T06:00:00'),
    (3, 2, 1, N'https://articulo.mercadolibre.com.ar/MLA-1103-mouse-inalambrico-24ghz-_JM',         N'Activa',     '2026-07-10T10:10:00', '2026-09-05T06:00:00'),
    (4, 3, 2, N'https://articulo.mercadolibre.com.ar/MLA-1104-monitor-27-pulgadas-144hz-_JM',       N'Activa',     '2026-07-12T09:00:00', '2026-09-05T06:00:00'),
    (5, 3, 3, N'https://articulo.mercadolibre.com.ar/MLA-1105-monitor-gamer-27-144hz-_JM',          N'Activa',     '2026-08-15T09:00:00', '2026-09-05T06:00:00'),
    (6, 5, 2, N'https://articulo.mercadolibre.com.ar/MLA-1106-notebook-14-core-i5-16gb-_JM',        N'Activa',     '2026-08-01T09:00:00', '2026-09-08T06:00:00'),
    (7, 4, 1, N'https://articulo.mercadolibre.com.ar/MLA-1107-auriculares-bluetooth-anc-_JM',       N'Activa',     '2026-09-07T18:00:00', NULL),
    (8, 2, 2, N'https://articulo.mercadolibre.com.ar/MLA-1108-mouse-inalambrico-oficina-_JM',       N'Finalizada', '2026-07-15T09:00:00', '2026-09-05T06:00:00'),
    (9, 6, 1, N'https://articulo.mercadolibre.com.ar/MLA-1109-webcam-hd-720p-_JM',                  N'Activa',     '2026-06-20T09:00:00', '2026-07-01T06:00:00')

SET IDENTITY_INSERT [dbo].[ProductoCompetencia] OFF
GO


/* ------------------------------------------------------------
   HistorialPrecioCompetencia
   Cada fila es una consulta del precio de la publicacion. Se guarda
   tambien el precio propio de ese momento, que coincide con el que
   figura en HistorialPrecioProducto para la misma fecha.
   Situaciones que se pueden ver en la pantalla:
     - mapeo 1: el producto propio quedo mas caro que la competencia
     - mapeo 2: el producto propio quedo mas barato
     - mapeo 3: los dos precios terminan iguales
     - mapeo 6: la competencia bajo el precio por debajo del propio
   ------------------------------------------------------------ */

INSERT INTO [dbo].[HistorialPrecioCompetencia]
    ([IdProductoCompetencia], [PrecioCompetencia], [PrecioPropio], [FechaConsulta])
VALUES
    (1,  41000.00,  40000.00, '2026-07-15T06:00:00'),
    (1,  42000.00,  42500.00, '2026-08-01T06:00:00'),
    (1,  42500.00,  44000.00, '2026-08-20T06:00:00'),
    (1,  43200.00,  45000.00, '2026-09-05T06:00:00'),

    (2,  47000.00,  40000.00, '2026-07-15T06:00:00'),
    (2,  48000.00,  44000.00, '2026-08-20T06:00:00'),
    (2,  49500.00,  45000.00, '2026-09-05T06:00:00'),

    (3,  16500.00,  17000.00, '2026-07-15T06:00:00'),
    (3,  17900.00,  17800.00, '2026-08-20T06:00:00'),
    (3,  18500.50,  18500.50, '2026-09-05T06:00:00'),

    (4, 299000.00, 305000.00, '2026-07-15T06:00:00'),
    (4, 308000.00, 312000.00, '2026-08-20T06:00:00'),
    (4, 315000.00, 320000.00, '2026-09-05T06:00:00'),

    (5, 335000.00, 312000.00, '2026-08-20T06:00:00'),
    (5, 339900.00, 320000.00, '2026-09-05T06:00:00'),

    (6, 920000.00, 875000.00, '2026-08-05T06:00:00'),
    (6, 905000.00, 890000.00, '2026-09-05T06:00:00'),
    (6, 869000.00, 890000.00, '2026-09-08T06:00:00'),

    (8,  19000.00,  18500.50, '2026-09-05T06:00:00')
GO


/* ------------------------------------------------------------
   Control: verifica que el MontoTotal de cada venta coincida
   con la suma de sus items. No deberia devolver ninguna fila.
   ------------------------------------------------------------ */

SELECT
    V.NroVenta,
    V.MontoTotal,
    SUM(IV.Cantidad * IV.PrecioVenta) AS SumaItems
FROM [dbo].[Venta] V
INNER JOIN [dbo].[ItemVenta] IV ON IV.IdVenta = V.IdVenta
WHERE V.NroVenta LIKE 'V-2026-%'
GROUP BY V.NroVenta, V.MontoTotal
HAVING V.MontoTotal <> SUM(IV.Cantidad * IV.PrecioVenta)
GO

/* ------------------------------------------------------------
   Control: los mapeos activos de productos vigentes son los que
   se ven en Comparar Precios. El conteo tiene que dar 7.
   ------------------------------------------------------------ */

SELECT COUNT(*) AS MapeosComparables
FROM [dbo].[ProductoCompetencia] PC
INNER JOIN [dbo].[Producto] P ON P.[IdProducto] = PC.[IdProducto]
WHERE P.[BorradoLogico] = 0
  AND PC.[Estado] = N'Activa'
  AND P.[Codigo] LIKE 'SKU-1%'
GO

/* ------------------------------------------------------------
   Control: usuarios de prueba por estado. Deberia devolver
   3 activos, 1 inactivo y 1 bloqueado.
   ------------------------------------------------------------ */

SELECT
    SUM(CASE WHEN [Activo] = 1 AND [Bloqueado] = 0 THEN 1 ELSE 0 END) AS Activos,
    SUM(CASE WHEN [Activo] = 0 THEN 1 ELSE 0 END)                     AS Inactivos,
    SUM(CASE WHEN [Bloqueado] = 1 THEN 1 ELSE 0 END)                  AS Bloqueados
FROM [dbo].[Usuario]
WHERE [NombreUsuario] IN (N'lgarcia', N'mfernandez', N'svega', N'rlopez', N'jperez')
GO

PRINT 'Datos de prueba cargados correctamente'
GO
