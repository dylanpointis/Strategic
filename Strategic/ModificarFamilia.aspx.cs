using BE;
using BE.Composite;
using BLL;
using Services;
using Strategic.Seguridad;
using System;
using System.Web.UI;

namespace Strategic
{
    // CU-005-028 - Modificar Familia
    public partial class ModificarFamilia : PaginaSegura
    {
        private readonly BLLPermiso bllPermiso = new BLLPermiso();

        private int FamiliaEditada
        {
            get { return ViewState["FamiliaEditada"] == null ? 0 : Convert.ToInt32(ViewState["FamiliaEditada"]); }
            set { ViewState["FamiliaEditada"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarFamilia(Request.QueryString["familia"]);
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

                bllPermiso.ModificarFamilia(
                    FamiliaEditada,
                    txtNombre.Text,
                    txtDescripcion.Text,
                    selectorPermisos.ObtenerSeleccionados(),
                    enSesion.NombreUsuario);

                Session["MensajeFamilias"] = string.Format("Se actualizó la familia {0}", txtNombre.Text.Trim());
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

        private void CargarFamilia(string valor)
        {
            int codPermiso;

            if (!int.TryParse(valor, out codPermiso))
            {
                MostrarSinRegistro("No se indicó qué familia modificar");
                return;
            }

            try
            {
                BEFamilia familia = bllPermiso.TraerFamiliaConHijos(codPermiso);

                if (familia == null)
                {
                    MostrarSinRegistro("No se encontró la familia indicada");
                    return;
                }

                FamiliaEditada = familia.CodPermiso;

                txtNombre.Text = familia.Nombre;
                txtDescripcion.Text = familia.Descripcion;
                lblEstadoActual.Text = Server.HtmlEncode(familia.EstadoTexto);

                selectorPermisos.Cargar(
                    bllPermiso.TraerComponentesDisponiblesConArbol(),
                    familia.ObtenerHijos());

                // Una familia no puede contenerse a si misma: se saca de la
                // lista de disponibles. Los ciclos indirectos los rechaza la BLL
                selectorPermisos.ExcluirDeDisponibles(familia.CodPermiso);
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
