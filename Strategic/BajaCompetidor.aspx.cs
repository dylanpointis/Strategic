using BE;
using BLL;
using Services;
using System;
using System.Web.UI;

namespace Strategic
{
    // Baja (y reactivacion) de Competidor
    public partial class BajaCompetidor : Page
    {
        private readonly BLLCompetencia bllCompetencia = new BLLCompetencia();

        /// <summary>
        /// Competidor sobre el que se está pidiendo confirmación. Viaja en el
        /// ViewState entre el clic y la confirmación.
        /// </summary>
        private int CompetidorAConfirmar
        {
            get { return ViewState["CompetidorAConfirmar"] == null ? 0 : Convert.ToInt32(ViewState["CompetidorAConfirmar"]); }
            set { ViewState["CompetidorAConfirmar"] = value; }
        }

        private bool EstadoAConfirmar
        {
            get { return ViewState["EstadoAConfirmar"] != null && Convert.ToBoolean(ViewState["EstadoAConfirmar"]); }
            set { ViewState["EstadoAConfirmar"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Solo WebMaster y Administrador, igual que la gestion de usuarios
            BEUsuario usuario = SessionManager.UsuarioActual;

            if (usuario == null || (usuario.CodRol != 1 && usuario.CodRol != 2))
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                CargarCompetidor(Request.QueryString["id"]);
            }
        }

        protected void btnConfirmar_Click(object sender, EventArgs e)
        {
            int idCompetencia = CompetidorAConfirmar;
            bool activo = EstadoAConfirmar;

            if (idCompetencia <= 0)
            {
                return;
            }

            try
            {
                BEUsuario enSesion = SessionManager.UsuarioActual;

                bllCompetencia.CambiarEstadoCompetidor(idCompetencia, activo, enSesion.NombreUsuario);

                Session["MensajeCompetidores"] = string.Format(
                    activo ? "Se reactivó el competidor" : "Se dio de baja al competidor");

                Response.Redirect("~/ConsultarCompetidores.aspx");
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(ex.Message);
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/ConsultarCompetidores.aspx");
        }

        private void CargarCompetidor(string idTexto)
        {
            int idCompetencia;

            if (!int.TryParse(idTexto, out idCompetencia) || idCompetencia <= 0)
            {
                MostrarSinCompetidor("No se indicó qué competidor dar de baja");
                return;
            }

            try
            {
                BECompetencia competidor = bllCompetencia.TraerCompetidorPorId(idCompetencia);

                if (competidor == null)
                {
                    MostrarSinCompetidor("No se encontró el competidor indicado");
                    return;
                }

                CompetidorAConfirmar = competidor.IdCompetencia;
                EstadoAConfirmar = !competidor.Activo;

                lblConfirmacion.Text = Server.HtmlEncode(string.Format(
                    competidor.Activo
                        ? "¿Deseás dar de baja al competidor {0}?"
                        : "¿Deseás reactivar al competidor {0}?",
                    competidor.Nombre));

                btnConfirmar.Text = competidor.Activo ? "Dar de baja" : "Reactivar";
                btnConfirmar.CssClass = competidor.Activo ? "btn btn-strategic-danger" : "btn btn-strategic";
            }
            catch (Exception ex)
            {
                MostrarSinCompetidor(ex.Message);
            }
        }

        private void MostrarSinCompetidor(string mensaje)
        {
            lblSinCompetidor.Text = Server.HtmlEncode(mensaje);

            pnlConfirmacion.Visible = false;
            pnlSinCompetidor.Visible = true;
        }
    }
}
