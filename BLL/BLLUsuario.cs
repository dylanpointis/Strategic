using BE;
using DAL;
using Services;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BLL
{
    public class BLLUsuario
    {
        private const string ModuloSesiones = "Sesiones";
        private const string ModuloUsuarios = "Usuarios";

        // Criticidad de los eventos: 1 informativo, 2 cambio de datos, 3 grave
        private const int CriticidadInformativa = 1;
        private const int CriticidadCambio = 2;
        private const int CriticidadGrave = 3;

        private const int IntentosAntesDeBloquear = 3;

        /// <summary>
        /// Roles habilitados para gestionar usuarios. Queda fijo hasta que esté
        /// el sistema de permisos: en ese momento la lista sale de consultar
        /// qué roles tienen el permiso SEGURIDAD_USUARIOS.
        /// </summary>
        private static readonly int[] RolesConGestionDeUsuarios = new int[] { 1, 2 };

        private readonly DALUsuario dalUsuario = new DALUsuario();
        private readonly BLLEvento bllEvento = new BLLEvento();

        #region Sesion

        public BEUsuario Login(string nombreUsuario, string clave)
        {
            BEUsuario usuario = ValidarUsuario(nombreUsuario, string.Empty);

            if (usuario == null)
            {
                throw new Exception("El usuario ingresado no existe");
            }

            if (usuario.Bloqueado || !usuario.Activo)
            {
                throw new Exception("El usuario se encuentra bloqueado o desactivado, comuniquese con el administrador del sistema");
            }

            if (Encriptacion.EncriptarSHA256(clave) != usuario.Clave)
            {
                int intentosFallidos = usuario.ContFallidos + 1;
                ModificarContFallido(usuario.NombreUsuario, intentosFallidos);

                if (intentosFallidos >= IntentosAntesDeBloquear)
                {
                    ModificarBloqueo(usuario.NombreUsuario, true);
                    bllEvento.RegistrarEvento(new Evento(usuario.NombreUsuario, ModuloSesiones, "Usuario bloqueado", CriticidadGrave));
                    throw new Exception("Se ha bloqueado al usuario por superar el limite de intentos fallidos, por favor comuniquese con el administrador del sistema");
                }

                throw new Exception("Las credenciales no coinciden");
            }

            ModificarContFallido(usuario.NombreUsuario, 0);
            bllEvento.RegistrarEvento(new Evento(usuario.NombreUsuario, ModuloSesiones, "Inicio sesion", CriticidadInformativa));

            return usuario;
        }

        /// <summary>
        /// Deja registrado en la bitácora que el usuario cerró su sesión. Se
        /// llama antes de limpiar la sesión, que es de donde sale el nombre.
        /// </summary>
        public void RegistrarCierreSesion(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                return;
            }

            bllEvento.RegistrarEvento(new Evento(nombreUsuario, ModuloSesiones, "Cierre sesion", CriticidadInformativa));
        }

        public BEUsuario ValidarUsuario(string nombreUsuario, string email)
        {
            return dalUsuario.ValidarUsuario(nombreUsuario, email);
        }

        public void ModificarContFallido(string nombreUsuario, int contFallidos)
        {
            dalUsuario.ModificarContFallido(nombreUsuario, contFallidos);
        }

        public void ModificarBloqueo(string nombreUsuario, bool bloqueo)
        {
            dalUsuario.ModificarBloqueo(nombreUsuario, bloqueo);
        }

        #endregion

        #region Consulta - CU-005-017

        public List<BEUsuario> TraerListaUsuarios()
        {
            return dalUsuario.TraerListaUsuarios();
        }

        public List<BEUsuario> FiltrarUsuarios(string texto, int? codRol, bool? activo)
        {
            if (codRol.HasValue && codRol.Value <= 0)
            {
                throw new Exception("Debe seleccionar un rol valido");
            }

            return dalUsuario.FiltrarUsuarios(Normalizar(texto), codRol, activo);
        }

        public BEUsuario TraerUsuarioPorNombre(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                throw new Exception("Debe seleccionar un usuario");
            }

            return dalUsuario.TraerUsuarioPorNombre(nombreUsuario.Trim());
        }

        #endregion

        #region Alta - CU-005-018

        /// <summary>
        /// Registra el usuario y devuelve la clave por defecto sin encriptar,
        /// para que la pantalla se la pueda mostrar al administrador una única
        /// vez: en la base solo queda guardado el hash.
        /// </summary>
        public string AltaUsuario(BEUsuario usuario, string nombreUsuarioEnSesion)
        {
            ValidarDatos(usuario, true);

            // Alternativa 5.1 del CU [#ERR021]
            if (dalUsuario.ContarUsuariosConDatos(usuario.NombreUsuario, usuario.Email, null) > 0)
            {
                throw new Exception("El nombre de usuario o el email ya se encuentra registrado");
            }

            string clavePorDefecto = ClavePorDefecto.Generar(usuario.Nombre, usuario.Apellido);

            usuario.Clave = Encriptacion.EncriptarSHA256(clavePorDefecto);
            usuario.Activo = true;
            usuario.Bloqueado = false;
            usuario.ContFallidos = 0;

            dalUsuario.AltaUsuario(usuario);

            RegistrarEventoDeUsuarios(nombreUsuarioEnSesion, "Alta de usuario", usuario.NombreUsuario);

            return clavePorDefecto;
        }

        #endregion

        #region Baja - CU-005-019

        public void CambiarEstadoUsuario(string nombreUsuario, bool activo, string nombreUsuarioEnSesion)
        {
            BEUsuario usuario = TraerUsuarioPorNombre(nombreUsuario);

            if (usuario == null)
            {
                throw new Exception("No se encontro el usuario seleccionado");
            }

            if (usuario.Activo == activo)
            {
                throw new Exception(activo
                    ? "El usuario ya se encuentra activo"
                    : "El usuario ya se encuentra dado de baja");
            }

            if (!activo)
            {
                // Alternativa 2.1 del CU [#ERR022]
                ValidarQueNoSeaElUsuarioEnSesion(usuario.NombreUsuario, nombreUsuarioEnSesion);
                ValidarQueQuedeAlgunAdministrador(usuario);
            }

            dalUsuario.ModificarEstado(usuario.NombreUsuario, activo);

            RegistrarEventoDeUsuarios(
                nombreUsuarioEnSesion,
                activo ? "Reactivacion de usuario" : "Baja de usuario",
                usuario.NombreUsuario);
        }

        #endregion

        #region Modificacion - CU-005-020

        public void ModificarUsuario(BEUsuario usuario, string nombreUsuarioEnSesion)
        {
            ValidarDatos(usuario, false);

            BEUsuario actual = TraerUsuarioPorNombre(usuario.NombreUsuario);

            if (actual == null)
            {
                throw new Exception("No se encontro el usuario seleccionado");
            }

            // Alternativa 4.1 del CU [#ERR021]: el email no puede estar en uso
            // por otro usuario, pero si puede seguir siendo el propio
            if (dalUsuario.ContarUsuariosConDatos(null, usuario.Email, usuario.NombreUsuario) > 0)
            {
                throw new Exception("El email ya se encuentra registrado en otro usuario");
            }

            // Si al usuario le sacan el rol de gestion, tiene que quedar
            // algun otro administrador habilitado
            if (EsRolDeGestion(actual.CodRol) && !EsRolDeGestion(usuario.CodRol))
            {
                ValidarQueQuedeAlgunAdministrador(actual);
            }

            dalUsuario.ModificarUsuario(usuario);

            RegistrarEventoDeUsuarios(nombreUsuarioEnSesion, "Modificacion de usuario", usuario.NombreUsuario);

            if (actual.Bloqueado && !usuario.Bloqueado)
            {
                RegistrarEventoDeUsuarios(nombreUsuarioEnSesion, "Desbloqueo de usuario", usuario.NombreUsuario);
            }
        }

        #endregion

        #region Validaciones

        private void ValidarDatos(BEUsuario usuario, bool esAlta)
        {
            if (usuario == null)
            {
                throw new Exception("No se recibieron los datos del usuario");
            }

            // Alternativa de campos requeridos incompletos [#ERR001]
            ExigirTexto(usuario.NombreUsuario, "el nombre de usuario", 50);
            ExigirTexto(usuario.Nombre, "el nombre", 50);
            ExigirTexto(usuario.Apellido, "el apellido", 50);
            ExigirTexto(usuario.Email, "el email", 100);

            if (!EsEmailValido(usuario.Email))
            {
                throw new Exception("El email ingresado no tiene un formato valido");
            }

            if (usuario.CodRol <= 0)
            {
                throw new Exception("Debe seleccionar un rol para el usuario");
            }

            if (esAlta && !EsNombreUsuarioValido(usuario.NombreUsuario))
            {
                throw new Exception("El nombre de usuario solo puede tener letras, numeros, punto, guion y guion bajo");
            }
        }

        private void ValidarQueNoSeaElUsuarioEnSesion(string nombreUsuario, string nombreUsuarioEnSesion)
        {
            if (EsElMismoUsuario(nombreUsuario, nombreUsuarioEnSesion))
            {
                throw new Exception("No se puede dar de baja al usuario con el que se inicio la sesion");
            }
        }

        /// <summary>
        /// Evita que el sistema quede sin ningún usuario capaz de administrar
        /// usuarios. Sin esta validación un administrador podría dar de baja al
        /// último que queda y nadie podría volver a entrar a gestionarlos.
        /// Un usuario bloqueado no cuenta: no puede iniciar sesión.
        /// </summary>
        private void ValidarQueQuedeAlgunAdministrador(BEUsuario usuarioAfectado)
        {
            if (!EsRolDeGestion(usuarioAfectado.CodRol))
            {
                return;
            }

            List<BEUsuario> usuarios = dalUsuario.TraerListaUsuarios();
            int administradores = 0;

            foreach (BEUsuario usuario in usuarios)
            {
                if (EsElMismoUsuario(usuario.NombreUsuario, usuarioAfectado.NombreUsuario))
                {
                    continue;
                }

                if (usuario.Activo && !usuario.Bloqueado && EsRolDeGestion(usuario.CodRol))
                {
                    administradores = administradores + 1;
                }
            }

            if (administradores == 0)
            {
                throw new Exception("No se puede dejar al sistema sin ningun administrador habilitado");
            }
        }

        private bool EsRolDeGestion(int codRol)
        {
            foreach (int rol in RolesConGestionDeUsuarios)
            {
                if (rol == codRol)
                {
                    return true;
                }
            }

            return false;
        }

        // El nombre de usuario es la clave primaria y la base no distingue
        // mayusculas de minusculas, asi que la comparacion tampoco
        private bool EsElMismoUsuario(string uno, string otro)
        {
            return string.Equals(uno, otro, StringComparison.OrdinalIgnoreCase);
        }

        private void ExigirTexto(string valor, string campo, int largoMaximo)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                throw new Exception(string.Format("Debe completar {0}", campo));
            }

            if (valor.Trim().Length > largoMaximo)
            {
                throw new Exception(string.Format("El valor de {0} supera los {1} caracteres", campo, largoMaximo));
            }
        }

        private bool EsEmailValido(string email)
        {
            return Regex.IsMatch(email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$");
        }

        private bool EsNombreUsuarioValido(string nombreUsuario)
        {
            return Regex.IsMatch(nombreUsuario.Trim(), @"^[A-Za-z0-9._-]+$");
        }

        #endregion

        private void RegistrarEventoDeUsuarios(string nombreUsuarioEnSesion, string accion, string nombreUsuarioAfectado)
        {
            // El evento guarda quien ejecuto la accion; sobre quien se ejecuto
            // va en la descripcion
            bllEvento.RegistrarEvento(new Evento(
                nombreUsuarioEnSesion,
                ModuloUsuarios,
                string.Format("{0}: {1}", accion, nombreUsuarioAfectado),
                CriticidadCambio));
        }

        private string Normalizar(string valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }
    }
}
