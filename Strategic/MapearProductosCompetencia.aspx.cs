using BE;
using BLL;
using Services;
using Strategic.Seguridad;
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Strategic
{
    // CU-007-036 - Mapear Productos con Competencia
    public partial class MapearProductosCompetencia : PaginaSegura
    {
        private readonly BLLCompetencia bllCompetencia = new BLLCompetencia();
        private readonly BLLProducto bllProducto = new BLLProducto();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarCombos();
            }
        }

        protected void btnMapear_Click(object sender, EventArgs e)
        {
            if (!IsValid)
            {
                return;
            }

            try
            {
                int idProducto = Convert.ToInt32(ddlProducto.SelectedValue);
                int idCompetencia = Convert.ToInt32(ddlCompetidor.SelectedValue);

                BEUsuario enSesion = SessionManager.UsuarioActual;

                bllCompetencia.MapearProducto(idProducto, idCompetencia, txtUrl.Text.Trim(), enSesion.NombreUsuario);

                lblExito.Text = "Se registró el mapeo. La publicación será monitoreada en el próximo ciclo automático.";
                lblError.Text = string.Empty;

                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                lblExito.Text = string.Empty;
                lblError.Text = Server.HtmlEncode(ex.Message);
            }
        }

        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            lblExito.Text = string.Empty;
            lblError.Text = string.Empty;

            LimpiarFormulario();
        }

        private void LimpiarFormulario()
        {
            ddlProducto.SelectedIndex = 0;
            ddlCompetidor.SelectedIndex = 0;
            txtUrl.Text = string.Empty;
        }

        /// <summary>
        /// Precondicion del CU: deben existir productos sincronizados (CU-006-002)
        /// y al menos un competidor activo (CU-007-033). Si falta alguno de los
        /// dos no tiene sentido mostrar el formulario.
        /// </summary>
        private void CargarCombos()
        {
            List<BEProducto> productos = bllProducto.TraerListaProductos();
            List<BECompetencia> competidores = bllCompetencia.FiltrarCompetidores(null, null, true);

            ddlProducto.Items.Clear();
            ddlProducto.Items.Add(new ListItem("Seleccioná un producto", string.Empty));

            foreach (BEProducto producto in productos)
            {
                ddlProducto.Items.Add(new ListItem(
                    string.Format("{0} - {1}", producto.Codigo, producto.Nombre),
                    producto.IdProducto.ToString()));
            }

            ddlCompetidor.Items.Clear();
            ddlCompetidor.Items.Add(new ListItem("Seleccioná un competidor", string.Empty));

            foreach (BECompetencia competidor in competidores)
            {
                ddlCompetidor.Items.Add(new ListItem(
                    string.Format("{0} ({1})", competidor.Nombre, competidor.Marketplace),
                    competidor.IdCompetencia.ToString()));
            }

            bool faltanDatos = productos.Count == 0 || competidores.Count == 0;

            pnlSinDatos.Visible = faltanDatos;
            pnlFormulario.Visible = !faltanDatos;

            if (faltanDatos)
            {
                lblSinDatos.Text = productos.Count == 0
                    ? "No existen productos sincronizados. Sincronizá el catálogo antes de mapear productos."
                    : "No existen competidores activos registrados. Registrá un competidor antes de mapear productos.";
            }
        }
    }
}
