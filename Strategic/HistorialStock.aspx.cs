using BE;
using BLL;
using Services;
using Strategic.Componentes;
using Strategic.Seguridad;
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Strategic
{
    // CU-004-015 - Ver Historial de Stock
    public partial class HistorialStock : PaginaSegura
    {
        private const string MensajeSinProducto = "historialStock.sinProducto";
        private const string MensajeSinHistorial = "historialStock.sinHistorial";
        private const string MensajeSinResultados = "comun.sinResultadosFiltros";

        private readonly BLLProducto bllProducto = new BLLProducto();

        
        /// JSON que consume Chart.js. Viaja en el ViewState para que el gráfico
        /// sobreviva a los postbacks del paginado de la grilla.
        
        protected string DatosGraficoJson
        {
            get
            {
                string json = Convert.ToString(ViewState["DatosGraficoJson"]);
                return string.IsNullOrEmpty(json) ? "null" : json;
            }
            private set { ViewState["DatosGraficoJson"] = value; }
        }

        public override void ActualizarIdioma()
        {
            Title = Traductor.T("historialStock.titulo");
            btnFiltrar.Text = Traductor.T("comun.filtrar");
            btnLimpiar.Text = Traductor.T("comun.limpiarFiltros");

            // La grilla guarda la clave del mensaje vacio y la cantidad en su ViewState
            lblSinGrafico.Text = Traductor.T(grillaStock.MensajeVacio);
            MostrarCantidad();
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
                CargarProductos();
                LimpiarHistorial(MensajeSinProducto);
            }
        }

        protected void selectorProducto_ProductoSeleccionado(object sender, EventArgs e)
        {
            ConsultarHistorial();
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            ConsultarHistorial();
        }

        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            selectorProducto.Limpiar();
            filtroFechas.Limpiar();

            LimpiarHistorial(MensajeSinProducto);
        }

        private void ConfigurarGrilla()
        {
            grillaStock.ClavePrimaria = "IdHistorialStock";
            grillaStock.FilasPorPagina = 10;
            grillaStock.MostrarSelectorFilas = true;

            grillaStock.AgregarColumna("Fecha", "comun.fecha", "celda-fecha", "{0:dd/MM/yyyy HH:mm}");
            grillaStock.AgregarColumna("Stock", "productos.stock", "celda-numero");
        }

        private void ConsultarHistorial()
        {
            int idProducto = selectorProducto.IdProducto ?? 0;

            if (idProducto <= 0)
            {
                LimpiarHistorial(MensajeSinProducto);
                return;
            }

            try
            {
                List<BEHistorialStock> historial = bllProducto.TraerHistorialStock(
                    idProducto,
                    filtroFechas.FechaDesde,
                    filtroFechas.FechaHasta);

                bool hayFiltroDeFechas = filtroFechas.FechaDesde.HasValue || filtroFechas.FechaHasta.HasValue;

                MostrarHistorial(historial, hayFiltroDeFechas ? MensajeSinResultados : MensajeSinHistorial);
            }
            catch (Exception ex)
            {
                lblError.Text = ex.Message;
                LimpiarHistorial(MensajeSinHistorial);
            }
        }

        private void MostrarHistorial(List<BEHistorialStock> historial, string mensajeVacio)
        {
            grillaStock.Cargar(historial, mensajeVacio);
            MostrarCantidad();

            lblProductoElegido.Text = Server.HtmlEncode(selectorProducto.TextoSeleccionado);

            ArmarGrafico(historial, mensajeVacio);
        }

        private void ArmarGrafico(List<BEHistorialStock> historial, string mensajeVacio)
        {
            List<string> etiquetas = new List<string>();
            List<decimal> valores = new List<decimal>();

            foreach (BEHistorialStock registro in historial)
            {
                etiquetas.Add(registro.Fecha.ToString("dd/MM/yyyy"));
                valores.Add(Convert.ToDecimal(registro.Stock));
            }

            DatosGraficoJson = DatosGrafico.Serializar(etiquetas, valores);

            bool hayDatos = historial.Count > 0;

            pnlGrafico.Visible = hayDatos;
            pnlSinGrafico.Visible = !hayDatos;
            lblSinGrafico.Text = Traductor.T(mensajeVacio);
        }

        private void MostrarCantidad()
        {
            int cantidad = grillaStock.CantidadRegistros;

            lblCantidad.Text = cantidad == 1
                ? Traductor.T("historialStock.cantidadUno")
                : Traductor.T("historialStock.cantidad", cantidad);
        }

        private void LimpiarHistorial(string mensaje)
        {
            MostrarHistorial(new List<BEHistorialStock>(), mensaje);
            lblProductoElegido.Text = string.Empty;
        }

        private void CargarProductos()
        {
            selectorProducto.Cargar(bllProducto.TraerListaProductos());
        }
    }
}
