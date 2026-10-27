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
          CU-002-005 Generar Recomendaciones
          CU-002-006 Generar Predicciones
          CU-002-007 Comparar Precios con Competidores
     005. Gestion de Usuarios y Permisos
          CU-005-017 Consultar Usuarios
          CU-005-018 Alta Usuario
          CU-005-019 Baja Usuario
          CU-005-020 Modificar Usuario
     006. Integraciones y Sincronizacion
          CU-006-029 Configurar Integracion
          CU-006-030 Importar y sincronizar Datos
          CU-006-031 Consultar Historial de Sincronizaciones
          (archivos XML de ejemplo en ScriptsSql/ArchivosEjemplo)
     009. Suscripciones
          CU-009-042 Renovar Suscripcion
          CU-009-043 Baja Suscripcion
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

DELETE FROM [dbo].[Recomendacion]
WHERE [IdProducto] IN (SELECT [IdProducto] FROM [dbo].[Producto] WHERE [Codigo] LIKE 'SKU-1%')
GO

DELETE FROM [dbo].[Producto] WHERE [Codigo] LIKE 'SKU-1%'
GO

DELETE FROM [dbo].[Competencia]
WHERE [Nombre] IN (N'TecnoStore Online', N'MegaCompu', N'Gamer Point', N'Insumos del Sur')
GO

-- La suscripcion de la instancia solo tiene los periodos de prueba
DELETE FROM [dbo].[Suscripcion]
GO

-- Las integraciones de la instancia son solo las de prueba
DELETE FROM [dbo].[HistorialSincronizacion]
GO

DELETE FROM [dbo].[MapeoColumna]
GO

DELETE FROM [dbo].[ConfiguracionIntegracion]
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
-- (Clave y Salt: PBKDF2 HMAC-SHA256, 100.000 iteraciones, en base64)
INSERT INTO [dbo].[Usuario]
    ([NombreUsuario], [Nombre], [Apellido], [Email], [Clave], [Salt], [CodRol], [Bloqueado], [Activo], [ContFallidos])
VALUES
    (N'lgarcia',    N'Lucía',  N'García',    N'lucia.garcia@strategic.local',    'MTwnbvXw4CIMuUcy8fpliCK1skCAuLgLqeJf+IZB9II=', 'WGTB4sU6JmRk9KKgjyjR9A==', 2, 0, 1, 0),
    (N'mfernandez', N'Martín', N'Fernández', N'martin.fernandez@strategic.local', 'zdKh4G9jJZWSbhllcwQmLCvkCO95MAKoMiYMcoWbbFs=', 'XxEPRET0EmKo8+bR5HKCXA==', 3, 0, 1, 0),
    (N'svega',      N'Sofía',  N'Vega',      N'sofia.vega@strategic.local',      'PA8bOrpegg6nDwaYwxSoVC1GZI1/eQnc95SoDIKI+Js=', 'YnjP4Hy8BjbY5uWqUqYXIQ==', 4, 0, 1, 0),
    (N'rlopez',     N'Ramiro', N'López',     N'ramiro.lopez@strategic.local',    'jQwrKheogMGfCEj1xX15QfLmoQec7eQWBVgEZozvaHQ=', 'pAyR3GyUc6HfqPR/NLegPA==', 3, 0, 0, 0),
    (N'jperez',     N'Juan',   N'Pérez',     N'juan.perez@strategic.local',      'ctzZH760RFP1gI9fEx80kVA1DRiRdZpwjeNAoA+i0AA=', 'Cm45fylw3gRLLwBWTJFXbw==', 4, 1, 1, 3)
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
    (6, N'SKU-1006', N'Webcam HD (discontinuada)',      N'Inactivo', 25000.00,   0, NULL, NULL, N'Periféricos', 1, '2026-07-01T03:00:00', N'Genius'),
    -- SKU-1007 y SKU-1008 quedan mas de un 15% por encima de la competencia
    -- para las recomendaciones de precio (CU-002-005)
    (7, N'SKU-1007', N'Parlante portátil Bluetooth 20W', N'Activo',  78000.00,  10,  40,  5, N'Audio',       0, '2026-09-05T03:00:00', N'JBL'),
    (8, N'SKU-1008', N'Silla gamer ergonómica',          N'Activo', 210000.00,   6,  20,  3, N'Gaming',      0, '2026-09-05T03:00:00', N'Corsair')

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
    (5, 890000.00, '2026-09-01T09:00:00'),

    -- Puntos intermedios para que la regresion lineal (CU-002-006) tenga
    -- una serie con algo de forma y no solo tres registros
    (1,  41200.00, '2026-07-10T09:00:00'),
    (1,  43000.00, '2026-07-28T09:00:00'),
    (1,  43200.00, '2026-08-05T09:00:00'),
    (1,  44600.00, '2026-08-25T09:00:00'),

    (2,  17200.00, '2026-07-15T09:00:00'),
    (2,  17500.00, '2026-07-25T09:00:00'),
    (2,  18000.00, '2026-08-12T09:00:00'),
    (2,  18200.00, '2026-08-22T09:00:00'),

    (3, 307000.00, '2026-07-20T09:00:00'),
    (3, 309500.00, '2026-07-30T09:00:00'),
    (3, 315000.00, '2026-08-20T09:00:00'),
    (3, 318000.00, '2026-08-28T09:00:00'),

    (5, 878000.00, '2026-08-08T09:00:00'),
    (5, 881000.00, '2026-08-15T09:00:00'),
    (5, 884000.00, '2026-08-22T09:00:00')
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
    (5,  4, '2026-09-05T09:00:00'),

    -- Puntos intermedios para la prediccion de stock. SKU-1004 se deja con
    -- dos registros a proposito: no alcanza para ajustar una recta
    (1, 33, '2026-07-10T09:00:00'),
    (1, 22, '2026-08-01T09:00:00'),
    (1, 15, '2026-08-25T09:00:00'),

    (2, 24, '2026-07-18T09:00:00'),
    (2, 10, '2026-08-15T09:00:00'),
    (2,  6, '2026-08-28T09:00:00'),

    (3, 13, '2026-07-25T09:00:00'),
    (3,  8, '2026-08-25T09:00:00'),

    (5,  6, '2026-08-15T09:00:00')
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
    (6, N'V-2026-0006', '2026-09-02T15:45:00',  99001.00, N'Pendiente'),

    -- Ventas de un solo item repartidas por semanas, para que la demanda
    -- semanal (CU-002-006) tenga una serie que proyectar
    ( 7, N'V-2026-0007', '2026-07-06T11:00:00',  40000.00, N'Completada'),
    ( 8, N'V-2026-0008', '2026-07-15T12:30:00',  34000.00, N'Completada'),
    ( 9, N'V-2026-0009', '2026-07-22T10:10:00', 127500.00, N'Completada'),
    (10, N'V-2026-0010', '2026-07-29T16:20:00',  86000.00, N'Completada'),
    (11, N'V-2026-0011', '2026-08-06T09:40:00', 129600.00, N'Completada'),
    (12, N'V-2026-0012', '2026-08-12T14:15:00',  88000.00, N'Completada'),
    (13, N'V-2026-0013', '2026-08-21T11:50:00', 178400.00, N'Completada'),
    (14, N'V-2026-0014', '2026-08-27T17:05:00', 132000.00, N'Completada'),
    (15, N'V-2026-0015', '2026-09-03T10:30:00', 225000.00, N'Completada'),
    (16, N'V-2026-0016', '2026-07-09T13:00:00',  34000.00, N'Completada'),
    (17, N'V-2026-0017', '2026-07-23T15:30:00',  52500.00, N'Completada'),
    (18, N'V-2026-0018', '2026-08-04T12:00:00',  53400.00, N'Completada'),
    (19, N'V-2026-0019', '2026-08-18T18:45:00',  72000.00, N'Completada'),
    (20, N'V-2026-0020', '2026-08-29T10:20:00',  91000.00, N'Completada'),
    (21, N'V-2026-0021', '2026-09-04T16:00:00',  92502.50, N'Completada'),

    -- Recomendaciones (CU-002-005): los periodos terminan en la ultima
    -- sincronizacion (05/09). Mes actual 07/08 al 05/09, anterior 08/07 al 06/08
    (22, N'V-2026-0022', '2026-07-14T11:00:00', 156000.00, N'Completada'),
    (23, N'V-2026-0023', '2026-07-27T15:20:00', 234000.00, N'Completada'),
    (24, N'V-2026-0024', '2026-08-05T10:45:00',  78000.00, N'Completada'),
    (25, N'V-2026-0025', '2026-08-25T12:10:00', 156000.00, N'Completada'),
    (26, N'V-2026-0026', '2026-07-30T17:00:00', 210000.00, N'Completada'),
    (27, N'V-2026-0027', '2026-08-14T09:30:00', 420000.00, N'Completada'),
    (28, N'V-2026-0028', '2026-09-01T14:00:00', 210000.00, N'Completada')

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
    (6, 4, 1,  62000.00),

    -- Teclado (producto 1): la demanda sube semana a semana
    ( 7, 1, 1,  40000.00),
    ( 8, 2, 2,  17000.00),
    ( 9, 1, 3,  42500.00),
    (10, 1, 2,  43000.00),
    (11, 1, 3,  43200.00),
    (12, 1, 2,  44000.00),
    (13, 1, 4,  44600.00),
    (14, 1, 3,  44000.00),
    (15, 1, 5,  45000.00),
    -- Mouse (producto 2): demanda pareja
    (16, 2, 2,  17000.00),
    (17, 2, 3,  17500.00),
    (18, 2, 3,  17800.00),
    (19, 2, 4,  18000.00),
    (20, 2, 5,  18200.00),
    (21, 2, 5,  18500.50),
    -- Parlante (producto 7): 6 unidades el mes anterior y 2 el actual
    (22, 7, 2,  78000.00),
    (23, 7, 3,  78000.00),
    (24, 7, 1,  78000.00),
    (25, 7, 2,  78000.00),
    -- Silla (producto 8): 1 unidad el mes anterior y 3 el actual
    (26, 8, 1, 210000.00),
    (27, 8, 2, 210000.00),
    (28, 8, 1, 210000.00)
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
    (9, 6, 1, N'https://articulo.mercadolibre.com.ar/MLA-1109-webcam-hd-720p-_JM',                  N'Activa',     '2026-06-20T09:00:00', '2026-07-01T06:00:00'),
    (10, 7, 1, N'https://articulo.mercadolibre.com.ar/MLA-1110-parlante-bluetooth-20w-_JM',         N'Activa',     '2026-07-20T09:00:00', '2026-09-05T06:00:00'),
    (11, 8, 3, N'https://articulo.mercadolibre.com.ar/MLA-1111-silla-gamer-ergonomica-_JM',         N'Activa',     '2026-07-20T09:05:00', '2026-09-05T06:00:00')

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
     - mapeos 10 y 11: el producto propio queda mas de un 15% mas
       caro (recomendaciones de precio del CU-002-005)
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

    (8,  19000.00,  18500.50, '2026-09-05T06:00:00'),

    (10,  64000.00,  76000.00, '2026-08-20T06:00:00'),
    (10,  64000.00,  78000.00, '2026-09-05T06:00:00'),

    (11, 182000.00, 205000.00, '2026-08-20T06:00:00'),
    (11, 175000.00, 210000.00, '2026-09-05T06:00:00')
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
   se ven en Comparar Precios. El conteo tiene que dar 9.
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

/* ------------------------------------------------------------
   Suscripcion
   Historial de la suscripcion del cliente: un periodo mensual
   del plan Max pagado por fila, de marzo a noviembre de 2026,
   con dos subas de precio. El ultimo periodo esta Activo; los anteriores ya
   vencieron. Los numeros de operacion estan encriptados con AES
   y la clave ClaveAES del Web.config; en claro son:
     84213307561, 84391122045, TRF-0051872, 85017734219,
     85244098113, TRF-0063390, 85871206654, 86102345987
   ------------------------------------------------------------ */

INSERT INTO [dbo].[Suscripcion]
    ([FechaPago], [FechaInicio], [FechaVencimiento], [Estado], [Plan], [Monto], [NumeroOperacion])
VALUES
    ('2026-03-02T10:15:00', '2026-03-02T10:15:00', '2026-04-02T10:15:00', N'Vencida', N'Max',         35000.00, N'/rB1hU/RFLTBO556I41d5EuajH8QAmOD2yJ6kMw/BxM='),
    ('2026-03-30T18:40:00', '2026-04-02T10:15:00', '2026-05-02T10:15:00', N'Vencida', N'Max',         35000.00, N'prKxwfp2gp6HOVSOA/aF56fcCq1d0lT007E39v0pbP0='),
    ('2026-05-04T09:05:00', '2026-05-04T09:05:00', '2026-06-04T09:05:00', N'Vencida', N'Max',         35000.00, N'WBW6YJW5PttfF77nyrzrW1lMY5hE90Iz/rIKDv/XTa4='),
    ('2026-06-01T11:20:00', '2026-06-04T09:05:00', '2026-07-04T09:05:00', N'Vencida', N'Max',         38500.00, N'lJF3OFOvIPUzkXAoxACvCTueh4ISKXZAEq9tvn2gIp8='),
    ('2026-07-03T16:45:00', '2026-07-04T09:05:00', '2026-08-04T09:05:00', N'Vencida', N'Max',         38500.00, N'lWbg10umji+AlCwU/3dR5bFdLaarSlx+07ZD92DnMLc='),
    ('2026-08-03T12:00:00', '2026-08-04T09:05:00', '2026-09-04T09:05:00', N'Vencida', N'Max',         38500.00, N'LHDXLUw7SOGWp7EKu4chlx4RCxmVDYvfi9Oq9VYS8uQ='),
    ('2026-09-02T08:30:00', '2026-09-04T09:05:00', '2026-10-04T09:05:00', N'Vencida', N'Max',         42000.00, N'8a1FJr+g4+9iuQn/1tH1oTBNBvKhUY3V7XGZy9b5C60='),
    ('2026-10-01T19:10:00', '2026-10-04T09:05:00', '2026-11-04T09:05:00', N'Activa',  N'Max',         42000.00, N'CLaaW8lfalIv9dLX/hwMQNI4ule3DFvg8DohM4AHviA=')
GO

/* ------------------------------------------------------------
   Integraciones (modulo 006)
   Productos y Ventas ya tienen el mapeo configurado con los
   nombres de columna de los archivos de ScriptsSql/ArchivosEjemplo
   (productos.xml y ventas.xml), asi que se pueden importar
   directamente. Stock queda sin configurar a proposito: sirve
   para probar el CU-006-029 con stock.xml y el aviso #ERR044 al
   intentar importar sin configuracion.
   El historial trae una sincronizacion de cada estado.
   ------------------------------------------------------------ */

SET IDENTITY_INSERT [dbo].[ConfiguracionIntegracion] ON
INSERT INTO [dbo].[ConfiguracionIntegracion] ([IdConfiguracion], [TipoArchivo], [FechaConfiguracion]) VALUES
    (1, N'Productos', '2026-07-30T10:00:00'),
    (2, N'Ventas',    '2026-07-30T10:20:00')
SET IDENTITY_INSERT [dbo].[ConfiguracionIntegracion] OFF
GO

INSERT INTO [dbo].[MapeoColumna] ([IdConfiguracion], [ColumnaStrategic], [ColumnaCliente]) VALUES
    (1, N'Codigo',         N'SKU'),
    (1, N'Nombre',         N'Descripcion'),
    (1, N'Precio',         N'PrecioVenta'),
    (1, N'Stock',          N'CantDisponible'),
    (1, N'Estado',         N'Situacion'),
    (1, N'Categoria',      N'Rubro'),
    (1, N'Marca',          N'Fabricante'),
    (1, N'StockMinimo',    N'StockMin'),
    (1, N'StockMaximo',    N'StockMax'),
    (2, N'NroVenta',       N'NumeroPedido'),
    (2, N'Fecha',          N'FechaPedido'),
    (2, N'CodigoProducto', N'SKU'),
    (2, N'Cantidad',       N'Unidades'),
    (2, N'PrecioVenta',    N'PrecioUnitario'),
    (2, N'Estado',         N'EstadoPedido')
GO

INSERT INTO [dbo].[HistorialSincronizacion]
    ([IdConfiguracion], [TipoArchivo], [RegistrosImportados], [RegistrosActualizados], [RegistrosError], [Estado], [Detalle], [FechaEjecucion])
VALUES
    (1, N'Productos', 8, 0, 0, N'Exitosa', NULL, '2026-08-01T09:00:00'),
    (2, N'Ventas', 12, 0, 2, N'ConErrores',
        N'Registro 4: el producto SKU-9999 no existe en el catálogo.' + CHAR(10) +
        N'Registro 9: Cantidad debe ser un número entero mayor o igual a 1 (0).',
        '2026-08-20T18:00:00'),
    (1, N'Productos', 0, 0, 0, N'Fallida',
        N'No se encontró la columna CantDisponible (campo Stock) en el archivo.',
        '2026-09-05T06:00:00'),
    (1, N'Productos', 0, 8, 0, N'Exitosa', NULL, '2026-09-05T06:10:00'),
    (2, N'Ventas', 16, 0, 0, N'Exitosa', NULL, '2026-09-10T08:30:00')
GO

/* ------------------------------------------------------------
   Los datos de prueba se cargan por fuera del sistema, asi que
   el digito verificador guardado ya no sirve. Se limpia para que
   el proximo inicio de sesion tome estos datos como referencia
   en vez de reportarlos como inconsistencia.
   ------------------------------------------------------------ */

UPDATE [dbo].[DigitoVerificador] SET [DVH] = NULL, [DVV] = NULL
DELETE FROM [dbo].[DVH_Fila]
DELETE FROM [dbo].[DVV_Columna]
GO

PRINT 'Datos de prueba cargados correctamente'
GO
