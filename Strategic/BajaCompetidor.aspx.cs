using BE;
using BLL;
using Services;
using Strategic.Seguridad;
using System;
using System.Web.UI;

namespace Strategic
{
    // Baja (y reactivacion) de Competidor
    public partial class BajaCompetidor : PaginaSegura
    {
        private readonly BLLCompetencia bllCompetencia = new BLLCompetencia();

        
        /// Competidor sobre el que se está pidiendo confirmación. Viaja en el
        /// ViewState entre el clic y la confirmación.
        
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

        public override void ActualizarIdioma()
        {
            Title = Traductor.T("bajaCompetidor.titulo");
            btnCancelar.Text = Traductor.T("comun.cancelar");
            btnVolver.Text = Traductor.T("comun.volverAlListado");

            // Lo que se esta confirmando se conoce recien al recuperar el
            // ViewState: se vuelve a cargar para armar los textos en el idioma nuevo
            if (CompetidorAConfirmar > 0)
            {
                CargarCompetidor(Request.QueryString["id"]);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
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

                Session["MensajeCompetidores"] = Traductor.T(
                    activo ? "bajaCompetidor.exitoReactivar" : "bajaCompetidor.exitoBaja");

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
                MostrarSinCompetidor(Traductor.T("bajaCompetidor.sinCompetidor"));
                return;
            }

            try
            {
                BECompetencia competidor = bllCompetencia.TraerCompetidorPorId(idCompetencia);

                if (competidor == null)
                {
                    MostrarSinCompetidor(Traductor.T("competidor.noEncontrado"));
                    return;
                }

                CompetidorAConfirmar = competidor.IdCompetencia;
                EstadoAConfirmar = !competidor.Activo;

                lblConfirmacion.Text = Server.HtmlEncode(Traductor.T(
                    competidor.Activo ? "bajaCompetidor.confirmarBaja" : "bajaCompetidor.confirmarReactivar",
                    competidor.Nombre));

                btnConfirmar.Text = Traductor.T(competidor.AccionEstado);
                btnConfirmar.CssClass = competidor.Activo ? "btn btn-strategic-danger" : "btn btn-strategic";

                MostrarAdvertenciaPublicaciones(competidor);
            }
            catch (Exception ex)
            {
                MostrarSinCompetidor(ex.Message);
            }
        }

        
        /// CU-007-034, paso 2: si el competidor se está dando de baja y tiene
        /// publicaciones mapeadas activas, se advierte que también se pausarán.
        
        private void MostrarAdvertenciaPublicaciones(BECompetencia competidor)
        {
            if (!competidor.Activo)
            {
                lblAdvertenciaPublicaciones.Visible = false;
                return;
            }

            int publicacionesActivas = bllCompetencia.ContarPublicacionesActivasPorCompetidor(competidor.IdCompetencia);

            if (publicacionesActivas == 0)
            {
                lblAdvertenciaPublicaciones.Visible = false;
                return;
            }

            lblAdvertenciaPublicaciones.Text = Server.HtmlEncode(publicacionesActivas == 1
                ? Traductor.T("bajaCompetidor.advertenciaUna")
                : Traductor.T("bajaCompetidor.advertencia", publicacionesActivas));

            lblAdvertenciaPublicaciones.Visible = true;
        }

        private void MostrarSinCompetidor(string mensaje)
        {
            lblSinCompetidor.Text = Server.HtmlEncode(mensaje);

            pnlConfirmacion.Visible = false;
            pnlSinCompetidor.Visible = true;
        }
    }
}
