using BE;
using BE.Composite;
using System;
using System.Configuration;

namespace Services
{
    // WebMaster de emergencia para cuando se rompe o se vacia la tabla Usuario.
    // Se define en el Web.config y no en la base. Su rol se arma aca, sin leer
    // las tablas de permisos, para que funcione aunque esten dañadas
    public static class UsuarioEmergencia
    {
        private static readonly string[] Permisos =
        {
            "Dashboard",
            "RepararDigitoVerificador",
            "RealizarBackup",
            "RealizarRestore",
            "ConsultarEventosSistema",
            "ConsultarUsuarios",
            "AltaUsuario",
            "ModificarUsuario"
        };

        public static bool EsUsuarioEmergencia(string nombreUsuario)
        {
            string nombre = ConfigurationManager.AppSettings["UsuarioEmergencia"];

            return !string.IsNullOrEmpty(nombre) && string.Equals(nombre, nombreUsuario, StringComparison.OrdinalIgnoreCase);
        }

        public static bool VerificarClave(string clave)
        {
            return Encriptacion.VerificarPBKDF2(
                clave,
                ConfigurationManager.AppSettings["ClaveEmergenciaSalt"],
                ConfigurationManager.AppSettings["ClaveEmergenciaHash"]);
        }

        public static BEUsuario Crear()
        {
            BERol rol = new BERol(1, "WebMaster");

            for (int i = 0; i < Permisos.Length; i++)
            {
                rol.Componentes.AgregarHijo(new BEPermiso(i + 1, Permisos[i]));
            }

            return new BEUsuario
            {
                NombreUsuario = ConfigurationManager.AppSettings["UsuarioEmergencia"],
                Nombre = "Usuario",
                Apellido = "de emergencia",
                CodRol = rol.CodRol,
                Activo = true,
                Rol = rol
            };
        }
    }
}
