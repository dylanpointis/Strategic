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
   ============================================================ */

USE [Strategic]
GO

SET NOCOUNT ON
GO

/* ------------------------------------------------------------
   Limpieza de los datos de prueba anteriores
   ------------------------------------------------------------ */

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

DELETE FROM [dbo].[Producto] WHERE [Codigo] LIKE 'SKU-1%'
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

PRINT 'Datos de prueba cargados correctamente'
GO
