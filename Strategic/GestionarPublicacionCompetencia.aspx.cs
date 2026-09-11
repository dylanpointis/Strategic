using BE;
using BLL;
using Services;
using System;
using System.Web.UI;

namespace Strategic
{
    // CU-007-037, pasos 5 a 8: confirmacion de Pausar / Reactivar / Desactivar
    public partial class GestionarPublicacionCompetencia : Page
    {
        private readonly BLLCompetencia bllCompetencia = new BLLCompetencia();

        /// <summary>
        /// Publicacion sobre la que se esta pidiendo confirmacion. Viaja en el
        /// ViewState entre el clic y la confirmacion.
        /// </summary>
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

        /// <summary>
        /// "PausarReactivar" resuelve la accion real segun el estado actual de
        /// la fila, igual que hace la grilla para elegir el texto del boton.
        /// </summary>
        private void CargarPublicacion(string idTexto, string accionSolicitada)
        {
            int idProductoCompetencia;

            if (!int.TryParse(idTexto, out idProductoCompetencia) || idProductoCompetencia <= 0)
            {
                MostrarSinDatos("No se indicó qué publicación gestionar");
                return;
            }

            try
            {
                BEPublicacionCompetencia publicacion = bllCompetencia.TraerPublicacionMonitoreoPorId(idProductoCompetencia);

                if (publicacion == null)
                {
                    MostrarSinDatos("No se encontró la publicación indicada");
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

                lblConfirmacion.Text = Server.HtmlEncode(string.Format(TextoConfirmacion(accion), descripcion));
                btnConfirmar.Text = accion;
                btnConfirmar.CssClass = accion == "Desactivar" ? "btn btn-strategic-danger" : "btn btn-strategic";
            }
            catch (Exception ex)
            {
                MostrarSinDatos(ex.Message);
            }
        }

        /// <summary>
        /// Traduce el comando que llega de la grilla a la accion concreta.
        /// Devuelve null si la accion no tiene sentido contra el estado actual,
        /// para no ofrecer un boton que la BLL va a terminar rechazando.
        /// </summary>
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
                return "Esta publicación ya está desactivada";
            }

            return "No se pudo determinar la acción solicitada";
        }

        private string TextoConfirmacion(string accion)
        {
            switch (accion)
            {
                case "Pausar":
                    return "¿Deseás pausar el monitoreo de {0}?";
                case "Reactivar":
                    return "¿Deseás reactivar el monitoreo de {0}?";
                case "Desactivar":
                    return "¿Deseás desactivar el monitoreo de {0}? Esta acción no se puede deshacer.";
                default:
                    return "¿Deseás confirmar la acción sobre {0}?";
            }
        }

        private string MensajeExito(string accion)
        {
            switch (accion)
            {
                case "Pausar":
                    return "Se pausó el monitoreo de la publicación";
                case "Reactivar":
                    return "Se reactivó el monitoreo de la publicación";
                case "Desactivar":
                    return "Se desactivó la publicación";
                default:
                    return "Se actualizó el estado de la publicación";
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
