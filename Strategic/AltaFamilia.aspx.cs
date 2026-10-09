using BE;
using BLL;
using Services;
using Strategic.Seguridad;
using System;
using System.Web.UI;

namespace Strategic
{
    // CU-005-026 - Alta Familia
    public partial class AltaFamilia : PaginaSegura
    {
        private readonly BLLPermiso bllPermiso = new BLLPermiso();

        public override void ActualizarIdioma()
        {
            Title = Traductor.T("altaFamilia.titulo");
            lblEstadoActual.Text = Traductor.T("altaFamilia.naceActiva");
            lblNombre.Text = Traductor.T("familia.nombre");
            valNombre.ErrorMessage = Traductor.T("familia.nombreRequerido");
            lblDescripcion.Text = Traductor.T("familia.descripcion");
            btnGuardar.Text = Traductor.T("altaFamilia.crear");
            btnCancelar.Text = Traductor.T("comun.cancelar");
            btnVolver.Text = Traductor.T("comun.volverAlListado");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
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

                bllPermiso.AltaFamilia(
                    txtNombre.Text,
                    txtDescripcion.Text,
                    selectorPermisos.ObtenerSeleccionados(),
                    enSesion.NombreUsuario);

                Session["MensajeFamilias"] = Traductor.T("altaFamilia.exito", txtNombre.Text.Trim());
                Response.Redirect("~/ConsultarFamilias.aspx");
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(ex.Message);
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/ConsultarFamilias.aspx");
        }
    }
}
