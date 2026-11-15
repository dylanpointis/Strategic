using BE;
using BLL;
using Services;
using Strategic.Componentes;
using System;

namespace Strategic
{
    // CU-001-003 - Cambiar Contrasena. No lleva permiso: es de todos los
    // roles, solo pide sesion iniciada
    public partial class CambiarClave : PaginaBase
    {
        private readonly BLLUsuario bllUsuario = new BLLUsuario();

        public override void ActualizarIdioma()
        {
            Title = Traductor.T("cambiarClave.titulo");
            btnGuardar.Text = Traductor.T("comun.guardar");
            btnCancelar.Text = Traductor.T("comun.cancelar");
        }

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
                    campoClaveActual.Clave,
                    campoClaveNueva.Clave,
                    campoClaveNueva.Confirmacion);

                // La sesion se queda con la clave y la sal nuevas
                SessionManager.UsuarioActual.Clave = usuario.Clave;
                SessionManager.UsuarioActual.Salt = usuario.Salt;

                lblExito.Text = Traductor.T("cambiarClave.exito");
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
