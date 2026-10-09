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
    // CU-004-016 - Ver Historial de Ventas
    public partial class HistorialVentas : PaginaSegura
    {
        private const string MensajeSinVentas = "historialVentas.sinVentas";
        private const string MensajeSinResultados = "comun.sinResultadosFiltros";

        private readonly BLLVenta bllVenta = new BLLVenta();
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
            Title = Traductor.T("historialVentas.titulo");
            lblProducto.Text = Traductor.T("monitoreo.producto");
            lblCategoria.Text = Traductor.T("productos.categoria");
            lblEstado.Text = Traductor.T("comun.estado");
            btnFiltrar.Text = Traductor.T("comun.filtrar");
            btnLimpiar.Text = Traductor.T("comun.limpiarFiltros");

            // La grilla guarda la clave del mensaje vacio y la cantidad en su ViewState
            lblSinGrafico.Text = Traductor.T(grillaVentas.MensajeVacio);
            MostrarCantidad();

            // Las primeras opciones de los combos se agregan desde codigo: en
            // el Init de un postback todavia no se recuperaron del ViewState
            if (ddlProducto.Items.Count > 0)
            {
                ddlProducto.Items[0].Text = Traductor.T("comun.todos");
                ddlCategoria.Items[0].Text = Traductor.T("comun.todos");
                ddlEstado.Items[0].Text = Traductor.T("comun.todos");
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

            grillaVentas.AgregarColumna("NroVenta", "historialVentas.nroVenta", "celda-codigo");
            grillaVentas.AgregarColumna("Fecha", "comun.fecha", "celda-fecha", "{0:dd/MM/yyyy HH:mm}");
            grillaVentas.AgregarColumna("CantidadItems", "historialVentas.items", "celda-numero");
            grillaVentas.AgregarColumna("UnidadesVendidas", "historialVentas.unidades", "celda-numero");
            grillaVentas.AgregarColumna("MontoTotal", "historialVentas.montoTotal", "celda-numero", "${0:N2}");
            grillaVentas.AgregarColumna("Estado", "comun.estado");
        }

        private void MostrarVentas(List<BEVenta> ventas, string mensajeVacio)
        {
            grillaVentas.Cargar(ventas, mensajeVacio);
            MostrarCantidad();

            MostrarResumen(ventas);
            ArmarGrafico(ventas, mensajeVacio);
        }

        private void MostrarCantidad()
        {
            int cantidad = grillaVentas.CantidadRegistros;

            lblCantidad.Text = cantidad == 1
                ? Traductor.T("historialVentas.cantidadUno")
                : Traductor.T("historialVentas.cantidad", cantidad);
        }

        private void MostrarResumen(List<BEVenta> ventas)
        {
            int unidades = 0;

            foreach (BEVenta venta in ventas)
            {
                unidades = unidades + venta.UnidadesVendidas;
            }

            lblFacturacionTotal.Text = string.Format("${0:N2}", bllVenta.CalcularFacturacionTotal(ventas));
            lblCantidadVentas.Text = ventas.Count.ToString("N0");
            lblTicketPromedio.Text = string.Format("${0:N2}", bllVenta.CalcularTicketPromedio(ventas));
            lblUnidadesVendidas.Text = unidades.ToString("N0");
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
            lblSinGrafico.Text = Traductor.T(mensajeVacio);
        }

        private void CargarCombos(List<BEVenta> ventas)
        {
            List<BEProducto> productos = bllProducto.TraerListaProductos();

            ddlProducto.Items.Clear();
            ddlProducto.Items.Add(new ListItem(Traductor.T("comun.todos"), "0"));

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
            lista.Items.Add(new ListItem(Traductor.T("comun.todos"), string.Empty));

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
