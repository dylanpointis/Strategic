using BE;
using BLL;
using Services;
using Strategic.Seguridad;
using System;
using System.Web.UI;

namespace Strategic
{
    // CU-005-019 - Baja (y reactivacion) de Usuario
    public partial class BajaUsuario : PaginaSegura
    {
        private readonly BLLUsuario bllUsuario = new BLLUsuario();

        
        /// Usuario sobre el que se está pidiendo confirmación. Viaja en el
        /// ViewState entre el clic y la confirmación.
        
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

        public override void ActualizarIdioma()
        {
            Title = Traductor.T("bajaUsuario.titulo");
            btnCancelar.Text = Traductor.T("comun.cancelar");
            btnVolver.Text = Traductor.T("comun.volverAlListado");

            // El usuario a confirmar se conoce recien al recuperar el ViewState
            if (!string.IsNullOrEmpty(UsuarioAConfirmar))
            {
                MostrarConfirmacion();
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
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

                Session["MensajeUsuarios"] = Traductor.T(
                    activo ? "bajaUsuario.exitoReactivar" : "bajaUsuario.exitoBaja",
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
                MostrarSinUsuario(Traductor.T("bajaUsuario.sinUsuario"));
                return;
            }

            try
            {
                BEUsuario usuario = bllUsuario.TraerUsuarioPorNombre(nombreUsuario);

                if (usuario == null)
                {
                    MostrarSinUsuario(Traductor.T("usuario.noEncontrado"));
                    return;
                }

                UsuarioAConfirmar = usuario.NombreUsuario;
                EstadoAConfirmar = !usuario.Activo;

                MostrarConfirmacion();
            }
            catch (Exception ex)
            {
                MostrarSinUsuario(ex.Message);
            }
        }

        private void MostrarConfirmacion()
        {
            // EstadoAConfirmar es el estado nuevo: false significa dar de baja
            bool darDeBaja = !EstadoAConfirmar;

            lblConfirmacion.Text = Server.HtmlEncode(Traductor.T(
                darDeBaja ? "bajaUsuario.confirmarBaja" : "bajaUsuario.confirmarReactivar",
                UsuarioAConfirmar));

            btnConfirmar.Text = Traductor.T(darDeBaja ? "comun.accion.darDeBaja" : "comun.accion.reactivar");
            btnConfirmar.CssClass = darDeBaja ? "btn btn-strategic-danger" : "btn btn-strategic";
        }

        private void MostrarSinUsuario(string mensaje)
        {
            lblSinUsuario.Text = Server.HtmlEncode(mensaje);

            pnlConfirmacion.Visible = false;
            pnlSinUsuario.Visible = true;
        }
    }
}
