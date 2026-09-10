using BE;
using BLL;
using Services;
using System;
using System.Web.UI;

namespace Strategic
{
    // CU-005-026 - Alta Familia
    public partial class AltaFamilia : Page
    {
        private readonly BLLPermiso bllPermiso = new BLLPermiso();

        protected void Page_Load(object sender, EventArgs e)
        {
            // El alta de familias es exclusiva del WebMaster segun el CU
            BEUsuario usuario = SessionManager.UsuarioActual;

            if (usuario == null || usuario.CodRol != 1)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                selectorPermisos.Cargar(bllPermiso.TraerComponentesDisponiblesConArbol(), null);
                lblEstadoActual.Text = "Nace activa";
            }
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!IsValid)
            {
                return;
            }

            try
            {
                BEUsuario enSesion = SessionManager.UsuarioActual;

                bllPermiso.AltaFamilia(
                    txtNombre.Text,
                    txtDescripcion.Text,
                    selectorPermisos.ObtenerSeleccionados(),
                    enSesion.NombreUsuario);

                Session["MensajeFamilias"] = string.Format("Se creó la familia {0}", txtNombre.Text.Trim());
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
    }
}
