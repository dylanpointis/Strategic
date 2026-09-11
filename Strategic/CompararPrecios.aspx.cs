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
    // CU-002-007 - Comparar Precios con Competidores
    public partial class CompararPrecios : PaginaSegura
    {
        private const string MensajeSinMapeos = "Los productos no tienen competidores asociados";
        private const string MensajeSinResultados = "No se encontraron datos con los filtros ingresados";
        private const string MensajeSinHistorial = "La publicación todavía no tiene precios registrados";

        // Formato con signo: deja a la vista si el precio propio quedó por
        // encima o por debajo del de la competencia
        private const string FormatoDiferencia = "{0:+#,##0.0;-#,##0.0;0.0}%";

        private readonly BLLCompetencia bllCompetencia = new BLLCompetencia();
        private readonly BLLProducto bllProducto = new BLLProducto();

        /// <summary>
        /// JSON que consume Chart.js. Viaja en el ViewState para que el gráfico
        /// sobreviva a los postbacks del paginado de las grillas.
        /// </summary>
        protected string DatosHistorialJson
        {
            get
            {
                string json = Convert.ToString(ViewState["DatosHistorialJson"]);
                return string.IsNullOrEmpty(json) ? "null" : json;
            }
            private set { ViewState["DatosHistorialJson"] = value; }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            // Las columnas se arman en el Init para que las grillas puedan
            // reconstruirse en cada postback a partir de su ViewState
            ConfigurarGrillas();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarComparacionCompleta();
            }
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            try
            {
                List<BEComparacionPrecio> comparaciones = bllCompetencia.FiltrarComparacionPrecios(
                    ObtenerId(ddlProducto.SelectedValue),
                    ddlCategoria.SelectedValue,
                    ObtenerId(ddlCompetidor.SelectedValue));

                OcultarDetalle();
                MostrarComparacion(comparaciones, MensajeSinResultados);

                if (comparaciones.Count == 0)
                {
                    lblAvisoFiltros.Text = MensajeSinResultados;
                }
            }
            catch (Exception ex)
            {
                lblError.Text = ex.Message;
            }
        }

        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            ddlProducto.SelectedIndex = 0;
            ddlCategoria.SelectedIndex = 0;
            ddlCompetidor.SelectedIndex = 0;

            OcultarDetalle();
            CargarComparacionCompleta();
        }

        protected void grillaComparacion_AccionSeleccionada(object sender, AccionGrillaEventArgs e)
        {
            if (e.Comando != "VerHistorial")
            {
                return;
            }

            try
            {
                int idProductoCompetencia = Convert.ToInt32(e.ObtenerClave("IdProductoCompetencia"));

                BEComparacionPrecio comparacion = bllCompetencia.TraerComparacionPrecioPorId(idProductoCompetencia);

                if (comparacion == null)
                {
                    OcultarDetalle();
                    lblError.Text = "No se encontró la publicación seleccionada";
                    return;
                }

                MostrarDetalle(comparacion, bllCompetencia.TraerHistorialPrecioCompetencia(idProductoCompetencia));
            }
            catch (Exception ex)
            {
                lblError.Text = ex.Message;
            }
        }

        protected void btnCerrarDetalle_Click(object sender, EventArgs e)
        {
            OcultarDetalle();
        }

        private void ConfigurarGrillas()
        {
            grillaComparacion.ClavePrimaria = "IdProductoCompetencia";
            grillaComparacion.FilasPorPagina = 10;
            grillaComparacion.MostrarSelectorFilas = true;

            grillaComparacion.AgregarColumna("NombreProducto", "Producto propio");
            grillaComparacion.AgregarColumna("NombreCompetidor", "Competidor");
            grillaComparacion.AgregarColumna("PrecioPropio", "Precio propio", "celda-numero", "${0:N2}");
            grillaComparacion.AgregarColumna("PrecioCompetencia", "Precio competencia", "celda-numero", "${0:N2}");
            grillaComparacion.AgregarColumna("DiferenciaPorcentaje", "Diferencia", "celda-numero", FormatoDiferencia);
            grillaComparacion.AgregarColumna("Posicion", "Posición");
            grillaComparacion.AgregarColumna("FechaConsulta", "Última consulta", "celda-fecha", "{0:dd/MM/yyyy HH:mm}");
            grillaComparacion.AgregarColumnaAccion("Historial", "VerHistorial", "Ver historial");

            grillaHistorial.ClavePrimaria = "IdPrecioCompetencia";
            grillaHistorial.FilasPorPagina = 10;
            grillaHistorial.MostrarSelectorFilas = false;

            grillaHistorial.AgregarColumna("FechaConsulta", "Fecha de consulta", "celda-fecha", "{0:dd/MM/yyyy HH:mm}");
            grillaHistorial.AgregarColumna("PrecioPropio", "Precio propio", "celda-numero", "${0:N2}");
            grillaHistorial.AgregarColumna("PrecioCompetencia", "Precio competencia", "celda-numero", "${0:N2}");
            grillaHistorial.AgregarColumna("DiferenciaPorcentaje", "Diferencia", "celda-numero", FormatoDiferencia);
        }

        /// <summary>
        /// Carga la comparación sin filtros. Es el estado inicial de la pantalla
        /// y el que deja el botón de limpiar filtros.
        /// </summary>
        private void CargarComparacionCompleta()
        {
            // Se consulta una sola vez por request: alimenta los combos y define
            // cuál de los dos avisos del CU hay que mostrar
            List<BEProducto> productos = CargarCombos();
            List<BEComparacionPrecio> comparaciones = bllCompetencia.TraerComparacionPrecios();

            MostrarComparacion(comparaciones, MensajeSinMapeos);

            // Los avisos se resuelven acá y no al filtrar: una consulta filtrada
            // sin resultados no significa que falten datos cargados

            // Alternativa 2.1 del CU: no hay productos sincronizados
            pnlSinDatos.Visible = productos.Count == 0;

            // Alternativa 2.2 del CU: hay productos pero ninguno tiene competidores
            pnlSinMapeos.Visible = productos.Count > 0 && comparaciones.Count == 0;
        }

        private void MostrarComparacion(List<BEComparacionPrecio> comparaciones, string mensajeVacio)
        {
            grillaComparacion.Cargar(comparaciones, mensajeVacio);

            lblCantidad.Text = comparaciones.Count == 1
                ? "1 publicación"
                : string.Format("{0} publicaciones", comparaciones.Count);

            MostrarIndicadores(bllCompetencia.CalcularResumen(comparaciones));
        }

        private void MostrarIndicadores(BEResumenComparacion resumen)
        {
            lblPublicaciones.Text = Server.HtmlEncode(resumen.PublicacionesMonitoreadas.ToString("N0"));
            lblMasCaros.Text = Server.HtmlEncode(resumen.ProductosMasCaros.ToString("N0"));
            lblMasBaratos.Text = Server.HtmlEncode(resumen.ProductosMasBaratos.ToString("N0"));
            lblDiferenciaPromedio.Text = Server.HtmlEncode(string.Format(FormatoDiferencia, resumen.DiferenciaPromedio));
        }

        private void MostrarDetalle(BEComparacionPrecio comparacion, List<BEHistorialPrecioCompetencia> historial)
        {
            lblDetalleTitulo.Text = Server.HtmlEncode(string.Format("{0} - {1}",
                comparacion.CodigoProducto,
                comparacion.NombreCompetidor));

            lblDetalleProducto.Text = Server.HtmlEncode(comparacion.NombreProducto);
            lblDetalleCompetidor.Text = Server.HtmlEncode(comparacion.NombreCompetidor);
            lblDetalleMarketplace.Text = Server.HtmlEncode(comparacion.Marketplace);
            lblDetallePrecioPropio.Text = Server.HtmlEncode(string.Format("${0:N2}", comparacion.PrecioPropio));

            lblDetallePrecioCompetencia.Text = Server.HtmlEncode(comparacion.PrecioCompetencia.HasValue
                ? string.Format("${0:N2}", comparacion.PrecioCompetencia.Value)
                : "Sin registro");

            lblDetalleDiferencia.Text = Server.HtmlEncode(comparacion.DiferenciaPorcentaje.HasValue
                ? string.Format("{0} ({1})",
                    string.Format(FormatoDiferencia, comparacion.DiferenciaPorcentaje.Value),
                    comparacion.Posicion)
                : comparacion.Posicion);

            lblDetalleUltimaConsulta.Text = Server.HtmlEncode(comparacion.FechaConsulta.HasValue
                ? comparacion.FechaConsulta.Value.ToString("dd/MM/yyyy HH:mm")
                : "Sin consultas");

            MostrarPublicacion(comparacion.Url);
            MostrarHistorial(historial);

            pnlDetalle.Visible = true;
        }

        /// <summary>
        /// Muestra el enlace a la publicación del competidor. Si la dirección
        /// guardada no es una URL web se muestra como texto: un enlace armado
        /// con otro esquema podría ejecutar código en el navegador.
        /// </summary>
        private void MostrarPublicacion(string url)
        {
            bool esEnlaceWeb = EsEnlaceWeb(url);

            lnkPublicacion.Visible = esEnlaceWeb;
            lblPublicacion.Visible = !esEnlaceWeb;

            if (esEnlaceWeb)
            {
                lnkPublicacion.NavigateUrl = url;
                lnkPublicacion.Text = Server.HtmlEncode(url);
                return;
            }

            lblPublicacion.Text = Server.HtmlEncode(string.IsNullOrWhiteSpace(url) ? "Sin publicación" : url);
        }

        private bool EsEnlaceWeb(string url)
        {
            Uri direccion;

            if (!Uri.TryCreate(url, UriKind.Absolute, out direccion))
            {
                return false;
            }

            return direccion.Scheme == Uri.UriSchemeHttp || direccion.Scheme == Uri.UriSchemeHttps;
        }

        private void MostrarHistorial(List<BEHistorialPrecioCompetencia> historial)
        {
            grillaHistorial.Cargar(historial, MensajeSinHistorial);

            lblCantidadHistorial.Text = historial.Count == 1
                ? "1 consulta de precio"
                : string.Format("{0} consultas de precio", historial.Count);

            ArmarGrafico(historial);
        }

        private void ArmarGrafico(List<BEHistorialPrecioCompetencia> historial)
        {
            List<string> etiquetas = new List<string>();
            List<decimal> propios = new List<decimal>();
            List<decimal> competencia = new List<decimal>();

            foreach (BEHistorialPrecioCompetencia registro in historial)
            {
                etiquetas.Add(registro.FechaConsulta.ToString("dd/MM/yyyy"));
                propios.Add(registro.PrecioPropio);
                competencia.Add(registro.PrecioCompetencia);
            }

            List<SerieGrafico> series = new List<SerieGrafico>();

            series.Add(new SerieGrafico("Precio propio", propios));
            series.Add(new SerieGrafico("Precio competencia", competencia));

            DatosHistorialJson = DatosGrafico.Serializar(etiquetas, series);

            bool hayDatos = historial.Count > 0;

            pnlGraficoHistorial.Visible = hayDatos;
            pnlSinHistorial.Visible = !hayDatos;
            lblSinHistorial.Text = MensajeSinHistorial;
        }

        private void OcultarDetalle()
        {
            pnlDetalle.Visible = false;
            grillaHistorial.Limpiar();
            DatosHistorialJson = null;
        }

        /// <summary>
        /// Llena los tres filtros y devuelve los productos consultados, que la
        /// pantalla reutiliza para decidir los avisos.
        /// </summary>
        private List<BEProducto> CargarCombos()
        {
            List<BEProducto> productos = bllProducto.TraerListaProductos();

            ddlProducto.Items.Clear();
            ddlProducto.Items.Add(new ListItem("Todos los productos", string.Empty));

            foreach (BEProducto producto in productos)
            {
                ddlProducto.Items.Add(new ListItem(
                    string.Format("{0} - {1}", producto.Codigo, producto.Nombre),
                    producto.IdProducto.ToString()));
            }

            ddlCategoria.Items.Clear();
            ddlCategoria.Items.Add(new ListItem("Todas las categorías", string.Empty));

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

            ddlCompetidor.Items.Clear();
            ddlCompetidor.Items.Add(new ListItem("Todos los competidores", string.Empty));

            foreach (BECompetencia competidor in bllCompetencia.TraerListaCompetidores())
            {
                ddlCompetidor.Items.Add(new ListItem(
                    competidor.Nombre,
                    competidor.IdCompetencia.ToString()));
            }

            return productos;
        }

        private int? ObtenerId(string valor)
        {
            int id;

            return int.TryParse(valor, out id) ? (int?)id : null;
        }
    }
}
