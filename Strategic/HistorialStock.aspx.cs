using BE;
using BLL;
using Services;
using Strategic.Componentes;
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Strategic
{
    // CU-004-015 - Ver Historial de Stock
    public partial class HistorialStock : Page
    {
        private const string MensajeSinProducto = "Seleccione un producto para ver su historial de stock";
        private const string MensajeSinHistorial = "El producto no posee historial de stock";
        private const string MensajeSinResultados = "No se encontraron datos con los filtros ingresados";

        private readonly BLLProducto bllProducto = new BLLProducto();

        /// <summary>
        /// JSON que consume Chart.js. Viaja en el ViewState para que el gráfico
        /// sobreviva a los postbacks del paginado de la grilla.
        /// </summary>
        protected string DatosGraficoJson
        {
            get
            {
                string json = Convert.ToString(ViewState["DatosGraficoJson"]);
                return string.IsNullOrEmpty(json) ? "null" : json;
            }
            private set { ViewState["DatosGraficoJson"] = value; }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            // Las columnas se arman en el Init para que la grilla pueda reconstruirse
            // en cada postback a partir de su ViewState
            ConfigurarGrilla();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Solo WebMaster, Administrador y Analista segun el CU
            BEUsuario usuario = SessionManager.UsuarioActual;

            if (usuario == null || (usuario.CodRol != 1 && usuario.CodRol != 2 && usuario.CodRol != 3))
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                CargarProductos();
                LimpiarHistorial(MensajeSinProducto);
            }
        }

        protected void ddlProducto_SelectedIndexChanged(object sender, EventArgs e)
        {
            ConsultarHistorial();
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            ConsultarHistorial();
        }

        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            ddlProducto.SelectedIndex = 0;
            filtroFechas.Limpiar();

            LimpiarHistorial(MensajeSinProducto);
        }

        private void ConfigurarGrilla()
        {
            grillaStock.ClavePrimaria = "IdHistorialStock";
            grillaStock.FilasPorPagina = 10;
            grillaStock.MostrarSelectorFilas = true;

            grillaStock.AgregarColumna("Fecha", "Fecha", "celda-fecha", "{0:dd/MM/yyyy HH:mm}");
            grillaStock.AgregarColumna("Stock", "Stock", "celda-numero");
        }

        private void ConsultarHistorial()
        {
            int idProducto = ObtenerProductoElegido();

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

            lblCantidad.Text = historial.Count == 1
                ? "1 registro de stock"
                : string.Format("{0} registros de stock", historial.Count);

            lblProductoElegido.Text = ddlProducto.SelectedItem != null
                ? Server.HtmlEncode(ddlProducto.SelectedItem.Text)
                : string.Empty;

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
            lblSinGrafico.Text = mensajeVacio;
        }

        private void LimpiarHistorial(string mensaje)
        {
            MostrarHistorial(new List<BEHistorialStock>(), mensaje);
            lblProductoElegido.Text = string.Empty;
        }

        private void CargarProductos()
        {
            List<BEProducto> productos = bllProducto.TraerListaProductos();

            ddlProducto.Items.Clear();
            ddlProducto.Items.Add(new ListItem("Seleccione un producto", "0"));

            foreach (BEProducto producto in productos)
            {
                ddlProducto.Items.Add(new ListItem(
                    string.Format("{0} - {1}", producto.Codigo, producto.Nombre),
                    producto.IdProducto.ToString()));
            }
        }

        private int ObtenerProductoElegido()
        {
            int idProducto;

            return int.TryParse(ddlProducto.SelectedValue, out idProducto) ? idProducto : 0;
        }
    }
}
