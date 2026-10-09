using BE;
using BLL;
using Services;
using Strategic.Seguridad;
using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Strategic
{
    // CU-005-020 - Modificar Usuario
    public partial class ModificarUsuario : PaginaSegura
    {
        private readonly BLLUsuario bllUsuario = new BLLUsuario();
        private readonly BLLRol bllRol = new BLLRol();

        
        /// Usuario que se está editando. Se guarda en el ViewState y no se
        /// vuelve a leer de la query string en cada postback.
        
        private string UsuarioEditado
        {
            get { return Convert.ToString(ViewState["UsuarioEditado"]); }
            set { ViewState["UsuarioEditado"] = value; }
        }

        // Clave de traduccion del estado del usuario editado
        private string EstadoEditado
        {
            get { return Convert.ToString(ViewState["EstadoEditado"]); }
            set { ViewState["EstadoEditado"] = value; }
        }

        public override void ActualizarIdioma()
        {
            Title = Traductor.T("modificarUsuario.titulo");
            lblNombreUsuario.Text = Traductor.T("usuario.nombreUsuario");
            lblRol.Text = Traductor.T("usuario.rol");
            valRol.ErrorMessage = Traductor.T("usuario.rolRequerido");
            lblNombre.Text = Traductor.T("usuario.nombre");
            valNombre.ErrorMessage = Traductor.T("usuario.nombreRequerido");
            lblApellido.Text = Traductor.T("usuario.apellido");
            valApellido.ErrorMessage = Traductor.T("usuario.apellidoRequerido");
            lblEmail.Text = Traductor.T("usuario.email");
            valEmail.ErrorMessage = Traductor.T("usuario.emailRequerido");
            valFormatoEmail.ErrorMessage = Traductor.T("usuario.formatoEmail");
            chkBloqueado.Text = Traductor.T("modificarUsuario.bloqueado");
            btnGuardar.Text = Traductor.T("comun.guardarCambios");
            btnCancelar.Text = Traductor.T("comun.cancelar");
            btnVolver.Text = Traductor.T("comun.volverAlListado");

            // Lo que se arma al cargar el usuario solo existe una vez
            // recuperado el ViewState, por eso se pregunta antes de traducirlo
            if (ddlRol.Items.Count > 0)
            {
                ddlRol.Items[0].Text = Traductor.T("usuario.seleccioneRol");
            }

            if (!string.IsNullOrEmpty(EstadoEditado))
            {
                lblEstadoActual.Text = Server.HtmlEncode(Traductor.T(EstadoEditado));
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarRoles();
                CargarUsuario(Request.QueryString["usuario"]);
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
                BEUsuario usuario = new BEUsuario();

                usuario.NombreUsuario = UsuarioEditado;
                usuario.Nombre = txtNombre.Text.Trim();
                usuario.Apellido = txtApellido.Text.Trim();
                usuario.Email = txtEmail.Text.Trim();
                usuario.CodRol = ObtenerRolElegido();
                usuario.Bloqueado = chkBloqueado.Checked;

                BEUsuario enSesion = SessionManager.UsuarioActual;

                bllUsuario.ModificarUsuario(usuario, enSesion.NombreUsuario);

                // El listado es el que confirma el resultado de la operacion
                Session["MensajeUsuarios"] = Traductor.T("modificarUsuario.exito", usuario.NombreUsuario);
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
                MostrarSinUsuario(Traductor.T("modificarUsuario.sinUsuario"));
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

                UsuarioEditado = usuario.NombreUsuario;
                EstadoEditado = usuario.EstadoTexto;

                lblUsuarioEditado.Text = Server.HtmlEncode(usuario.NombreUsuario);
                lblEstadoActual.Text = Server.HtmlEncode(Traductor.T(usuario.EstadoTexto));
                txtNombre.Text = usuario.Nombre;
                txtApellido.Text = usuario.Apellido;
                txtEmail.Text = usuario.Email;
                chkBloqueado.Checked = usuario.Bloqueado;

                ListItem rol = ddlRol.Items.FindByValue(usuario.CodRol.ToString());

                if (rol != null)
                {
                    ddlRol.ClearSelection();
                    rol.Selected = true;
                }
            }
            catch (Exception ex)
            {
                MostrarSinUsuario(ex.Message);
            }
        }

        private void MostrarSinUsuario(string mensaje)
        {
            lblSinUsuario.Text = Server.HtmlEncode(mensaje);

            pnlFormulario.Visible = false;
            pnlSinUsuario.Visible = true;
        }

        private void CargarRoles()
        {
            ddlRol.Items.Clear();
            ddlRol.Items.Add(new ListItem(Traductor.T("usuario.seleccioneRol"), string.Empty));

            foreach (BERol rol in bllRol.TraerListaRoles())
            {
                ddlRol.Items.Add(new ListItem(rol.Nombre, rol.CodRol.ToString()));
            }
        }

        private int ObtenerRolElegido()
        {
            int codRol;

            return int.TryParse(ddlRol.SelectedValue, out codRol) ? codRol : 0;
        }
    }
}
