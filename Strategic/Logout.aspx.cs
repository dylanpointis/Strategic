using BE;
using BLL;
using Services;
using System;
using System.Web.UI;

namespace Strategic
{
    public partial class Logout : Page
    {
        private readonly BLLUsuario bllUsuario = new BLLUsuario();

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnConfirmarLogout_Click(object sender, EventArgs e)
        {
            // El evento se registra antes de limpiar la sesion, que es de donde
            // sale el nombre del usuario que la esta cerrando
            BEUsuario usuario = SessionManager.UsuarioActual;

            if (usuario != null)
            {
                bllUsuario.RegistrarCierreSesion(usuario.NombreUsuario);
            }

            SessionManager.CerrarSesion();
            Response.Redirect("~/Login.aspx");
        }

        protected void btnCancelarLogout_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Default.aspx");
        }
    }
}
