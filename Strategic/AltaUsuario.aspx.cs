using BE;
using BLL;
using Services;
using Strategic.Seguridad;
using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Strategic
{
    // CU-005-018 - Alta Usuario
    public partial class AltaUsuario : PaginaSegura
    {
        private readonly BLLUsuario bllUsuario = new BLLUsuario();
        private readonly BLLRol bllRol = new BLLRol();

        public override void ActualizarIdioma()
        {
            Title = Traductor.T("altaUsuario.titulo");
            lblNombreUsuario.Text = Traductor.T("usuario.nombreUsuario");
            valNombreUsuario.ErrorMessage = Traductor.T("usuario.nombreUsuarioRequerido");
            valFormatoUsuario.ErrorMessage = Traductor.T("usuario.formatoUsuario");
            lblRol.Text = Traductor.T("usuario.rol");
            valRol.ErrorMessage = Traductor.T("usuario.rolRequerido");
            lblNombre.Text = Traductor.T("usuario.nombre");
            valNombre.ErrorMessage = Traductor.T("usuario.nombreRequerido");
            lblApellido.Text = Traductor.T("usuario.apellido");
            valApellido.ErrorMessage = Traductor.T("usuario.apellidoRequerido");
            lblEmail.Text = Traductor.T("usuario.email");
            valEmail.ErrorMessage = Traductor.T("usuario.emailRequerido");
            valFormatoEmail.ErrorMessage = Traductor.T("usuario.formatoEmail");
            btnCrear.Text = Traductor.T("altaUsuario.crear");
            btnCancelar.Text = Traductor.T("comun.cancelar");
            btnOtro.Text = Traductor.T("altaUsuario.crearOtro");
            btnVolver.Text = Traductor.T("comun.volverAlListado");

            // La primera opcion del combo se agrega desde codigo: en el Init
            // de un postback todavia no se recupero del ViewState
            if (ddlRol.Items.Count > 0)
            {
                ddlRol.Items[0].Text = Traductor.T("usuario.seleccioneRol");
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarRoles();
            }
        }

        protected void btnCrear_Click(object sender, EventArgs e)
        {
            if (!IsValid)
            {
                return;
            }

            try
            {
                BEUsuario nuevo = new BEUsuario();

                nuevo.NombreUsuario = txtNombreUsuario.Text.Trim();
                nuevo.Nombre = txtNombre.Text.Trim();
                nuevo.Apellido = txtApellido.Text.Trim();
                nuevo.Email = txtEmail.Text.Trim();
                nuevo.CodRol = ObtenerRolElegido();

                BEUsuario enSesion = SessionManager.UsuarioActual;

                // El alta devuelve la clave por defecto sin encriptar para
                // poder mostrarsela al administrador una unica vez
                string clave = bllUsuario.AltaUsuario(nuevo, enSesion.NombreUsuario);

                MostrarCreado(nuevo, clave);
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

        protected void btnOtro_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();

            pnlCreado.Visible = false;
            pnlFormulario.Visible = true;
        }

        protected void btnVolver_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/ConsultarUsuarios.aspx");
        }

        private void MostrarCreado(BEUsuario usuario, string clave)
        {
            lblCreadoUsuario.Text = Server.HtmlEncode(usuario.NombreUsuario);
            lblCreadoNombre.Text = Server.HtmlEncode(string.Format("{0} {1}", usuario.Nombre, usuario.Apellido));
            lblCreadoRol.Text = Server.HtmlEncode(ObtenerTextoRol());
            lblCreadoClave.Text = Server.HtmlEncode(clave);

            pnlFormulario.Visible = false;
            pnlCreado.Visible = true;
        }

        private void LimpiarFormulario()
        {
            txtNombreUsuario.Text = string.Empty;
            txtNombre.Text = string.Empty;
            txtApellido.Text = string.Empty;
            txtEmail.Text = string.Empty;
            ddlRol.SelectedIndex = 0;
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

        private string ObtenerTextoRol()
        {
            return ddlRol.SelectedItem != null ? ddlRol.SelectedItem.Text : string.Empty;
        }
    }
}
