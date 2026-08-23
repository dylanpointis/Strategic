using BE;
using BLL;
using Services;
using System;
using System.Web.UI;

namespace Strategic
{
    public partial class Login : Page
    {
        private readonly BLLUsuario bllUsuario = new BLLUsuario();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack && SessionManager.HayUsuarioLogueado)
            {
                Response.Redirect("~/Default.aspx");
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            if (!IsValid)
            {
                return;
            }

            try
            {
                BEUsuario usuario = bllUsuario.Login(txtNombreUsuario.Text.Trim(), txtClave.Text);
                SessionManager.UsuarioActual = usuario;
                Response.Redirect("~/Default.aspx");
            }
            catch (Exception ex)
            {
                lblError.Text = ex.Message;
            }
        }
    }
}
