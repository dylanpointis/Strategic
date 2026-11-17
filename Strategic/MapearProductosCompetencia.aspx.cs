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

        public override void ActualizarIdioma()
        {
            Title = Traductor.T("mapearProductos.titulo");
            lblUrl.Text = Traductor.T("mapearProductos.url");
            valUrl.ErrorMessage = Traductor.T("mapearProductos.urlRequerida");
            valFormatoUrl.ErrorMessage = Traductor.T("error.urlInvalida");
            btnMapear.Text = Traductor.T("mapearProductos.mapear");
            btnLimpiar.Text = Traductor.T("comun.limpiar");
        }

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
                // Los dos selectores son obligatorios: con la pagina valida tienen valor
                int idProducto = selectorProducto.IdProducto.Value;
                int idCompetencia = selectorCompetidor.IdCompetencia.Value;

                BEUsuario enSesion = SessionManager.UsuarioActual;

                bllCompetencia.MapearProducto(idProducto, idCompetencia, txtUrl.Text.Trim(), enSesion.NombreUsuario);

                lblExito.Text = Traductor.T("mapearProductos.exito");
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
            selectorProducto.Limpiar();
            selectorCompetidor.Limpiar();
            txtUrl.Text = string.Empty;
        }

        
        /// Precondicion del CU: deben existir productos sincronizados (CU-006-002)
        /// y al menos un competidor activo (CU-007-033). Si falta alguno de los
        /// dos no tiene sentido mostrar el formulario.
        
        private void CargarCombos()
        {
            List<BEProducto> productos = bllProducto.TraerListaProductos();
            List<BECompetencia> competidores = bllCompetencia.FiltrarCompetidores(null, null, true);

            selectorProducto.Cargar(productos);
            selectorCompetidor.Cargar(competidores);

            bool faltanDatos = productos.Count == 0 || competidores.Count == 0;

            pnlSinDatos.Visible = faltanDatos;
            pnlFormulario.Visible = !faltanDatos;

            if (faltanDatos)
            {
                lblSinDatos.Text = productos.Count == 0
                    ? Traductor.T("mapearProductos.sinProductos")
                    : Traductor.T("mapearProductos.sinCompetidores");
            }
        }
    }
}
