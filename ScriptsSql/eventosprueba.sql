/* ============================================================
   Strategic - Eventos de prueba de la bitacora
   ------------------------------------------------------------
   Carga eventos con fechas relativas al dia en que se corre, para
   probar la baja de los eventos anteriores a los ultimos 30 dias
   (BajaEventos.aspx, CU-008-038).

   Resultado esperado si se da de baja ese mismo dia:
     - 26 eventos tienen 31 dias o mas: son los que se dan de baja
     - 14 eventos tienen 30 dias o menos: siguen en la bitacora
   El evento de hace 31 dias se da de baja y el de hace 30 no: la
   baja toma los eventos con fecha anterior a hoy menos 30 dias.

   Se corre despues de datosprueba.sql, porque usa sus usuarios
   (los eventos de un usuario que no existe no se cargan). Se puede
   volver a ejecutar: primero borra los eventos que cargo antes.
   La tabla Eventos no tiene digito verificador, asi que cargar
   eventos por script no genera inconsistencias.
   ============================================================ */

USE [Strategic]
GO

SET NOCOUNT ON
GO

DECLARE @Hoy DATE = CAST(GETDATE() AS DATE)

-- Dias hacia atras, hora, usuario, modulo, texto y criticidad de cada evento
DECLARE @Prueba TABLE (
    [Dias] INT NOT NULL,
    [Hora] VARCHAR(5) NOT NULL,
    [NombreUsuario] VARCHAR(50) NULL,
    [Modulo] VARCHAR(50) NOT NULL,
    [Evento] VARCHAR(100) NOT NULL,
    [Criticidad] SMALLINT NOT NULL
)

INSERT INTO @Prueba ([Dias], [Hora], [NombreUsuario], [Modulo], [Evento], [Criticidad])
VALUES
    -- Anteriores a los ultimos 30 dias: se dan de baja
    (120, '09:02', 'WebMasterStrategic', 'Sesiones',         'Inicio sesion', 1),
    (120, '09:15', 'WebMasterStrategic', 'Usuarios',         'Alta de usuario: svega', 2),
    (118, '10:41', 'lgarcia',            'Sesiones',         'Inicio sesion', 1),
    (115, '11:07', 'lgarcia',            'Roles',            'Alta de rol: Supervisor', 2),
    (110, '08:33', 'svega',              'Sesiones',         'Clave incorrecta', 3),
    (110, '08:34', 'svega',              'Sesiones',         'Inicio sesion', 1),
    (104, '17:26', 'WebMasterStrategic', 'Respaldos',        'Backup realizado: Strategic_Backup_prueba_01.bak', 1),
    ( 98, '12:12', 'lgarcia',            'Familias',         'Alta de familia: Reportes avanzados', 2),
    ( 95, '06:30', NULL,                 'Automatizaciones', 'Automatizaciones ejecutadas: 3 acciones', 2),
    ( 90, '15:48', 'mfernandez',         'Sesiones',         'Acceso denegado: ConsultarUsuarios', 3),
    ( 88, '09:55', 'lgarcia',            'Competencia',      'Alta de competidor: ElectroMundo', 2),
    ( 82, '10:20', 'WebMasterStrategic', 'Integraciones',    'Importacion de Productos: 8 importados, 0 actualizados, 0 con error', 2),
    ( 75, '14:05', 'lgarcia',            'Usuarios',         'Modificacion de usuario: mfernandez', 2),
    ( 70, '18:40', 'lgarcia',            'Suscripciones',    'Renovacion de suscripcion: plan Max', 2),
    ( 66, '07:12', 'svega',              'Sesiones',         'Clave incorrecta', 3),
    ( 66, '07:13', 'svega',              'Sesiones',         'Clave incorrecta', 3),
    ( 66, '07:14', 'svega',              'Sesiones',         'Usuario bloqueado', 3),
    ( 65, '09:30', 'lgarcia',            'Usuarios',         'Modificacion de usuario: svega', 2),
    ( 60, '11:45', 'WebMasterStrategic', 'Respaldos',        'Backup realizado: Strategic_Backup_prueba_02.bak', 1),
    ( 55, '16:10', 'lgarcia',            'Roles',            'Modificacion de rol: Analista', 2),
    ( 50, '06:30', NULL,                 'Automatizaciones', 'Automatizaciones ejecutadas: 2 acciones', 2),
    ( 46, '13:22', 'mfernandez',         'Sesiones',         'Cierre sesion', 1),
    ( 42, '10:03', 'lgarcia',            'Competencia',      'Modificacion de competidor: MegaCompu', 2),
    ( 38, '19:17', 'WebMasterStrategic', 'Integraciones',    'Importacion de Ventas: 12 importados, 0 actualizados, 2 con error', 2),
    ( 35, '08:50', 'lgarcia',            'Familias',         'Modificacion de familia: Reportes avanzados', 2),
    -- Limite: 31 dias, todavia se da de baja
    ( 31, '12:00', 'WebMasterStrategic', 'Sesiones',         'Inicio sesion', 1),

    -- Ultimos 30 dias: se conservan
    -- Limite: 30 dias, ya no se da de baja
    ( 30, '12:00', 'WebMasterStrategic', 'Sesiones',         'Inicio sesion', 1),
    ( 27, '09:40', 'lgarcia',            'Usuarios',         'Alta de usuario: jperez', 2),
    ( 24, '15:05', 'mfernandez',         'Sesiones',         'Acceso denegado: RealizarBackup', 3),
    ( 21, '06:30', NULL,                 'Automatizaciones', 'Automatizaciones ejecutadas: 4 acciones', 2),
    ( 18, '11:11', 'WebMasterStrategic', 'Respaldos',        'Backup realizado: Strategic_Backup_prueba_03.bak', 1),
    ( 15, '10:25', 'lgarcia',            'Roles',            'Baja de rol: Supervisor', 2),
    ( 12, '17:48', 'lgarcia',            'Suscripciones',    'Renovacion de suscripcion: plan Max', 2),
    ( 10, '08:02', 'svega',              'Sesiones',         'Inicio sesion', 1),
    (  8, '14:36', 'lgarcia',            'Competencia',      'Baja de competidor: Insumos del Sur', 2),
    (  6, '09:09', 'WebMasterStrategic', 'Integraciones',    'Importacion de Ventas: 16 importados, 0 actualizados, 0 con error', 2),
    (  4, '16:20', 'mfernandez',         'Sesiones',         'Clave incorrecta', 3),
    (  3, '10:44', 'lgarcia',            'Familias',         'Baja de familia: Reportes avanzados', 2),
    (  2, '13:30', 'lgarcia',            'Usuarios',         'Modificacion de usuario: jperez', 2),
    (  1, '18:05', 'WebMasterStrategic', 'Sesiones',         'Cierre sesion', 1)

-- Limpieza de una carga anterior: coinciden usuario, modulo, texto y hora
DELETE E
FROM [dbo].[Eventos] E
INNER JOIN @Prueba P
    ON  P.[Modulo] = E.[Modulo]
    AND P.[Evento] = E.[Evento]
    AND P.[Hora] = E.[Hora]
    AND ((P.[NombreUsuario] IS NULL AND E.[NombreUsuario] IS NULL) OR P.[NombreUsuario] = E.[NombreUsuario])

INSERT INTO [dbo].[Eventos] ([NombreUsuario], [Modulo], [Evento], [Criticidad], [Fecha], [Hora], [BorradoLogico])
SELECT
    P.[NombreUsuario],
    P.[Modulo],
    P.[Evento],
    P.[Criticidad],
    CONVERT(VARCHAR(10), DATEADD(DAY, -P.[Dias], @Hoy), 23),
    P.[Hora],
    0
FROM @Prueba P
WHERE P.[NombreUsuario] IS NULL
   OR EXISTS (SELECT 1 FROM [dbo].[Usuario] U WHERE U.[NombreUsuario] = P.[NombreUsuario])
ORDER BY P.[Dias] DESC, P.[Hora]

-- Control: cuantos eventos vigentes de toda la bitacora (los de prueba
-- y los que ya tenia) quedarian dados de baja hoy y cuantos se conservan
SELECT
    SUM(CASE WHEN E.[Fecha] < CONVERT(VARCHAR(10), DATEADD(DAY, -30, @Hoy), 23) THEN 1 ELSE 0 END) AS AnterioresA30Dias,
    SUM(CASE WHEN E.[Fecha] >= CONVERT(VARCHAR(10), DATEADD(DAY, -30, @Hoy), 23) THEN 1 ELSE 0 END) AS Ultimos30Dias
FROM [dbo].[Eventos] E
WHERE E.[BorradoLogico] = 0
GO

PRINT 'Eventos de prueba cargados correctamente'
GO
