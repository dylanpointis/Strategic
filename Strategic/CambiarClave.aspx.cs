using BE;
using BLL;
using Services;
using System;
using System.Web.UI;

namespace Strategic
{
    // CU-001-003 - Cambiar Contrasena. No lleva permiso: es de todos los
    // perfiles, solo pide sesion iniciada
    public partial class CambiarClave : Page
    {
        private readonly BLLUsuario bllUsuario = new BLLUsuario();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SessionManager.HayUsuarioLogueado)
            {
                Response.Redirect("~/Login.aspx");
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
                BEUsuario usuario = bllUsuario.CambiarClave(
                    SessionManager.UsuarioActual.NombreUsuario,
                    txtClaveActual.Text,
                    txtClaveNueva.Text,
                    txtConfirmarClave.Text);

                // La sesion se queda con la clave y la sal nuevas
                SessionManager.UsuarioActual.Clave = usuario.Clave;
                SessionManager.UsuarioActual.Salt = usuario.Salt;

                lblExito.Text = "La clave se cambió correctamente.";
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(ex.Message);
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Default.aspx");
        }
    }
}
