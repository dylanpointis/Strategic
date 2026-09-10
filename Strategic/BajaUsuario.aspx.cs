using BE;
using BLL;
using Services;
using System;
using System.Web.UI;

namespace Strategic
{
    // CU-005-019 - Baja (y reactivacion) de Usuario
    public partial class BajaUsuario : Page
    {
        private readonly BLLUsuario bllUsuario = new BLLUsuario();

        /// <summary>
        /// Usuario sobre el que se está pidiendo confirmación. Viaja en el
        /// ViewState entre el clic y la confirmación.
        /// </summary>
        private string UsuarioAConfirmar
        {
            get { return Convert.ToString(ViewState["UsuarioAConfirmar"]); }
            set { ViewState["UsuarioAConfirmar"] = value; }
        }

        private bool EstadoAConfirmar
        {
            get { return ViewState["EstadoAConfirmar"] != null && Convert.ToBoolean(ViewState["EstadoAConfirmar"]); }
            set { ViewState["EstadoAConfirmar"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Solo WebMaster y Administrador segun el CU
            BEUsuario usuario = SessionManager.UsuarioActual;

            if (usuario == null || (usuario.CodRol != 1 && usuario.CodRol != 2))
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                CargarUsuario(Request.QueryString["usuario"]);
            }
        }

        protected void btnConfirmar_Click(object sender, EventArgs e)
        {
            string nombreUsuario = UsuarioAConfirmar;
            bool activo = EstadoAConfirmar;

            if (string.IsNullOrEmpty(nombreUsuario))
            {
                return;
            }

            try
            {
                BEUsuario enSesion = SessionManager.UsuarioActual;

                bllUsuario.CambiarEstadoUsuario(nombreUsuario, activo, enSesion.NombreUsuario);

                Session["MensajeUsuarios"] = string.Format(
                    activo ? "Se reactivó el usuario {0}" : "Se dio de baja al usuario {0}",
                    nombreUsuario);

                Response.Redirect("~/ConsultarUsuarios.aspx");
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(ex.Message);
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/ConsultarUsuarios.aspx");
        }

        private void CargarUsuario(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                MostrarSinUsuario("No se indicó qué usuario dar de baja");
                return;
            }

            try
            {
                BEUsuario usuario = bllUsuario.TraerUsuarioPorNombre(nombreUsuario);

                if (usuario == null)
                {
                    MostrarSinUsuario("No se encontró el usuario indicado");
                    return;
                }

                UsuarioAConfirmar = usuario.NombreUsuario;
                EstadoAConfirmar = !usuario.Activo;

                lblConfirmacion.Text = Server.HtmlEncode(string.Format(
                    usuario.Activo
                        ? "¿Deseás dar de baja al usuario {0}? No va a poder iniciar sesión."
                        : "¿Deseás reactivar al usuario {0}?",
                    usuario.NombreUsuario));

                btnConfirmar.Text = usuario.Activo ? "Dar de baja" : "Reactivar";
                btnConfirmar.CssClass = usuario.Activo ? "btn btn-strategic-danger" : "btn btn-strategic";
            }
            catch (Exception ex)
            {
                MostrarSinUsuario(ex.Message);
            }
        }

        private void MostrarSinUsuario(string mensaje)
        {
            lblSinUsuario.Text = Server.HtmlEncode(mensaje);

            pnlConfirmacion.Visible = false;
            pnlSinUsuario.Visible = true;
        }
    }
}
