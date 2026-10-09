using BE;
using BLL;
using Services;
using Strategic.Seguridad;
using System;
using System.Web.UI;

namespace Strategic
{
    // CU-005-022 - Alta Rol
    public partial class AltaRol : PaginaSegura
    {
        private readonly BLLRol bllRol = new BLLRol();
        private readonly BLLPermiso bllPermiso = new BLLPermiso();

        public override void ActualizarIdioma()
        {
            Title = Traductor.T("altaRol.titulo");
            lblEstadoActual.Text = Traductor.T("altaRol.naceActivo");
            lblNombre.Text = Traductor.T("rol.nombre");
            valNombre.ErrorMessage = Traductor.T("rol.nombreRequerido");
            btnGuardar.Text = Traductor.T("altaRol.crear");
            btnCancelar.Text = Traductor.T("comun.cancelar");
            btnVolver.Text = Traductor.T("comun.volverAlListado");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Los componentes vienen con su subarbol para que el selector
                // pueda avisar cuando un permiso ya esta dentro de una familia
                selectorPermisos.Cargar(bllPermiso.TraerComponentesDisponiblesConArbol(), null);
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

                bllRol.AltaRol(txtNombre.Text, selectorPermisos.ObtenerSeleccionados(), enSesion.NombreUsuario);

                Session["MensajeRoles"] = Traductor.T("altaRol.exito", txtNombre.Text.Trim());
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
    }
}
