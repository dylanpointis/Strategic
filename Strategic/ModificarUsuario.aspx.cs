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

        /// <summary>
        /// Usuario que se está editando. Se guarda en el ViewState y no se
        /// vuelve a leer de la query string en cada postback.
        /// </summary>
        private string UsuarioEditado
        {
            get { return Convert.ToString(ViewState["UsuarioEditado"]); }
            set { ViewState["UsuarioEditado"] = value; }
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
                Session["MensajeUsuarios"] = string.Format("Se actualizaron los datos del usuario {0}", usuario.NombreUsuario);
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
                MostrarSinUsuario("No se indicó qué usuario modificar");
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

                UsuarioEditado = usuario.NombreUsuario;

                lblUsuarioEditado.Text = Server.HtmlEncode(usuario.NombreUsuario);
                lblEstadoActual.Text = Server.HtmlEncode(usuario.EstadoTexto);
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
            ddlRol.Items.Add(new ListItem("Seleccione un rol", string.Empty));

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
