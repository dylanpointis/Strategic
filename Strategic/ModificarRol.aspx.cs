using BE;
using BLL;
using Services;
using Strategic.Seguridad;
using System;
using System.Web.UI;

namespace Strategic
{
    // CU-005-024 - Modificar Rol
    public partial class ModificarRol : PaginaSegura
    {
        private readonly BLLRol bllRol = new BLLRol();
        private readonly BLLPermiso bllPermiso = new BLLPermiso();

        private int RolEditado
        {
            get { return ViewState["RolEditado"] == null ? 0 : Convert.ToInt32(ViewState["RolEditado"]); }
            set { ViewState["RolEditado"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarRol(Request.QueryString["rol"]);
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
                BEUsuario enSesion = SessionManager.UsuarioActual;

                bllRol.ModificarRol(RolEditado, txtNombre.Text, selectorPermisos.ObtenerSeleccionados(), enSesion.NombreUsuario);

                Session["MensajeRoles"] = string.Format("Se actualizó el rol {0}", txtNombre.Text.Trim());
                Response.Redirect("~/ConsultarRoles.aspx");
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(ex.Message);
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/ConsultarRoles.aspx");
        }

        private void CargarRol(string valor)
        {
            int codRol;

            if (!int.TryParse(valor, out codRol))
            {
                MostrarSinRegistro("No se indicó qué rol modificar");
                return;
            }

            try
            {
                BERol rol = bllRol.TraerRolConComponentes(codRol);

                if (rol == null)
                {
                    MostrarSinRegistro("No se encontró el rol indicado");
                    return;
                }

                RolEditado = rol.CodRol;

                txtNombre.Text = rol.Nombre;
                lblEstadoActual.Text = Server.HtmlEncode(rol.EstadoTexto);

                // El selector arranca con los componentes que el rol ya tiene
                selectorPermisos.Cargar(
                    bllPermiso.TraerComponentesDisponiblesConArbol(),
                    rol.Componentes.ObtenerHijos());
            }
            catch (Exception ex)
            {
                MostrarSinRegistro(ex.Message);
            }
        }

        private void MostrarSinRegistro(string mensaje)
        {
            lblSinRegistro.Text = Server.HtmlEncode(mensaje);

            pnlFormulario.Visible = false;
            pnlSinRegistro.Visible = true;
        }
    }
}
