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

        /// <summary>
        /// Familia sobre la que se está pidiendo confirmación. Viaja en el
        /// ViewState entre el clic y la confirmación.
        /// </summary>
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

                Session["MensajeFamilias"] = string.Format(
                    activo ? "Se reactivó la familia {0}" : "Se dio de baja la familia {0}",
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
                MostrarSinRegistro("No se indicó qué familia dar de baja");
                return;
            }

            try
            {
                BEFamilia familia = bllPermiso.TraerFamiliaConHijos(codPermiso);

                if (familia == null)
                {
                    MostrarSinRegistro("No se encontró la familia indicada");
                    return;
                }

                FamiliaAConfirmar = familia.CodPermiso;
                EstadoAConfirmar = !familia.Activo;

                lblConfirmacion.Text = Server.HtmlEncode(string.Format(
                    familia.Activo
                        ? "¿Deseás dar de baja la familia {0}? No va a poder asignarse a nuevos roles ni familias."
                        : "¿Deseás reactivar la familia {0}?",
                    familia.Nombre));

                btnConfirmar.Text = familia.Activo ? "Dar de baja" : "Reactivar";
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
