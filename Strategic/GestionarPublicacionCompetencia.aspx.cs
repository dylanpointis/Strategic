using BE;
using BLL;
using Services;
using Strategic.Componentes;
using System;

namespace Strategic
{
    // CU-007-037, pasos 5 a 8: confirmacion de Pausar / Reactivar / Desactivar
    public partial class GestionarPublicacionCompetencia : PaginaBase
    {
        private readonly BLLCompetencia bllCompetencia = new BLLCompetencia();

        
        /// Publicacion sobre la que se esta pidiendo confirmacion. Viaja en el
        /// ViewState entre el clic y la confirmacion.
        
        private int PublicacionAConfirmar
        {
            get { return ViewState["PublicacionAConfirmar"] == null ? 0 : Convert.ToInt32(ViewState["PublicacionAConfirmar"]); }
            set { ViewState["PublicacionAConfirmar"] = value; }
        }

        private string AccionAConfirmar
        {
            get { return Convert.ToString(ViewState["AccionAConfirmar"]); }
            set { ViewState["AccionAConfirmar"] = value; }
        }

        public override void ActualizarIdioma()
        {
            Title = Traductor.T("gestionarPublicacion.titulo");
            btnCancelar.Text = Traductor.T("comun.cancelar");
            btnVolver.Text = Traductor.T("comun.volverAlListado");

            // Lo que se esta confirmando se conoce recien al recuperar el
            // ViewState: se vuelve a cargar para armar los textos en el idioma nuevo
            if (PublicacionAConfirmar > 0)
            {
                CargarPublicacion(Request.QueryString["id"], Request.QueryString["accion"]);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Solo WebMaster y Administrador, igual que el resto de la gestion de competencia
            BEUsuario usuario = SessionManager.UsuarioActual;

            if (usuario == null || (usuario.CodRol != 1 && usuario.CodRol != 2))
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                CargarPublicacion(Request.QueryString["id"], Request.QueryString["accion"]);
            }
        }

        protected void btnConfirmar_Click(object sender, EventArgs e)
        {
            int idProductoCompetencia = PublicacionAConfirmar;
            string accion = AccionAConfirmar;

            if (idProductoCompetencia <= 0 || string.IsNullOrEmpty(accion))
            {
                return;
            }

            try
            {
                BEUsuario enSesion = SessionManager.UsuarioActual;

                bllCompetencia.CambiarEstadoPublicacion(idProductoCompetencia, accion, enSesion.NombreUsuario);

                Session["MensajePublicaciones"] = MensajeExito(accion);
                Response.Redirect("~/MonitoreoPublicacionesCompetidoras.aspx");
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(ex.Message);
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/MonitoreoPublicacionesCompetidoras.aspx");
        }

        
        /// "PausarReactivar" resuelve la accion real segun el estado actual de
        /// la fila, igual que hace la grilla para elegir el texto del boton.
        
        private void CargarPublicacion(string idTexto, string accionSolicitada)
        {
            int idProductoCompetencia;

            if (!int.TryParse(idTexto, out idProductoCompetencia) || idProductoCompetencia <= 0)
            {
                MostrarSinDatos(Traductor.T("gestionarPublicacion.sinPublicacion"));
                return;
            }

            try
            {
                BEPublicacionCompetencia publicacion = bllCompetencia.TraerPublicacionMonitoreoPorId(idProductoCompetencia);

                if (publicacion == null)
                {
                    MostrarSinDatos(Traductor.T("gestionarPublicacion.noEncontrada"));
                    return;
                }

                string accion = ResolverAccion(accionSolicitada, publicacion.Estado);

                if (accion == null)
                {
                    MostrarSinDatos(MensajeAccionInvalida(accionSolicitada, publicacion.Estado));
                    return;
                }

                PublicacionAConfirmar = publicacion.IdProductoCompetencia;
                AccionAConfirmar = accion;

                string descripcion = string.Format("{0} ({1})", publicacion.NombreProducto, publicacion.NombreCompetidor);

                lblConfirmacion.Text = Server.HtmlEncode(Traductor.T(TextoConfirmacion(accion), descripcion));
                btnConfirmar.Text = Traductor.T(TextoBoton(accion));
                btnConfirmar.CssClass = accion == "Desactivar" ? "btn btn-strategic-danger" : "btn btn-strategic";
            }
            catch (Exception ex)
            {
                MostrarSinDatos(ex.Message);
            }
        }

        
        /// Traduce el comando que llega de la grilla a la accion concreta.
        /// Devuelve null si la accion no tiene sentido contra el estado actual,
        /// para no ofrecer un boton que la BLL va a terminar rechazando.
        
        private string ResolverAccion(string accionSolicitada, string estadoActual)
        {
            if (accionSolicitada == "Desactivar")
            {
                return estadoActual == "Finalizada" ? null : "Desactivar";
            }

            if (accionSolicitada == "PausarReactivar")
            {
                if (estadoActual == "Activa")
                {
                    return "Pausar";
                }

                return estadoActual == "Finalizada" ? null : "Reactivar";
            }

            return null;
        }

        private string MensajeAccionInvalida(string accionSolicitada, string estadoActual)
        {
            if (estadoActual == "Finalizada")
            {
                return Traductor.T("gestionarPublicacion.yaDesactivada");
            }

            return Traductor.T("gestionarPublicacion.accionDesconocida");
        }

        // Claves de traduccion de la pregunta y del boton segun la accion
        private string TextoConfirmacion(string accion)
        {
            switch (accion)
            {
                case "Pausar":
                    return "gestionarPublicacion.confirmarPausar";
                case "Reactivar":
                    return "gestionarPublicacion.confirmarReactivar";
                case "Desactivar":
                    return "gestionarPublicacion.confirmarDesactivar";
                default:
                    return "gestionarPublicacion.confirmarAccion";
            }
        }

        private string TextoBoton(string accion)
        {
            switch (accion)
            {
                case "Pausar":
                    return "comun.accion.pausar";
                case "Reactivar":
                    return "comun.accion.reactivar";
                default:
                    return "monitoreo.desactivar";
            }
        }

        private string MensajeExito(string accion)
        {
            switch (accion)
            {
                case "Pausar":
                    return Traductor.T("gestionarPublicacion.exitoPausar");
                case "Reactivar":
                    return Traductor.T("gestionarPublicacion.exitoReactivar");
                case "Desactivar":
                    return Traductor.T("gestionarPublicacion.exitoDesactivar");
                default:
                    return Traductor.T("gestionarPublicacion.exito");
            }
        }

        private void MostrarSinDatos(string mensaje)
        {
            lblSinDatos.Text = Server.HtmlEncode(mensaje);

            pnlConfirmacion.Visible = false;
            pnlSinDatos.Visible = true;
        }
    }
}
