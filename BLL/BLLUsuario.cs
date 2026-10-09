using BE;
using DAL;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
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

        
        /// Roles habilitados para gestionar usuarios. Queda fijo hasta que esté
        /// el sistema de permisos: en ese momento la lista sale de consultar
        /// qué roles tienen el permiso SEGURIDAD_USUARIOS.
        
        private static readonly int[] RolesConGestionDeUsuarios = new int[] { 1, 2 };

        private readonly DALUsuario dalUsuario = new DALUsuario();
        private readonly BLLEvento bllEvento = new BLLEvento();
        private readonly BLLDigitoVerificador bllDigitoVerificador = new BLLDigitoVerificador();

        #region Sesion

        public BEUsuario Login(string nombreUsuario, string clave)
        {
            BEUsuario usuario = ValidarUsuario(nombreUsuario, string.Empty);

            if (usuario == null)
            {
                throw new Exception(Traductor.T("error.usuarioInexistente"));
            }

            if (usuario.Bloqueado || !usuario.Activo)
            {
                throw new Exception(Traductor.T("error.usuarioBloqueado"));
            }

            if (!Encriptacion.VerificarPBKDF2(clave, usuario.Salt, usuario.Clave))
            {
                int intentosFallidos = usuario.ContFallidos + 1;
                ModificarContFallido(usuario.NombreUsuario, intentosFallidos);

                if (intentosFallidos >= IntentosAntesDeBloquear)
                {
                    ModificarBloqueo(usuario.NombreUsuario, true);
                    bllEvento.RegistrarEvento(new Evento(usuario.NombreUsuario, ModuloSesiones, "Usuario bloqueado", CriticidadGrave));
                    throw new Exception(Traductor.T("error.usuarioBloqueadoIntentos"));
                }

                throw new Exception(Traductor.T("error.credencialesInvalidas"));
            }

            // Con las credenciales validas y antes de escribir nada se compara el
            // digito verificador. Tiene que ser antes ya que el propio login pone
            // ContFallidos en 0 en Usuario, y esa escritura recalcula el DV de la
            // tabla, con lo que una alteracion externa quedaria tapada
            List<BEInconsistenciaDV> inconsistencias = bllDigitoVerificador.CompararDV();

            if (inconsistencias.Count > 0)
            {
                // Deja en la bitacora que el login encontro inconsistencias
                string tablas = string.Join(", ", inconsistencias.Select(i => i.Tabla).Distinct());
                bllEvento.RegistrarEvento(new Evento(nombreUsuario, "Integridad", "Inconsistencia de datos detectada en: " + tablas, CriticidadGrave));

                throw new InconsistenciaDVException(usuario, inconsistencias);
            }

            ModificarContFallido(usuario.NombreUsuario, 0);
            bllEvento.RegistrarEvento(new Evento(usuario.NombreUsuario, ModuloSesiones, "Inicio sesion", CriticidadInformativa));

            return usuario;
        }

        
        /// Deja registrado en la bitácora que el usuario cerró su sesión. Se
        /// llama antes de limpiar la sesión, que es de donde sale el nombre.
        
        public void RegistrarCierreSesion(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                return;
            }

            bllEvento.RegistrarEvento(new Evento(nombreUsuario, ModuloSesiones, "Cierre sesion", CriticidadInformativa));
        }

        
        /// Deja registrado que un usuario intentó abrir una pantalla que su
        /// rol no habilita. Es grave porque no pasa navegando por el menú,
        /// que ya filtra lo que no corresponde: pasa escribiendo la dirección.
        
        public void RegistrarAccesoDenegado(string nombreUsuario, string pantalla)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                return;
            }

            bllEvento.RegistrarEvento(new Evento(
                nombreUsuario,
                ModuloSesiones,
                string.Format("Acceso denegado: {0}", pantalla),
                CriticidadGrave));
        }

        public BEUsuario ValidarUsuario(string nombreUsuario, string email)
        {
            return dalUsuario.ValidarUsuario(nombreUsuario, email);
        }

        public void ModificarContFallido(string nombreUsuario, int contFallidos)
        {
            dalUsuario.ModificarContFallido(nombreUsuario, contFallidos);
            bllDigitoVerificador.PersistirDV("Usuario");
        }

        public void ModificarBloqueo(string nombreUsuario, bool bloqueo)
        {
            dalUsuario.ModificarBloqueo(nombreUsuario, bloqueo);
            bllDigitoVerificador.PersistirDV("Usuario");
        }

        // CU-001-003 Cambiar Contrasena. Devuelve el usuario con la clave y la
        // sal nuevas para que la sesion quede al dia
        public BEUsuario CambiarClave(string nombreUsuario, string claveActual, string claveNueva, string confirmacion)
        {
            BEUsuario usuario = ValidarUsuario(nombreUsuario, string.Empty);

            if (usuario == null)
            {
                throw new Exception(Traductor.T("error.usuarioInexistente"));
            }

            // Alternativa 4.1 [#ERR014]
            if (!Encriptacion.VerificarPBKDF2(claveActual, usuario.Salt, usuario.Clave))
            {
                throw new Exception(Traductor.T("error.claveActualIncorrecta"));
            }

            // Alternativa 4.3 [#ERR015]
            if (claveNueva != confirmacion)
            {
                throw new Exception(Traductor.T("error.claveConfirmacion"));
            }

            // Alternativa 4.2 [#ERR016]
            ValidarPoliticaDeClave(claveNueva);

            if (Encriptacion.VerificarPBKDF2(claveNueva, usuario.Salt, usuario.Clave))
            {
                throw new Exception(Traductor.T("error.claveIgualAnterior"));
            }

            string sal;
            usuario.Clave = Encriptacion.EncriptarPBKDF2(claveNueva, out sal);
            usuario.Salt = sal;

            dalUsuario.ModificarClave(usuario.NombreUsuario, usuario.Clave, usuario.Salt);
            bllDigitoVerificador.PersistirDV("Usuario");

            bllEvento.RegistrarEvento(new Evento(usuario.NombreUsuario, ModuloSesiones, "Cambio de clave", CriticidadCambio));

            return usuario;
        }

        // Politica de claves: minimo 8 caracteres, una mayuscula, un numero y
        // un caracter especial
        private void ValidarPoliticaDeClave(string clave)
        {
            if (string.IsNullOrEmpty(clave) || clave.Length < 8)
            {
                throw new Exception(Traductor.T("error.claveLargo"));
            }

            if (!clave.Any(char.IsUpper))
            {
                throw new Exception(Traductor.T("error.claveMayuscula"));
            }

            if (!clave.Any(char.IsDigit))
            {
                throw new Exception(Traductor.T("error.claveNumero"));
            }

            if (!clave.Any(c => !char.IsLetterOrDigit(c)))
            {
                throw new Exception(Traductor.T("error.claveEspecial"));
            }
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
                throw new Exception(Traductor.T("error.rolInvalido"));
            }

            return dalUsuario.FiltrarUsuarios(Normalizar(texto), codRol, activo);
        }

        public BEUsuario TraerUsuarioPorNombre(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                throw new Exception(Traductor.T("error.seleccioneUsuario"));
            }

            return dalUsuario.TraerUsuarioPorNombre(nombreUsuario.Trim());
        }

        #endregion

        #region Alta - CU-005-018

        
        /// Registra el usuario y devuelve la clave por defecto sin encriptar,
        /// para que la pantalla se la pueda mostrar al administrador una única
        /// vez: en la base solo queda guardado el hash.
        
        public string AltaUsuario(BEUsuario usuario, string nombreUsuarioEnSesion)
        {
            ValidarDatos(usuario, true);

            // Alternativa 5.1 del CU [#ERR021]
            if (dalUsuario.ContarUsuariosConDatos(usuario.NombreUsuario, usuario.Email, null) > 0)
            {
                throw new Exception(Traductor.T("error.usuarioDuplicado"));
            }

            string clavePorDefecto = ClavePorDefecto.Generar(usuario.Nombre, usuario.Apellido);

            string salt;
            usuario.Clave = Encriptacion.EncriptarPBKDF2(clavePorDefecto, out salt);
            usuario.Salt = salt;
            usuario.Activo = true;
            usuario.Bloqueado = false;
            usuario.ContFallidos = 0;

            dalUsuario.AltaUsuario(usuario);
            bllDigitoVerificador.PersistirDV("Usuario");

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
                throw new Exception(Traductor.T("error.usuarioNoEncontrado"));
            }

            if (usuario.Activo == activo)
            {
                throw new Exception(activo
                    ? Traductor.T("error.usuarioYaActivo")
                    : Traductor.T("error.usuarioYaDadoDeBaja"));
            }

            if (!activo)
            {
                // Alternativa 2.1 del CU [#ERR022]
                ValidarQueNoSeaElUsuarioEnSesion(usuario.NombreUsuario, nombreUsuarioEnSesion);
                ValidarQueQuedeAlgunAdministrador(usuario);
            }

            dalUsuario.ModificarEstado(usuario.NombreUsuario, activo);
            bllDigitoVerificador.PersistirDV("Usuario");

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
                throw new Exception(Traductor.T("error.usuarioNoEncontrado"));
            }

            // Alternativa 4.1 del CU [#ERR021]: el email no puede estar en uso
            // por otro usuario, pero si puede seguir siendo el propio
            if (dalUsuario.ContarUsuariosConDatos(null, usuario.Email, usuario.NombreUsuario) > 0)
            {
                throw new Exception(Traductor.T("error.emailDuplicado"));
            }

            // Si al usuario le sacan el rol de gestion, tiene que quedar
            // algun otro administrador habilitado
            if (EsRolDeGestion(actual.CodRol) && !EsRolDeGestion(usuario.CodRol))
            {
                ValidarQueQuedeAlgunAdministrador(actual);
            }

            dalUsuario.ModificarUsuario(usuario);
            bllDigitoVerificador.PersistirDV("Usuario");

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
                throw new Exception(Traductor.T("error.sinDatosUsuario"));
            }

            // Alternativa de campos requeridos incompletos [#ERR001]
            ExigirTexto(usuario.NombreUsuario, "campo.elNombreUsuario", 50);
            ExigirTexto(usuario.Nombre, "campo.elNombre", 50);
            ExigirTexto(usuario.Apellido, "campo.elApellido", 50);
            ExigirTexto(usuario.Email, "campo.elEmail", 100);

            if (!EsEmailValido(usuario.Email))
            {
                throw new Exception(Traductor.T("usuario.formatoEmail"));
            }

            if (usuario.CodRol <= 0)
            {
                throw new Exception(Traductor.T("error.seleccioneRolUsuario"));
            }

            if (esAlta && !EsNombreUsuarioValido(usuario.NombreUsuario))
            {
                throw new Exception(Traductor.T("error.formatoNombreUsuario"));
            }
        }

        private void ValidarQueNoSeaElUsuarioEnSesion(string nombreUsuario, string nombreUsuarioEnSesion)
        {
            if (EsElMismoUsuario(nombreUsuario, nombreUsuarioEnSesion))
            {
                throw new Exception(Traductor.T("error.bajaUsuarioPropio"));
            }
        }

        
        /// Evita que el sistema quede sin ningún usuario capaz de administrar
        /// usuarios. Sin esta validación un administrador podría dar de baja al
        /// último que queda y nadie podría volver a entrar a gestionarlos.
        /// Un usuario bloqueado no cuenta: no puede iniciar sesión.
        
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
                throw new Exception(Traductor.T("error.sinAdministrador"));
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

        // "campo" es la clave de traduccion del nombre del campo, por ejemplo "campo.elNombre"
        private void ExigirTexto(string valor, string campo, int largoMaximo)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                throw new Exception(Traductor.T("error.debeCompletar", Traductor.T(campo)));
            }

            if (valor.Trim().Length > largoMaximo)
            {
                throw new Exception(Traductor.T("error.valorLargo", Traductor.T(campo), largoMaximo));
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
