using BE;
using BLL;
using Services;
using Strategic.Componentes;
using System;
using System.IO;

namespace Strategic.Seguridad
{
    
    /// Página base de todas las pantallas que exigen permiso.
    ///
    /// El permiso que habilita una pantalla lleva el mismo nombre que el aspx,
    /// así que la página no tiene que declarar nada: en el Init se toma el
    /// nombre del archivo y se le pregunta al rol del usuario en sesión si lo
    /// alcanza. El rol responde recorriendo su árbol de permisos (Composite),
    /// sin que acá haga falta saber si el permiso llega suelto o dentro de
    /// una familia.
    ///
    /// Sin sesión se redirige al login; con sesión pero sin permiso se
    /// registra el intento en la bitácora y se muestra Acceso denegado.
    
    public abstract class PaginaSegura : PaginaBase
    {
        private readonly BLLUsuario bllUsuario = new BLLUsuario();

        // Con inconsistencias de digito verificador el sistema esta fuera de
        // servicio: el WebMaster solo puede repararlo o restaurar un backup
        private static readonly string[] PaginasConSistemaInconsistente =
        {
            "RepararDigitoVerificador",
            "RealizarRestore"
        };

        public const string PaginaConSuscripcionVencida = "RenovarSuscripcion";

        
        /// Usuario logueado. Queda disponible desde el Page_Init en adelante.
        
        protected BEUsuario UsuarioEnSesion { get; private set; }

        
        /// Nombre del aspx sin extensión, que es el nombre del permiso.
        
        protected string NombrePagina
        {
            get { return Path.GetFileNameWithoutExtension(Request.AppRelativeCurrentExecutionFilePath); }
        }

        
        /// Permiso que habilita la página. Por defecto es el nombre del aspx;
        /// una página puede sobreescribirlo si se protege con otro.
        
        protected virtual string PermisoRequerido
        {
            get { return NombrePagina; }
        }

        
        /// Indica si el usuario en sesión alcanza un permiso. Las pantallas lo
        /// usan para ocultar botones que llevan a páginas que no podría abrir.
        
        protected bool TienePermiso(string nombrePermiso)
        {
            return UsuarioEnSesion != null
                && UsuarioEnSesion.Rol != null
                && UsuarioEnSesion.Rol.TienePermiso(nombrePermiso);
        }

        protected override void OnInit(EventArgs e)
        {
            UsuarioEnSesion = SessionManager.UsuarioActual;

            if (UsuarioEnSesion == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!TienePermiso(PermisoRequerido))
            {
                // Alguien llego a una pantalla que su rol no habilita, por lo
                // general escribiendo la direccion a mano. Queda en la bitacora.
                bllUsuario.RegistrarAccesoDenegado(UsuarioEnSesion.NombreUsuario, PermisoRequerido);

                Response.Redirect("~/AccesoDenegado.aspx?pantalla=" + Server.UrlEncode(PermisoRequerido));
                return;
            }

            if (SessionManager.SistemaInconsistente && Array.IndexOf(PaginasConSistemaInconsistente, NombrePagina) < 0)
            {
                Response.Redirect("~/RepararDigitoVerificador.aspx");
                return;
            }

            // Con la suscripcion vencida solo queda la pantalla de renovacion
            if (SessionManager.SuscripcionVencida && NombrePagina != PaginaConSuscripcionVencida)
            {
                Response.Redirect("~/RenovarSuscripcion.aspx");
                return;
            }

            // Recien ahora corre el Page_Init de la pagina concreta
            base.OnInit(e);
        }
    }
}
