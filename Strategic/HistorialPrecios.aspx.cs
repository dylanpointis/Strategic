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
    // CU-004-014 - Ver Historial de Precios
    public partial class HistorialPrecios : PaginaSegura
    {
        private const string MensajeSinProducto = "historialPrecios.sinProducto";
        private const string MensajeSinHistorial = "historialPrecios.sinHistorial";
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
            Title = Traductor.T("historialPrecios.titulo");
            lblProducto.Text = Traductor.T("monitoreo.producto");
            btnFiltrar.Text = Traductor.T("comun.filtrar");
            btnLimpiar.Text = Traductor.T("comun.limpiarFiltros");

            // La grilla guarda la clave del mensaje vacio y la cantidad en su ViewState
            lblSinGrafico.Text = Traductor.T(grillaPrecios.MensajeVacio);
            MostrarCantidad();

            // La primera opcion del combo se agrega desde codigo: en el Init
            // de un postback todavia no se recupero del ViewState
            if (ddlProducto.Items.Count > 0)
            {
                ddlProducto.Items[0].Text = Traductor.T("historial.seleccioneProducto");
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
            grillaPrecios.ClavePrimaria = "IdHistorialPrecio";
            grillaPrecios.FilasPorPagina = 10;
            grillaPrecios.MostrarSelectorFilas = true;

            grillaPrecios.AgregarColumna("Fecha", "comun.fecha", "celda-fecha", "{0:dd/MM/yyyy HH:mm}");
            grillaPrecios.AgregarColumna("Precio", "productos.precio", "celda-numero", "${0:N2}");
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
                List<BEHistorialPrecio> historial = bllProducto.TraerHistorialPrecios(
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

        private void MostrarHistorial(List<BEHistorialPrecio> historial, string mensajeVacio)
        {
            grillaPrecios.Cargar(historial, mensajeVacio);
            MostrarCantidad();

            lblProductoElegido.Text = ddlProducto.SelectedItem != null
                ? Server.HtmlEncode(ddlProducto.SelectedItem.Text)
                : string.Empty;

            ArmarGrafico(historial, mensajeVacio);
        }

        private void ArmarGrafico(List<BEHistorialPrecio> historial, string mensajeVacio)
        {
            List<string> etiquetas = new List<string>();
            List<decimal> valores = new List<decimal>();

            foreach (BEHistorialPrecio registro in historial)
            {
                etiquetas.Add(registro.Fecha.ToString("dd/MM/yyyy"));
                valores.Add(Convert.ToDecimal(registro.Precio));
            }

            DatosGraficoJson = DatosGrafico.Serializar(etiquetas, valores);

            bool hayDatos = historial.Count > 0;

            pnlGrafico.Visible = hayDatos;
            pnlSinGrafico.Visible = !hayDatos;
            lblSinGrafico.Text = Traductor.T(mensajeVacio);
        }

        private void MostrarCantidad()
        {
            int cantidad = grillaPrecios.CantidadRegistros;

            lblCantidad.Text = cantidad == 1
                ? Traductor.T("historialPrecios.cantidadUno")
                : Traductor.T("historialPrecios.cantidad", cantidad);
        }

        private void LimpiarHistorial(string mensaje)
        {
            MostrarHistorial(new List<BEHistorialPrecio>(), mensaje);
            lblProductoElegido.Text = string.Empty;
        }

        private void CargarProductos()
        {
            List<BEProducto> productos = bllProducto.TraerListaProductos();

            ddlProducto.Items.Clear();
            ddlProducto.Items.Add(new ListItem(Traductor.T("historial.seleccioneProducto"), "0"));

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
