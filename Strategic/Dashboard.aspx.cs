using BE;
using BLL;
using Services;
using Strategic.Componentes;
using Strategic.Seguridad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Strategic
{
    // CU-002-004 - Visualizar Dashboard Principal
    public partial class Dashboard : PaginaSegura
    {
        private const string MensajeSinVentas = "dashboard.sinVentas";
        private const string MensajeSinBajoStock = "dashboard.sinBajoStock";
        private const string MensajeSinFiltros = "comun.sinResultadosFiltros";

        private readonly BLLDashboard bllDashboard = new BLLDashboard();
        private readonly BLLProducto bllProducto = new BLLProducto();

        
        /// JSON de los gráficos. Viajan en el ViewState para sobrevivir a los
        /// postbacks del paginado de la grilla de bajo stock.
        
        protected string DatosCategoriaJson
        {
            get { return LeerJson("DatosCategoriaJson"); }
            private set { ViewState["DatosCategoriaJson"] = value; }
        }

        protected string DatosTopJson
        {
            get { return LeerJson("DatosTopJson"); }
            private set { ViewState["DatosTopJson"] = value; }
        }

        public override void ActualizarIdioma()
        {
            Title = Traductor.T("dashboard.titulo");
            lblCategoria.Text = Traductor.T("productos.categoria");
            btnFiltrar.Text = Traductor.T("dashboard.aplicarFiltros");
            btnLimpiar.Text = Traductor.T("dashboard.restablecer");

            // Con el dashboard ya cargado (se sabe recien al recuperar el
            // ViewState) se vuelve a armar para que todos sus textos cambien
            if (DatosCategoriaJson != "null")
            {
                CargarDashboard(false, false);

                ddlCategoria.Items[0].Text = Traductor.T("dashboard.todasLasCategorias");
            }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            // Las columnas se arman en el Init para que la grilla pueda reconstruirse
            // en cada postback a partir de su ViewState
            ConfigurarGrilla();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                AplicarPeriodoPorDefecto();
                CargarDashboard(false, true);
            }
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            CargarDashboard(true, false);
        }

        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            ddlCategoria.SelectedIndex = 0;
            AplicarPeriodoPorDefecto();
            CargarDashboard(false, false);
        }

        private void ConfigurarGrilla()
        {
            grillaBajoStock.ClavePrimaria = "IdProducto";
            grillaBajoStock.FilasPorPagina = 5;
            grillaBajoStock.MostrarSelectorFilas = false;

            grillaBajoStock.AgregarColumna("Codigo", "productos.codigo", "celda-codigo");
            grillaBajoStock.AgregarColumna("Nombre", "comun.nombre");
            grillaBajoStock.AgregarColumna("Categoria", "productos.categoria");
            grillaBajoStock.AgregarColumna("Stock", "dashboard.stockActual", "celda-numero");
            grillaBajoStock.AgregarColumna("StockMinimo", "dashboard.stockMinimo", "celda-numero");
        }

        
        /// Deja el filtro de fechas en el período que el dashboard muestra por defecto.
        
        private void AplicarPeriodoPorDefecto()
        {
            DateTime fechaInicio;
            DateTime fechaFin;

            bllDashboard.ObtenerPeriodoPorDefecto(out fechaInicio, out fechaFin);
            filtroFechas.EstablecerRango(fechaInicio, fechaFin);
        }

        private void CargarDashboard(bool vieneDeFiltros, bool recargarCategorias)
        {
            try
            {
                // Se consulta una sola vez por request: alimenta el combo de categorías
                // y define si hay que mostrar el aviso de "sin datos sincronizados"
                List<BEProducto> productos = bllProducto.TraerListaProductos();

                if (recargarCategorias)
                {
                    CargarCategorias(productos);
                }

                DateTime? fechaDesde = filtroFechas.FechaDesde;
                DateTime? fechaHasta = filtroFechas.FechaHasta;
                string categoria = ddlCategoria.SelectedValue;

                BEResumenDashboard resumen = bllDashboard.TraerResumen(fechaDesde, fechaHasta, categoria);
                List<BEVentaPorCategoria> porCategoria = bllDashboard.TraerVentasPorCategoria(fechaDesde, fechaHasta, categoria);
                List<BETopProducto> topProductos = bllDashboard.TraerTopProductos(fechaDesde, fechaHasta, categoria);
                List<BEProducto> bajoStock = bllDashboard.TraerProductosBajoStock(categoria);

                string mensajeVacio = vieneDeFiltros ? MensajeSinFiltros : MensajeSinVentas;

                MostrarPeriodo(fechaDesde, fechaHasta);
                MostrarIndicadores(resumen);
                MostrarGraficoCategoria(porCategoria, mensajeVacio);
                MostrarGraficoTop(topProductos, mensajeVacio);
                MostrarBajoStock(bajoStock);

                // Alternativa 2.1 del CU: sin productos sincronizados no hay nada que mostrar
                pnlSinDatos.Visible = productos.Count == 0;

                if (vieneDeFiltros && resumen.CantidadVentas == 0)
                {
                    lblAvisoFiltros.Text = Traductor.T(MensajeSinFiltros);
                }
            }
            catch (Exception ex)
            {
                lblError.Text = ex.Message;
            }
        }

        private void MostrarPeriodo(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            if (!fechaDesde.HasValue && !fechaHasta.HasValue)
            {
                lblPeriodo.Text = Traductor.T("dashboard.todoElHistorial");
                return;
            }

            lblPeriodo.Text = Traductor.T("dashboard.periodo",
                fechaDesde.HasValue ? fechaDesde.Value.ToString("dd/MM/yyyy") : Traductor.T("dashboard.inicio"),
                fechaHasta.HasValue ? fechaHasta.Value.ToString("dd/MM/yyyy") : Traductor.T("dashboard.hoy"));
        }

        private void MostrarIndicadores(BEResumenDashboard resumen)
        {
            lblFacturacionTotal.Text = Server.HtmlEncode(string.Format("${0:N2}", resumen.FacturacionTotal));
            lblCantidadVentas.Text = Server.HtmlEncode(resumen.CantidadVentas.ToString("N0"));
        }

        private void MostrarGraficoCategoria(List<BEVentaPorCategoria> porCategoria, string mensajeVacio)
        {
            // La torta se lee bien hasta unas pocas porciones: las categorías con menos
            // ventas se suman en una sola porción "Otras"
            List<BEVentaPorCategoria> porciones = bllDashboard.AgruparCategoriasMenores(porCategoria);

            List<string> etiquetas = new List<string>();
            List<decimal> valores = new List<decimal>();

            foreach (BEVentaPorCategoria fila in porciones)
            {
                etiquetas.Add(fila.Categoria);
                valores.Add(fila.Unidades);
            }

            DatosCategoriaJson = DatosGrafico.Serializar(etiquetas, valores);

            bool hayDatos = porCategoria.Count > 0;

            pnlGraficoCategoria.Visible = hayDatos;
            pnlSinCategoria.Visible = !hayDatos;
            lblSinCategoria.Text = Server.HtmlEncode(Traductor.T(mensajeVacio));
        }

        private void MostrarGraficoTop(List<BETopProducto> topProductos, string mensajeVacio)
        {
            List<string> etiquetas = new List<string>();
            List<decimal> valores = new List<decimal>();

            foreach (BETopProducto producto in topProductos)
            {
                etiquetas.Add(producto.Nombre);
                valores.Add(producto.Unidades);
            }

            DatosTopJson = DatosGrafico.Serializar(etiquetas, valores);

            bool hayDatos = topProductos.Count > 0;

            pnlGraficoTop.Visible = hayDatos;
            pnlSinTop.Visible = !hayDatos;
            lblSinTop.Text = Server.HtmlEncode(Traductor.T(mensajeVacio));

            lblTopMeta.Text = hayDatos
                ? Traductor.T("dashboard.topMeta", topProductos.Count)
                : string.Empty;
        }

        private void MostrarBajoStock(List<BEProducto> bajoStock)
        {
            grillaBajoStock.Cargar(bajoStock, MensajeSinBajoStock);

            lblCantidadBajoStock.Text = bajoStock.Count == 1
                ? Traductor.T("productos.cantidadUno")
                : Traductor.T("productos.cantidad", bajoStock.Count);
        }

        private void CargarCategorias(List<BEProducto> productos)
        {
            ddlCategoria.Items.Clear();
            ddlCategoria.Items.Add(new ListItem(Traductor.T("dashboard.todasLasCategorias"), string.Empty));

            List<string> categorias = productos
                .Select(producto => producto.Categoria)
                .Where(categoria => !string.IsNullOrWhiteSpace(categoria))
                .Distinct()
                .OrderBy(categoria => categoria)
                .ToList();

            foreach (string categoria in categorias)
            {
                ddlCategoria.Items.Add(new ListItem(categoria, categoria));
            }
        }

        private string LeerJson(string clave)
        {
            string json = Convert.ToString(ViewState[clave]);
            return string.IsNullOrEmpty(json) ? "null" : json;
        }
    }
}
