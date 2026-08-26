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

INSERT INTO [dbo].[DigitoVerificador] ([Tabla], [DVH], [DVV]) VALUES
('Usuario', NULL, NULL),
('Rol', NULL, NULL),
('Permiso', NULL, NULL),
('Rol_Permiso', NULL, NULL),
('Permiso_Componente', NULL, NULL),
('Eventos', NULL, NULL)
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
