/* ============================================================
   Strategic - Datos de prueba
   ------------------------------------------------------------
   Carga un juego de datos para probar las pantallas, con ventas
   desde enero de 2026 hasta el dia en que se corre. Las ventas
   recientes y la suscripcion activa usan GETDATE().
   Se puede volver a ejecutar cuantas veces haga falta: primero
   borra los registros que el propio script crea y despues los
   vuelve a insertar.

   Al sumar casos de uso nuevos, agregar los DELETE correspondientes
   en el bloque de limpieza respetando el orden de las claves foraneas
   (primero las tablas hijas, despues las padres).

   Cubre:
     003. Automatizaciones
          CU-003-009 Consultar Automatizaciones
          CU-003-010 Pausar o Reactivar Automatizacion
          CU-003-011 Modificar parametro de Automatizacion
          CU-003-012 Ver Historial de Automatizaciones Ejecutadas
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

DELETE FROM [dbo].[HistorialAutomatizacion]
GO

DELETE FROM [dbo].[Automatizacion]
GO

DELETE FROM [dbo].[Recomendacion]
WHERE [IdProducto] IN (SELECT [IdProducto] FROM [dbo].[Producto] WHERE [Codigo] LIKE 'SKU-1%')
GO

DELETE FROM [dbo].[Producto] WHERE [Codigo] LIKE 'SKU-1%'
GO

DELETE FROM [dbo].[Competencia]
WHERE [Nombre] IN (N'TecnoStore Online', N'MegaCompu', N'Gamer Point', N'Insumos del Sur', N'ElectroMundo')
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
    (8, N'SKU-1008', N'Silla gamer ergonómica',          N'Activo', 210000.00,   6,  20,  3, N'Gaming',      0, '2026-09-05T03:00:00', N'Corsair'),
    -- SKU-1009 a SKU-1014 completan 14 productos para ver el paginado de
    -- las grillas. Tienen stock sobre el minimo, ventas parejas y precios
    -- cerca de la competencia: no suman recomendaciones ni automatizaciones
    (9,  N'SKU-1009', N'Disco SSD NVMe 1TB',               N'Activo',  95000.00,  14,  40,  4, N'Almacenamiento', 0, '2026-09-05T03:00:00', N'Kingston'),
    (10, N'SKU-1010', N'Memoria RAM 16GB DDR4',            N'Activo',  52000.00,  20,  60,  6, N'Componentes',    0, '2026-09-05T03:00:00', N'Kingston'),
    (11, N'SKU-1011', N'Impresora multifunción WiFi',      N'Activo', 280000.00,   5,  15,  2, N'Impresión',      0, '2026-09-05T03:00:00', N'HP'),
    (12, N'SKU-1012', N'Router WiFi 6 doble banda',        N'Activo',  89000.00,   9,  30,  3, N'Redes',          0, '2026-09-05T03:00:00', N'TP-Link'),
    (13, N'SKU-1013', N'Tablet 10" 64GB',                  N'Activo', 410000.00,   6,  18,  2, N'Tablets',        0, '2026-09-05T03:00:00', N'Samsung'),
    (14, N'SKU-1014', N'Smartwatch deportivo',             N'Activo', 135000.00,  11,  30,  3, N'Wearables',      0, '2026-09-05T03:00:00', N'Xiaomi')

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
   Datos de enero a junio de 2026
   Ventas, precios y stock mas viejos, para los filtros por fecha,
   los reportes y las predicciones. Terminan antes del 08/07: asi no
   entran en los periodos de las recomendaciones (CU-002-005).
   SKU-1004 sigue sin historial de precios a proposito.
   ------------------------------------------------------------ */

SET IDENTITY_INSERT [dbo].[Venta] ON

INSERT INTO [dbo].[Venta] ([IdVenta], [NroVenta], [Fecha], [MontoTotal], [Estado])
VALUES
    (101, N'V-2026-0101', '2026-01-08T10:30:00',    87000.00, N'Completada'),
    (102, N'V-2026-0102', '2026-01-16T15:10:00',    72000.00, N'Completada'),
    (103, N'V-2026-0103', '2026-01-27T11:45:00',   280000.00, N'Completada'),
    (104, N'V-2026-0104', '2026-02-05T09:50:00',    45600.00, N'Completada'),
    (105, N'V-2026-0105', '2026-02-13T17:20:00',   835200.00, N'Completada'),
    (106, N'V-2026-0106', '2026-02-24T12:35:00',   190000.00, N'Cancelada'),
    (107, N'V-2026-0107', '2026-03-04T10:05:00',   111000.00, N'Completada'),
    (108, N'V-2026-0108', '2026-03-12T16:30:00',   146000.00, N'Completada'),
    (109, N'V-2026-0109', '2026-03-23T13:15:00',   226000.00, N'Completada'),
    (110, N'V-2026-0110', '2026-04-02T09:25:00',   288000.00, N'Completada'),
    (111, N'V-2026-0111', '2026-04-11T18:00:00',   149000.00, N'Completada'),
    (112, N'V-2026-0112', '2026-04-22T11:40:00',    63200.00, N'Completada'),
    (113, N'V-2026-0113', '2026-05-06T14:20:00',   840000.00, N'Completada'),
    (114, N'V-2026-0114', '2026-05-15T10:50:00',   115500.00, N'Completada'),
    (115, N'V-2026-0115', '2026-05-26T16:45:00',   400000.00, N'Completada'),
    (116, N'V-2026-0116', '2026-06-03T12:10:00',   182600.00, N'Completada'),
    (117, N'V-2026-0117', '2026-06-12T09:35:00',   296000.00, N'Cancelada'),
    (118, N'V-2026-0118', '2026-06-19T17:25:00',    78000.00, N'Completada'),
    (119, N'V-2026-0119', '2026-06-29T11:05:00',   899500.00, N'Completada')

SET IDENTITY_INSERT [dbo].[Venta] OFF
GO

INSERT INTO [dbo].[ItemVenta] ([IdVenta], [IdProducto], [Cantidad], [PrecioVenta])
VALUES
    (101, 1, 2,   36000.00),
    (101, 2, 1,   15000.00),
    (102, 7, 1,   72000.00),
    (103, 3, 1,  280000.00),
    (104, 2, 3,   15200.00),
    (105, 5, 1,  820000.00),
    (105, 2, 1,   15200.00),
    (106, 8, 1,  190000.00),
    (107, 1, 3,   37000.00),
    (108, 7, 2,   73000.00),
    (109, 8, 1,  195000.00),
    (109, 2, 2,   15500.00),
    (110, 3, 1,  288000.00),
    (111, 1, 2,   37500.00),
    (111, 7, 1,   74000.00),
    (112, 2, 4,   15800.00),
    (113, 5, 1,  840000.00),
    (114, 1, 3,   38500.00),
    (115, 8, 2,  200000.00),
    (116, 7, 2,   75000.00),
    (116, 2, 2,   16300.00),
    (117, 3, 1,  296000.00),
    (118, 1, 2,   39000.00),
    (119, 5, 1,  860000.00),
    (119, 1, 1,   39500.00)
GO

INSERT INTO [dbo].[HistorialPrecioProducto] ([IdProducto], [Precio], [Fecha])
VALUES
    (1,   36000.00, '2026-01-05T09:00:00'),
    (1,   37000.00, '2026-03-01T09:00:00'),
    (1,   38500.00, '2026-05-01T09:00:00'),
    (1,   39500.00, '2026-06-20T09:00:00'),
    (2,   15000.00, '2026-01-05T09:00:00'),
    (2,   15500.00, '2026-03-10T09:00:00'),
    (2,   16300.00, '2026-05-20T09:00:00'),
    (3,  280000.00, '2026-01-10T09:00:00'),
    (3,  288000.00, '2026-03-25T09:00:00'),
    (3,  296000.00, '2026-05-30T09:00:00'),
    (5,  820000.00, '2026-02-01T09:00:00'),
    (5,  840000.00, '2026-04-15T09:00:00'),
    (5,  860000.00, '2026-06-15T09:00:00'),
    (7,   72000.00, '2026-01-05T09:00:00'),
    (7,   74000.00, '2026-04-01T09:00:00'),
    (7,   75000.00, '2026-06-01T09:00:00'),
    (8,  190000.00, '2026-02-01T09:00:00'),
    (8,  200000.00, '2026-05-01T09:00:00')
GO

INSERT INTO [dbo].[HistorialStock] ([IdProducto], [Stock], [Fecha])
VALUES
    (1,  68, '2026-01-05T09:00:00'),
    (1,  59, '2026-03-01T09:00:00'),
    (1,  48, '2026-05-01T09:00:00'),
    (2,  62, '2026-01-10T09:00:00'),
    (2,  50, '2026-03-05T09:00:00'),
    (2,  38, '2026-05-15T09:00:00'),
    (3,  19, '2026-02-01T09:00:00'),
    (3,  17, '2026-05-01T09:00:00'),
    (5,  12, '2026-02-01T09:00:00'),
    (5,  10, '2026-05-01T09:00:00'),
    (7,  28, '2026-01-15T09:00:00'),
    (7,  21, '2026-04-15T09:00:00'),
    (8,  13, '2026-02-15T09:00:00'),
    (8,  10, '2026-05-15T09:00:00')
GO


/* ------------------------------------------------------------
   Productos SKU-1009 a SKU-1014
   Historial de precios y de stock de enero a septiembre, ventas de
   enero a junio y una venta en cada periodo de las recomendaciones.
   Esas ventas son la excepcion a no cargar ventas entre el 08/07 y
   el 05/09: sin ventas en esos periodos, cada producto nuevo sumaria
   una recomendacion de sobrestock. Venden las mismas unidades en los
   dos periodos y su stock no supera tres meses de venta, asi que no
   cambian las recomendaciones del CU-002-005.
   ------------------------------------------------------------ */

INSERT INTO [dbo].[HistorialPrecioProducto] ([IdProducto], [Precio], [Fecha])
VALUES
    (9,   84000.00, '2026-01-10T09:00:00'),
    (9,   88000.00, '2026-04-10T09:00:00'),
    (9,   92000.00, '2026-07-01T09:00:00'),
    (9,   95000.00, '2026-09-01T09:00:00'),
    (10,  46000.00, '2026-01-10T09:00:00'),
    (10,  48500.00, '2026-04-10T09:00:00'),
    (10,  50500.00, '2026-07-01T09:00:00'),
    (10,  52000.00, '2026-09-01T09:00:00'),
    (11, 245000.00, '2026-01-10T09:00:00'),
    (11, 260000.00, '2026-04-10T09:00:00'),
    (11, 272000.00, '2026-07-01T09:00:00'),
    (11, 280000.00, '2026-09-01T09:00:00'),
    (12,  78000.00, '2026-01-10T09:00:00'),
    (12,  82000.00, '2026-04-10T09:00:00'),
    (12,  86500.00, '2026-07-01T09:00:00'),
    (12,  89000.00, '2026-09-01T09:00:00'),
    (13, 360000.00, '2026-01-10T09:00:00'),
    (13, 380000.00, '2026-04-10T09:00:00'),
    (13, 398000.00, '2026-07-01T09:00:00'),
    (13, 410000.00, '2026-09-01T09:00:00'),
    (14, 118000.00, '2026-01-10T09:00:00'),
    (14, 125000.00, '2026-04-10T09:00:00'),
    (14, 131000.00, '2026-07-01T09:00:00'),
    (14, 135000.00, '2026-09-01T09:00:00')
GO

INSERT INTO [dbo].[HistorialStock] ([IdProducto], [Stock], [Fecha])
VALUES
    (9,  30, '2026-01-10T09:00:00'),
    (9,  24, '2026-04-10T09:00:00'),
    (9,  19, '2026-07-01T09:00:00'),
    (9,  14, '2026-09-05T09:00:00'),
    (10, 45, '2026-01-10T09:00:00'),
    (10, 36, '2026-04-10T09:00:00'),
    (10, 27, '2026-07-01T09:00:00'),
    (10, 20, '2026-09-05T09:00:00'),
    (11, 12, '2026-01-10T09:00:00'),
    (11,  9, '2026-04-10T09:00:00'),
    (11,  7, '2026-07-01T09:00:00'),
    (11,  5, '2026-09-05T09:00:00'),
    (12, 22, '2026-01-10T09:00:00'),
    (12, 17, '2026-04-10T09:00:00'),
    (12, 13, '2026-07-01T09:00:00'),
    (12,  9, '2026-09-05T09:00:00'),
    (13, 14, '2026-01-10T09:00:00'),
    (13, 11, '2026-04-10T09:00:00'),
    (13,  9, '2026-07-01T09:00:00'),
    (13,  6, '2026-09-05T09:00:00'),
    (14, 25, '2026-01-10T09:00:00'),
    (14, 20, '2026-04-10T09:00:00'),
    (14, 15, '2026-07-01T09:00:00'),
    (14, 11, '2026-09-05T09:00:00')
GO

SET IDENTITY_INSERT [dbo].[Venta] ON

INSERT INTO [dbo].[Venta] ([IdVenta], [NroVenta], [Fecha], [MontoTotal], [Estado])
VALUES
    -- Enero a junio
    (120, N'V-2026-0120', '2026-01-20T10:30:00',  168000.00, N'Completada'),
    (121, N'V-2026-0121', '2026-04-15T15:10:00',  264000.00, N'Completada'),
    (122, N'V-2026-0122', '2026-02-10T11:45:00',  184000.00, N'Completada'),
    (123, N'V-2026-0123', '2026-05-12T09:50:00',  242500.00, N'Completada'),
    (124, N'V-2026-0124', '2026-03-18T17:20:00',  245000.00, N'Completada'),
    (125, N'V-2026-0125', '2026-06-08T12:35:00',  520000.00, N'Completada'),
    (126, N'V-2026-0126', '2026-02-25T10:05:00',  156000.00, N'Completada'),
    (127, N'V-2026-0127', '2026-05-20T16:30:00',  246000.00, N'Completada'),
    (128, N'V-2026-0128', '2026-03-09T13:15:00',  360000.00, N'Completada'),
    (129, N'V-2026-0129', '2026-06-15T09:25:00',  760000.00, N'Completada'),
    (130, N'V-2026-0130', '2026-01-28T18:00:00',  236000.00, N'Completada'),
    (131, N'V-2026-0131', '2026-04-27T11:40:00',  375000.00, N'Completada'),
    -- Mes anterior (08/07 al 06/08) y mes actual (07/08 al 05/09) de las recomendaciones
    (29, N'V-2026-0029', '2026-07-16T10:00:00',  460000.00, N'Completada'),
    (30, N'V-2026-0030', '2026-08-24T10:00:00',  460000.00, N'Completada'),
    (31, N'V-2026-0031', '2026-07-17T11:00:00',  353500.00, N'Completada'),
    (32, N'V-2026-0032', '2026-08-25T11:00:00',  353500.00, N'Completada'),
    (33, N'V-2026-0033', '2026-07-21T12:00:00',  544000.00, N'Completada'),
    (34, N'V-2026-0034', '2026-08-26T12:00:00',  544000.00, N'Completada'),
    (35, N'V-2026-0035', '2026-07-22T15:00:00',  346000.00, N'Completada'),
    (36, N'V-2026-0036', '2026-08-27T15:00:00',  346000.00, N'Completada'),
    (37, N'V-2026-0037', '2026-07-24T16:00:00', 1194000.00, N'Completada'),
    (38, N'V-2026-0038', '2026-08-28T16:00:00', 1194000.00, N'Completada'),
    (39, N'V-2026-0039', '2026-07-28T17:00:00',  524000.00, N'Completada'),
    (40, N'V-2026-0040', '2026-08-31T17:00:00',  524000.00, N'Completada')

SET IDENTITY_INSERT [dbo].[Venta] OFF
GO

INSERT INTO [dbo].[ItemVenta] ([IdVenta], [IdProducto], [Cantidad], [PrecioVenta])
VALUES
    (120,  9, 2,  84000.00),
    (121,  9, 3,  88000.00),
    (122, 10, 4,  46000.00),
    (123, 10, 5,  48500.00),
    (124, 11, 1, 245000.00),
    (125, 11, 2, 260000.00),
    (126, 12, 2,  78000.00),
    (127, 12, 3,  82000.00),
    (128, 13, 1, 360000.00),
    (129, 13, 2, 380000.00),
    (130, 14, 2, 118000.00),
    (131, 14, 3, 125000.00),
    (29,   9, 5,  92000.00),
    (30,   9, 5,  92000.00),
    (31,  10, 7,  50500.00),
    (32,  10, 7,  50500.00),
    (33,  11, 2, 272000.00),
    (34,  11, 2, 272000.00),
    (35,  12, 4,  86500.00),
    (36,  12, 4,  86500.00),
    (37,  13, 3, 398000.00),
    (38,  13, 3, 398000.00),
    (39,  14, 4, 131000.00),
    (40,  14, 4, 131000.00)
GO


/* ------------------------------------------------------------
   Ventas recientes, calculadas con la fecha en que se corre el script
   Una venta cada 4 dias desde el 08/09 hasta el mes anterior, y 12
   ventas repartidas dentro del mes en curso: el Dashboard abre con
   ese periodo, asi que siempre tiene datos sin filtrar por fechas.
   Empiezan despues de la ultima sincronizacion de los productos
   (05/09): tampoco cambian las recomendaciones del CU-002-005.
   Los precios son los actuales de cada producto.
   ------------------------------------------------------------ */

DECLARE @Ahora DATETIME = GETDATE()
DECLARE @InicioMes DATETIME = DATEADD(DAY, 1 - DAY(@Ahora), CAST(CAST(@Ahora AS DATE) AS DATETIME))
DECLARE @Fecha DATETIME = '2026-09-08T10:00:00'
DECLARE @Indice INT = 0
DECLARE @Numero INT = 1
DECLARE @Total INT
DECLARE @IdVenta INT

DECLARE @Fechas TABLE ([Numero] INT IDENTITY(1,1) PRIMARY KEY, [Fecha] DATETIME NOT NULL)

-- Plan de productos que se repite: cada venta toma una fila, y una de
-- cada tres suma la que esta a cinco lugares
DECLARE @Plan TABLE ([Orden] INT PRIMARY KEY, [IdProducto] INT NOT NULL, [Cantidad] INT NOT NULL)

INSERT INTO @Plan ([Orden], [IdProducto], [Cantidad])
VALUES (0, 1, 2), (1, 2, 3), (2, 7, 1), (3, 2, 2), (4, 1, 1), (5, 8, 1), (6, 3, 1), (7, 2, 4), (8, 7, 2), (9, 5, 1)

WHILE @Fecha < @InicioMes
BEGIN
    INSERT INTO @Fechas ([Fecha]) VALUES (@Fecha)
    SET @Fecha = DATEADD(DAY, 4, @Fecha)
END

-- El resto por DAY(@Ahora) reparte las ventas entre el dia 1 y hoy
WHILE @Indice < 12
BEGIN
    SET @Fecha = DATEADD(HOUR, 9 + @Indice % 9, DATEADD(DAY, @Indice % DAY(@Ahora), @InicioMes))
    INSERT INTO @Fechas ([Fecha]) VALUES (CASE WHEN @Fecha > @Ahora THEN @Ahora ELSE @Fecha END)
    SET @Indice = @Indice + 1
END

SELECT @Total = COUNT(*) FROM @Fechas

WHILE @Numero <= @Total
BEGIN
    INSERT INTO [dbo].[Venta] ([NroVenta], [Fecha], [MontoTotal], [Estado])
    SELECT
        N'V-2026-' + CAST(2000 + @Numero AS NVARCHAR(10)),
        [Fecha],
        0,
        CASE WHEN @Numero % 9 = 0 THEN N'Cancelada' WHEN @Numero % 7 = 0 THEN N'Pendiente' ELSE N'Completada' END
    FROM @Fechas
    WHERE [Numero] = @Numero

    SET @IdVenta = SCOPE_IDENTITY()

    INSERT INTO [dbo].[ItemVenta] ([IdVenta], [IdProducto], [Cantidad], [PrecioVenta])
    SELECT @IdVenta, P.[IdProducto], P.[Cantidad], PR.[Precio]
    FROM @Plan P
    INNER JOIN [dbo].[Producto] PR ON PR.[IdProducto] = P.[IdProducto]
    WHERE P.[Orden] = @Numero % 10
       OR (@Numero % 3 = 0 AND P.[Orden] = (@Numero + 5) % 10)

    SET @Numero = @Numero + 1
END

UPDATE V
SET [MontoTotal] = (SELECT SUM(IV.[Cantidad] * IV.[PrecioVenta]) FROM [dbo].[ItemVenta] IV WHERE IV.[IdVenta] = V.[IdVenta])
FROM [dbo].[Venta] V
WHERE V.[NroVenta] LIKE 'V-2026-2%'

-- Puntos recientes de los historiales: el ultimo coincide con el valor actual
INSERT INTO [dbo].[HistorialPrecioProducto] ([IdProducto], [Precio], [Fecha])
SELECT [IdProducto], [Precio], DATEADD(DAY, -3, @Ahora)
FROM [dbo].[Producto]
WHERE [IdProducto] IN (1, 2, 3, 5, 7, 8, 9, 10, 11, 12, 13, 14)

INSERT INTO [dbo].[HistorialStock] ([IdProducto], [Stock], [Fecha])
SELECT P.[IdProducto], P.[Stock] + D.[Extra], DATEADD(DAY, D.[Dias], @Ahora)
FROM [dbo].[Producto] P
CROSS JOIN (VALUES (-40, 6), (-15, 3), (-1, 0)) D ([Dias], [Extra])
WHERE P.[IdProducto] IN (1, 2, 3, 5, 7, 8, 9, 10, 11, 12, 13, 14)
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
    (4, N'Insumos del Sur',   N'MercadoLibre', N'Competidor que dejó de operar',             N'Inactivo'),
    (5, N'ElectroMundo',      N'MercadoLibre', N'Tienda de tecnología, redes y electrónica', N'Activo')

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
    (11, 8, 3, N'https://articulo.mercadolibre.com.ar/MLA-1111-silla-gamer-ergonomica-_JM',         N'Activa',     '2026-07-20T09:05:00', '2026-09-05T06:00:00'),
    (12,  9, 5, N'https://articulo.mercadolibre.com.ar/MLA-1112-disco-ssd-nvme-1tb-_JM',            N'Activa',     '2026-08-10T09:00:00', '2026-09-05T06:00:00'),
    (13, 10, 5, N'https://articulo.mercadolibre.com.ar/MLA-1113-memoria-ram-16gb-ddr4-_JM',         N'Activa',     '2026-08-10T09:05:00', '2026-09-05T06:00:00'),
    (14, 11, 2, N'https://articulo.mercadolibre.com.ar/MLA-1114-impresora-multifuncion-wifi-_JM',   N'Activa',     '2026-08-10T09:10:00', '2026-09-05T06:00:00'),
    (15, 12, 5, N'https://articulo.mercadolibre.com.ar/MLA-1115-router-wifi-6-doble-banda-_JM',     N'Activa',     '2026-08-10T09:15:00', '2026-09-05T06:00:00'),
    (16, 13, 2, N'https://articulo.mercadolibre.com.ar/MLA-1116-tablet-10-64gb-_JM',                N'Activa',     '2026-08-10T09:20:00', '2026-09-05T06:00:00'),
    (17, 14, 5, N'https://articulo.mercadolibre.com.ar/MLA-1117-smartwatch-deportivo-_JM',          N'Activa',     '2026-08-10T09:25:00', '2026-09-05T06:00:00')

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
    (11, 175000.00, 210000.00, '2026-09-05T06:00:00'),

    -- Mapeos 12 a 17: precios cercanos al propio, por encima o por debajo
    (12,  90000.00,  92000.00, '2026-08-20T06:00:00'),
    (12,  93500.00,  95000.00, '2026-09-05T06:00:00'),
    (13,  52000.00,  50500.00, '2026-08-20T06:00:00'),
    (13,  51000.00,  52000.00, '2026-09-05T06:00:00'),
    (14, 279000.00, 272000.00, '2026-08-20T06:00:00'),
    (14, 289000.00, 280000.00, '2026-09-05T06:00:00'),
    (15,  85000.00,  86500.00, '2026-08-20T06:00:00'),
    (15,  87000.00,  89000.00, '2026-09-05T06:00:00'),
    (16, 405000.00, 398000.00, '2026-08-20T06:00:00'),
    (16, 399000.00, 410000.00, '2026-09-05T06:00:00'),
    (17, 129000.00, 131000.00, '2026-08-20T06:00:00'),
    (17, 138000.00, 135000.00, '2026-09-05T06:00:00')
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
   se ven en Comparar Precios. El conteo tiene que dar 15.
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
   del plan Max pagado por fila, de marzo a noviembre de 2026.
   El plan cuesta USD 480 y se cobra en pesos, por eso el monto sube
   con el tipo de cambio: ARS 1.250, 1.400 y 1.535 (el vigente).
   El ultimo periodo esta Activo; los anteriores ya
   vencieron. Los numeros de operacion estan encriptados con AES
   y la clave ClaveAES del Web.config; en claro son:
     84213307561, 84391122045, TRF-0051872, 85017734219,
     85244098113, TRF-0063390, 85871206654, 86102345987
   ------------------------------------------------------------ */

INSERT INTO [dbo].[Suscripcion]
    ([FechaPago], [FechaInicio], [FechaVencimiento], [Estado], [Plan], [Monto], [NumeroOperacion])
VALUES
    ('2026-03-02T10:15:00', '2026-03-02T10:15:00', '2026-04-02T10:15:00', N'Vencida', N'Max',        600000.00, N'/rB1hU/RFLTBO556I41d5EuajH8QAmOD2yJ6kMw/BxM='),
    ('2026-03-30T18:40:00', '2026-04-02T10:15:00', '2026-05-02T10:15:00', N'Vencida', N'Max',        600000.00, N'prKxwfp2gp6HOVSOA/aF56fcCq1d0lT007E39v0pbP0='),
    ('2026-05-04T09:05:00', '2026-05-04T09:05:00', '2026-06-04T09:05:00', N'Vencida', N'Max',        600000.00, N'WBW6YJW5PttfF77nyrzrW1lMY5hE90Iz/rIKDv/XTa4='),
    ('2026-06-01T11:20:00', '2026-06-04T09:05:00', '2026-07-04T09:05:00', N'Vencida', N'Max',        672000.00, N'lJF3OFOvIPUzkXAoxACvCTueh4ISKXZAEq9tvn2gIp8='),
    ('2026-07-03T16:45:00', '2026-07-04T09:05:00', '2026-08-04T09:05:00', N'Vencida', N'Max',        672000.00, N'lWbg10umji+AlCwU/3dR5bFdLaarSlx+07ZD92DnMLc='),
    ('2026-08-03T12:00:00', '2026-08-04T09:05:00', '2026-09-04T09:05:00', N'Vencida', N'Max',        672000.00, N'LHDXLUw7SOGWp7EKu4chlx4RCxmVDYvfi9Oq9VYS8uQ='),
    ('2026-09-02T08:30:00', '2026-09-04T09:05:00', '2026-10-04T09:05:00', N'Vencida', N'Max',        736800.00, N'8a1FJr+g4+9iuQn/1tH1oTBNBvKhUY3V7XGZy9b5C60='),
    ('2026-10-01T19:10:00', '2026-10-04T09:05:00', '2026-11-04T09:05:00', N'Activa',  N'Max',        736800.00, N'CLaaW8lfalIv9dLX/hwMQNI4ule3DFvg8DohM4AHviA=')

-- El periodo activo se calcula con la fecha en que se corre el script: si
-- no, al pasar su vencimiento el login solo dejaria entrar a renovar
UPDATE [dbo].[Suscripcion]
SET [FechaPago] = DATEADD(DAY, -10, GETDATE()),
    [FechaInicio] = DATEADD(DAY, -10, GETDATE()),
    [FechaVencimiento] = DATEADD(MONTH, 1, DATEADD(DAY, -10, GETDATE()))
WHERE [Estado] = N'Activa'
GO

/* ------------------------------------------------------------
   003. Automatizaciones
   Las tres del catalogo, activas, con sus parametros en JSON.
   Al ejecutarlas con estos datos:
     - AjustarPrecio baja SKU-1007 y SKU-1008 (mas de 15% sobre la
       competencia)
     - PausarVenta no actua: el unico producto sin stock ya esta
       pausado. Se ve subiendo el parametro stock a 3 (SKU-1002)
     - AlertaStock avisa por SKU-1002 y SKU-1004
   ------------------------------------------------------------ */

SET IDENTITY_INSERT [dbo].[Automatizacion] ON

INSERT INTO [dbo].[Automatizacion]
    ([IdAutomatizacion], [IdCatalogoAutomatizacion], [Parametros], [Estado], [FechaAlta], [FechaUltimaEjecucion])
VALUES
    (1, 1, N'{"umbral":15,"ajuste":5}', N'Activa', '2026-08-01T09:00:00', '2026-09-08T06:30:00'),
    (2, 2, N'{"stock":0}',              N'Activa', '2026-08-01T09:00:00', '2026-09-08T06:30:00'),
    (3, 3, N'{"margen":0}',             N'Activa', '2026-08-01T09:00:00', '2026-09-08T06:30:00')

SET IDENTITY_INSERT [dbo].[Automatizacion] OFF
GO

INSERT INTO [dbo].[HistorialAutomatizacion]
    ([IdProducto], [IdAutomatizacion], [AccionEjecutada], [ValorAnterior], [ValorNuevo], [Estado], [FechaEjecucion])
VALUES
    (4, 2, N'Venta pausada por falta de stock', N'Activo', N'Pausado', N'Exitosa', '2026-09-05T06:30:00'),
    (2, 3, N'Recomendación generada y aviso enviado al WebMaster', NULL, N'Stock: 3 (mínimo: 10)', N'Exitosa', '2026-09-08T06:30:00'),
    (7, 1, N'No se pudo actualizar el precio del producto', N'78000,00', NULL, N'ConError', '2026-09-08T06:30:00')
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
