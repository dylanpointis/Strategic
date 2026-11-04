using BE;
using BLL;
using Services;
using Strategic.Seguridad;
using System;

namespace Strategic
{
    // CU-003-010 - Pausar o Reactivar Automatizacion
    public partial class PausarReactivarAutomatizacion : PaginaSegura
    {
        private readonly BLLAutomatizacion bllAutomatizacion = new BLLAutomatizacion();

        // Automatizacion a confirmar. Viaja en el ViewState entre el clic y la confirmacion
        private int AutomatizacionAConfirmar
        {
            get { return ViewState["AutomatizacionAConfirmar"] == null ? 0 : Convert.ToInt32(ViewState["AutomatizacionAConfirmar"]); }
            set { ViewState["AutomatizacionAConfirmar"] = value; }
        }

        public override void ActualizarIdioma()
        {
            Title = Traductor.T("pausarAutomatizacion.titulo");
            btnCancelar.Text = Traductor.T("comun.cancelar");
            btnVolver.Text = Traductor.T("comun.volverAlListado");

            // Lo que se esta confirmando se conoce recien al recuperar el ViewState
            if (AutomatizacionAConfirmar > 0)
            {
                CargarAutomatizacion(Request.QueryString["id"]);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarAutomatizacion(Request.QueryString["id"]);
            }
        }

        protected void btnConfirmar_Click(object sender, EventArgs e)
        {
            int idAutomatizacion = AutomatizacionAConfirmar;

            if (idAutomatizacion <= 0)
            {
                return;
            }

            try
            {
                bllAutomatizacion.CambiarEstado(idAutomatizacion, SessionManager.UsuarioActual.NombreUsuario);

                Session["MensajeAutomatizaciones"] = Traductor.T("pausarAutomatizacion.exito");

                Response.Redirect("~/ConsultarAutomatizaciones.aspx");
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(ex.Message);
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/ConsultarAutomatizaciones.aspx");
        }

        private void CargarAutomatizacion(string idTexto)
        {
            int idAutomatizacion;

            if (!int.TryParse(idTexto, out idAutomatizacion) || idAutomatizacion <= 0)
            {
                MostrarSinAutomatizacion(Traductor.T("automatizacion.noEncontrada"));
                return;
            }

            try
            {
                BEAutomatizacion automatizacion = bllAutomatizacion.TraerAutomatizacionPorId(idAutomatizacion);

                if (automatizacion == null)
                {
                    MostrarSinAutomatizacion(Traductor.T("automatizacion.noEncontrada"));
                    return;
                }

                AutomatizacionAConfirmar = automatizacion.IdAutomatizacion;

                lblConfirmacion.Text = Server.HtmlEncode(Traductor.T(
                    automatizacion.Activa ? "pausarAutomatizacion.confirmarPausar" : "pausarAutomatizacion.confirmarReactivar",
                    automatizacion.Nombre));

                btnConfirmar.Text = Traductor.T(automatizacion.AccionEstado);
            }
            catch (Exception ex)
            {
                MostrarSinAutomatizacion(ex.Message);
            }
        }

        private void MostrarSinAutomatizacion(string mensaje)
        {
            lblSinAutomatizacion.Text = Server.HtmlEncode(mensaje);

            pnlConfirmacion.Visible = false;
            pnlSinAutomatizacion.Visible = true;
        }
    }
}
