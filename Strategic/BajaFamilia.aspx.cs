using BE;
using BE.Composite;
using BLL;
using Services;
using Strategic.Seguridad;
using System;
using System.Web.UI;

namespace Strategic
{
    // CU-005-027 - Baja (y reactivacion) de Familia
    public partial class BajaFamilia : PaginaSegura
    {
        private readonly BLLPermiso bllPermiso = new BLLPermiso();

        
        /// Familia sobre la que se está pidiendo confirmación. Viaja en el
        /// ViewState entre el clic y la confirmación.
        
        private int FamiliaAConfirmar
        {
            get { return ViewState["FamiliaAConfirmar"] == null ? 0 : Convert.ToInt32(ViewState["FamiliaAConfirmar"]); }
            set { ViewState["FamiliaAConfirmar"] = value; }
        }

        private bool EstadoAConfirmar
        {
            get { return ViewState["EstadoAConfirmar"] != null && Convert.ToBoolean(ViewState["EstadoAConfirmar"]); }
            set { ViewState["EstadoAConfirmar"] = value; }
        }

        public override void ActualizarIdioma()
        {
            Title = Traductor.T("bajaFamilia.titulo");
            btnCancelar.Text = Traductor.T("comun.cancelar");
            btnVolver.Text = Traductor.T("comun.volverAlListado");

            // Lo que se esta confirmando se conoce recien al recuperar el
            // ViewState: se vuelve a cargar para armar los textos en el idioma nuevo
            if (FamiliaAConfirmar > 0)
            {
                CargarFamilia(Request.QueryString["familia"]);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarFamilia(Request.QueryString["familia"]);
            }
        }

        protected void btnConfirmar_Click(object sender, EventArgs e)
        {
            int codPermiso = FamiliaAConfirmar;
            bool activo = EstadoAConfirmar;

            if (codPermiso <= 0)
            {
                return;
            }

            try
            {
                BEUsuario enSesion = SessionManager.UsuarioActual;
                BEFamilia familia = bllPermiso.TraerFamiliaConHijos(codPermiso);

                bllPermiso.CambiarEstadoFamilia(codPermiso, activo, enSesion.NombreUsuario);

                Session["MensajeFamilias"] = Traductor.T(
                    activo ? "bajaFamilia.exitoReactivar" : "bajaFamilia.exitoBaja",
                    familia != null ? familia.Nombre : codPermiso.ToString());

                Response.Redirect("~/ConsultarFamilias.aspx");
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(ex.Message);
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/ConsultarFamilias.aspx");
        }

        private void CargarFamilia(string valor)
        {
            int codPermiso;

            if (!int.TryParse(valor, out codPermiso))
            {
                MostrarSinRegistro(Traductor.T("bajaFamilia.sinFamilia"));
                return;
            }

            try
            {
                BEFamilia familia = bllPermiso.TraerFamiliaConHijos(codPermiso);

                if (familia == null)
                {
                    MostrarSinRegistro(Traductor.T("familia.noEncontrada"));
                    return;
                }

                FamiliaAConfirmar = familia.CodPermiso;
                EstadoAConfirmar = !familia.Activo;

                lblConfirmacion.Text = Server.HtmlEncode(Traductor.T(
                    familia.Activo ? "bajaFamilia.confirmarBaja" : "bajaFamilia.confirmarReactivar",
                    familia.Nombre));

                btnConfirmar.Text = Traductor.T(familia.AccionEstado);
                btnConfirmar.CssClass = familia.Activo ? "btn btn-strategic-danger" : "btn btn-strategic";
            }
            catch (Exception ex)
            {
                MostrarSinRegistro(ex.Message);
            }
        }

        private void MostrarSinRegistro(string mensaje)
        {
            lblSinRegistro.Text = Server.HtmlEncode(mensaje);

            pnlConfirmacion.Visible = false;
            pnlSinRegistro.Visible = true;
        }
    }
}
