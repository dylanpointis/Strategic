using BE;
using BLL;
using Services;
using Strategic.Componentes;
using System;

namespace Strategic
{
    public partial class Logout : PaginaBase
    {
        private readonly BLLUsuario bllUsuario = new BLLUsuario();

        public override void ActualizarIdioma()
        {
            Title = Traductor.T("logout.titulo");
            btnConfirmarLogout.Text = Traductor.T("logout.confirmar");
            btnCancelarLogout.Text = Traductor.T("comun.cancelar");
        }

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
