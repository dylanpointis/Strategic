using BE;
using BLL;
using Services;
using Strategic.Componentes;
using System;

namespace Strategic
{
    public partial class Login : PaginaBase
    {
        private readonly BLLUsuario bllUsuario = new BLLUsuario();
        private readonly BLLDigitoVerificador bllDigitoVerificador = new BLLDigitoVerificador();
        private readonly BLLSuscripcion bllSuscripcion = new BLLSuscripcion();

        public override void ActualizarIdioma()
        {
            Title = Traductor.T("login.titulo");
            lblNombreUsuario.Text = Traductor.T("login.usuario");
            rfvNombreUsuario.ErrorMessage = Traductor.T("login.usuarioRequerido");
            lblClave.Text = Traductor.T("login.clave");
            rfvClave.ErrorMessage = Traductor.T("login.claveRequerida");
            btnLogin.Text = Traductor.T("login.ingresar");
        }

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

                // Con la suscripcion vencida solo se puede renovar. El personal de
                // Strategic entra igual: tiene que poder ver la facturacion del cliente
                if (!bllSuscripcion.TieneServicioActivo() && !TienePermiso(usuario, "ReporteFacturacion"))
                {
                    if (!TienePermiso(usuario, "RenovarSuscripcion"))
                    {
                        lblError.Text = Traductor.T("login.suscripcionVencida");
                        return;
                    }

                    SessionManager.UsuarioActual = usuario;
                    SessionManager.SuscripcionVencida = true;
                    Response.Redirect("~/RenovarSuscripcion.aspx");
                    return;
                }

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
                    lblError.Text = Traductor.T("login.sistemaInconsistente");
                }
            }
            catch (Exception ex)
            {
                lblError.Text = ex.Message;
            }
        }

        private bool TienePermiso(BEUsuario usuario, string permiso)
        {
            return usuario.Rol != null && usuario.Rol.TienePermiso(permiso);
        }
    }
}
