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
        private readonly BLLDigitoVerificador bllDigitoVerificador = new BLLDigitoVerificador();

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
                // Intenta realizar el inicio de sesión y redirige al menú principal si es exitoso
                BEUsuario usuario = bllUsuario.Login(txtNombreUsuario.Text.Trim(), txtClave.Text);
                SessionManager.UsuarioActual = usuario;
                Response.Redirect("~/Default.aspx");
            }
            catch (InconsistenciaDVException ex)
            {
                // El digito verificador no coincide: el sistema queda fuera de
                // servicio. Solo entra quien puede repararlo, y va directo a esa pantalla

                if (ex.Usuario.Rol != null && ex.Usuario.Rol.TienePermiso("RepararDigitoVerificador"))
                {
                    SessionManager.UsuarioActual = ex.Usuario;
                    SessionManager.InconsistenciasDV = ex.Inconsistencias;
                    Response.Redirect("~/RepararDigitoVerificador.aspx");
                }
                else
                {
                    lblError.Text = "El sistema no está disponible por inconsistencias en los datos. Comunicate con el WebMaster.";
                }
            }
            catch (Exception ex)
            {
                lblError.Text = ex.Message;
            }
        }
    }
}
