using BE;
using BLL;
using Services;
using Strategic.Componentes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Strategic
{
    // CU-004-016 - Ver Historial de Ventas
    public partial class HistorialVentas : Page
    {
        private const string MensajeSinVentas = "No existen ventas registradas en el sistema";
        private const string MensajeSinResultados = "No se encontraron datos con los filtros ingresados";

        private readonly BLLVenta bllVenta = new BLLVenta();
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
                List<BEVenta> ventas = bllVenta.TraerListaVentas();

                CargarCombos(ventas);
                MostrarVentas(ventas, MensajeSinVentas);
            }
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            try
            {
                List<BEVenta> ventas = bllVenta.FiltrarVentas(
                    filtroFechas.FechaDesde,
                    filtroFechas.FechaHasta,
                    ObtenerProductoElegido(),
                    ddlCategoria.SelectedValue,
                    ddlEstado.SelectedValue);

                MostrarVentas(ventas, MensajeSinResultados);
            }
            catch (Exception ex)
            {
                lblError.Text = ex.Message;
            }
        }

        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            filtroFechas.Limpiar();
            ddlProducto.SelectedIndex = 0;
            ddlCategoria.SelectedIndex = 0;
            ddlEstado.SelectedIndex = 0;

            List<BEVenta> ventas = bllVenta.TraerListaVentas();

            CargarCombos(ventas);
            MostrarVentas(ventas, MensajeSinVentas);
        }

        private void ConfigurarGrilla()
        {
            grillaVentas.ClavePrimaria = "IdVenta";
            grillaVentas.FilasPorPagina = 10;
            grillaVentas.MostrarSelectorFilas = true;

            grillaVentas.AgregarColumna("NroVenta", "N° de venta", "celda-codigo");
            grillaVentas.AgregarColumna("Fecha", "Fecha", "celda-fecha", "{0:dd/MM/yyyy HH:mm}");
            grillaVentas.AgregarColumna("CantidadItems", "Ítems", "celda-numero");
            grillaVentas.AgregarColumna("UnidadesVendidas", "Unidades", "celda-numero");
            grillaVentas.AgregarColumna("MontoTotal", "Monto total", "celda-numero", "${0:N2}");
            grillaVentas.AgregarColumna("Estado", "Estado");
        }

        private void MostrarVentas(List<BEVenta> ventas, string mensajeVacio)
        {
            grillaVentas.Cargar(ventas, mensajeVacio);

            lblCantidad.Text = ventas.Count == 1
                ? "1 venta"
                : string.Format("{0} ventas", ventas.Count);

            MostrarResumen(ventas);
            ArmarGrafico(ventas, mensajeVacio);
        }

        private void MostrarResumen(List<BEVenta> ventas)
        {
            int unidades = 0;

            foreach (BEVenta venta in ventas)
            {
                unidades = unidades + venta.UnidadesVendidas;
            }

            litFacturacionTotal.Text = Server.HtmlEncode(string.Format("${0:N2}", bllVenta.CalcularFacturacionTotal(ventas)));
            litCantidadVentas.Text = Server.HtmlEncode(ventas.Count.ToString("N0"));
            litTicketPromedio.Text = Server.HtmlEncode(string.Format("${0:N2}", bllVenta.CalcularTicketPromedio(ventas)));
            litUnidadesVendidas.Text = Server.HtmlEncode(unidades.ToString("N0"));
        }

        private void ArmarGrafico(List<BEVenta> ventas, string mensajeVacio)
        {
            List<KeyValuePair<DateTime, decimal>> facturacion = bllVenta.AgruparFacturacionPorDia(ventas);

            List<string> etiquetas = new List<string>();
            List<decimal> valores = new List<decimal>();

            foreach (KeyValuePair<DateTime, decimal> dia in facturacion)
            {
                etiquetas.Add(dia.Key.ToString("dd/MM/yyyy"));
                valores.Add(dia.Value);
            }

            DatosGraficoJson = DatosGrafico.Serializar(etiquetas, valores);

            bool hayDatos = facturacion.Count > 0;

            pnlGrafico.Visible = hayDatos;
            pnlSinGrafico.Visible = !hayDatos;
            litSinGrafico.Text = Server.HtmlEncode(mensajeVacio);
        }

        private void CargarCombos(List<BEVenta> ventas)
        {
            List<BEProducto> productos = bllProducto.TraerListaProductos();

            ddlProducto.Items.Clear();
            ddlProducto.Items.Add(new ListItem("Todos", "0"));

            foreach (BEProducto producto in productos)
            {
                ddlProducto.Items.Add(new ListItem(
                    string.Format("{0} - {1}", producto.Codigo, producto.Nombre),
                    producto.IdProducto.ToString()));
            }

            CargarOpciones(ddlCategoria, productos.Select(producto => producto.Categoria));
            CargarOpciones(ddlEstado, ventas.Select(venta => venta.Estado));
        }

        private void CargarOpciones(DropDownList lista, IEnumerable<string> valores)
        {
            lista.Items.Clear();
            lista.Items.Add(new ListItem("Todos", string.Empty));

            List<string> opciones = valores
                .Where(valor => !string.IsNullOrWhiteSpace(valor))
                .Distinct()
                .OrderBy(valor => valor)
                .ToList();

            foreach (string opcion in opciones)
            {
                lista.Items.Add(new ListItem(opcion, opcion));
            }
        }

        private int? ObtenerProductoElegido()
        {
            int idProducto;

            if (int.TryParse(ddlProducto.SelectedValue, out idProducto) && idProducto > 0)
            {
                return idProducto;
            }

            return null;
        }
    }
}
