# Strategic

Proyecto Universitario

Aplicación web de análisis de datos e inteligencia competitiva, desarrollada en ASP.NET WebForms (C#) sobre .NET Framework 4.7.2 y SQL Server.

Permite centralizar y analizar información de ventas, productos, stock y precios; visualizar indicadores mediante dashboards, generar predicciones y recomendaciones estratégicas, comparar precios con la competencia y automatizar procesos para optimizar la toma de decisiones.

## Requisitos

- Windows.
- Visual Studio 2022.
- .NET Framework 4.7.2.
- SQL Server.
- IIS Express.

## Instalación y ejecución

1. Clonar el repositorio.
2. Abrir `Strategic.sln` con Visual Studio 2022.
3. Restaurar los paquetes NuGet.
4. Ejecutar los scripts SQL en SQL Server, en este orden:

bash

```bash
sqlcmd -S localhost -E -i ScriptsSql/bdscript.sql
sqlcmd -S localhost -E -i ScriptsSql/datosprueba.sql
```

5. Verificar la cadena de conexión en `Strategic/Web.config`.
6. Compilar la solución y ejecutar el proyecto `Strategic` desde Visual Studio.

## Usuarios de prueba

| Usuario | Contraseña | Rol |
| --- | --- | --- |
| `WebMasterStrategic` | `WebMaster123.` | WebMaster |
| `lgarcia` | `lucia.garcia` | Administrador |
| `mfernandez` | `martin.fernandez` | Analista |
| `svega` | `sofia.vega` | Usuario |
| `EmergenciaStrategic` | `Emergencia2026!` | WebMaster de emergencia |

**Nota:** El usuario de emergencia está configurado en `Web.config` y permite acceder al sistema ante problemas con la tabla de usuarios o permisos.

Los usuarios de prueba están precargados en `ScriptsSql/datosprueba.sql`.
