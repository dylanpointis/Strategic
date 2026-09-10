using BE;
using BLL;
using Services;
using System;
using System.Web.UI;

namespace Strategic
{
    // CU-005-022 - Alta Rol
    public partial class AltaRol : Page
    {
        private readonly BLLRol bllRol = new BLLRol();
        private readonly BLLPermiso bllPermiso = new BLLPermiso();

        protected void Page_Load(object sender, EventArgs e)
        {
            // El alta de roles es exclusiva del WebMaster segun el CU
            BEUsuario usuario = SessionManager.UsuarioActual;

            if (usuario == null || usuario.CodRol != 1)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                // Los componentes vienen con su subarbol para que el selector
                // pueda avisar cuando un permiso ya esta dentro de una familia
                selectorPermisos.Cargar(bllPermiso.TraerComponentesDisponiblesConArbol(), null);
                lblEstadoActual.Text = "Nace activo";
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

                bllRol.AltaRol(txtNombre.Text, selectorPermisos.ObtenerSeleccionados(), enSesion.NombreUsuario);

                Session["MensajeRoles"] = string.Format("Se creó el rol {0}", txtNombre.Text.Trim());
                Response.Redirect("~/ConsultarRoles.aspx");
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(ex.Message);
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/ConsultarRoles.aspx");
        }
    }
}
