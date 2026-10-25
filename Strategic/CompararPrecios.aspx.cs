using BE;
using BLL;
using Services;
using Strategic.Componentes;
using Strategic.Seguridad;
using Strategic.ServiciosWeb;
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
        private const string MensajeSinMapeos = "compararPrecios.sinMapeos";
        private const string MensajeSinResultados = "comun.sinResultadosFiltros";
        private const string MensajeSinHistorial = "compararPrecios.sinHistorial";

        // Formato con signo: deja a la vista si el precio propio quedó por
        // encima o por debajo del de la competencia
        private const string FormatoDiferencia = "{0:+#,##0.0;-#,##0.0;0.0}%";

        private readonly BLLCompetencia bllCompetencia = new BLLCompetencia();
        private readonly BLLProducto bllProducto = new BLLProducto();

        
        /// JSON que consume Chart.js. Viaja en el ViewState para que el gráfico
        /// sobreviva a los postbacks del paginado de las grillas.
        
        protected string DatosHistorialJson
        {
            get
            {
                string json = Convert.ToString(ViewState["DatosHistorialJson"]);
                return string.IsNullOrEmpty(json) ? "null" : json;
            }
            private set { ViewState["DatosHistorialJson"] = value; }
        }

        private int PublicacionSeleccionada
        {
            get { return ViewState["PublicacionSeleccionada"] == null ? 0 : Convert.ToInt32(ViewState["PublicacionSeleccionada"]); }
            set { ViewState["PublicacionSeleccionada"] = value; }
        }

        public override void ActualizarIdioma()
        {
            Title = Traductor.T("compararPrecios.titulo");
            lblProducto.Text = Traductor.T("monitoreo.producto");
            lblCategoria.Text = Traductor.T("productos.categoria");
            lblCompetidor.Text = Traductor.T("mapearProductos.competidor");
            btnFiltrar.Text = Traductor.T("comun.filtrar");
            btnLimpiar.Text = Traductor.T("comun.limpiarFiltros");
            btnCerrarDetalle.Text = Traductor.T("compararPrecios.cerrarHistorial");
            MostrarCantidad();

            // Lo que se arma desde codigo solo existe una vez recuperado el
            // ViewState, por eso se pregunta antes de traducirlo
            if (ddlProducto.Items.Count > 0)
            {
                ddlProducto.Items[0].Text = Traductor.T("monitoreo.todosLosProductos");
                ddlCategoria.Items[0].Text = Traductor.T("dashboard.todasLasCategorias");
                ddlCompetidor.Items[0].Text = Traductor.T("monitoreo.todosLosCompetidores");
            }

            if (PublicacionSeleccionada > 0)
            {
                VerHistorial(PublicacionSeleccionada);
            }
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
                List<BEComparacionPrecio> comparaciones = ConsultarComparacion(
                    ObtenerId(ddlProducto.SelectedValue),
                    ddlCategoria.SelectedValue,
                    ObtenerId(ddlCompetidor.SelectedValue));

                OcultarDetalle();
                MostrarComparacion(comparaciones, MensajeSinResultados);

                if (comparaciones.Count == 0)
                {
                    lblAvisoFiltros.Text = Traductor.T(MensajeSinResultados);
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

            VerHistorial(Convert.ToInt32(e.ObtenerClave("IdProductoCompetencia")));
        }

        private void VerHistorial(int idProductoCompetencia)
        {
            try
            {
                BEComparacionPrecio comparacion = bllCompetencia.TraerComparacionPrecioPorId(idProductoCompetencia);

                if (comparacion == null)
                {
                    OcultarDetalle();
                    lblError.Text = Traductor.T("gestionarPublicacion.noEncontrada");
                    return;
                }

                PublicacionSeleccionada = idProductoCompetencia;
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

            grillaComparacion.AgregarColumna("NombreProducto", "mapearProductos.productoPropio");
            grillaComparacion.AgregarColumna("NombreCompetidor", "mapearProductos.competidor");
            grillaComparacion.AgregarColumna("PrecioPropio", "compararPrecios.precioPropio", "celda-numero", "${0:N2}");
            grillaComparacion.AgregarColumna("PrecioCompetencia", "compararPrecios.precioCompetencia", "celda-numero", "${0:N2}");
            grillaComparacion.AgregarColumna("DiferenciaPorcentaje", "compararPrecios.diferencia", "celda-numero", FormatoDiferencia);
            grillaComparacion.AgregarColumnaTraducida("Posicion", "compararPrecios.posicion");
            grillaComparacion.AgregarColumna("FechaConsulta", "compararPrecios.ultimaConsulta", "celda-fecha", "{0:dd/MM/yyyy HH:mm}");
            grillaComparacion.AgregarColumnaAccion("compararPrecios.historial", "VerHistorial", "compararPrecios.verHistorial");

            grillaHistorial.ClavePrimaria = "IdPrecioCompetencia";
            grillaHistorial.FilasPorPagina = 10;
            grillaHistorial.MostrarSelectorFilas = false;

            grillaHistorial.AgregarColumna("FechaConsulta", "compararPrecios.fechaConsulta", "celda-fecha", "{0:dd/MM/yyyy HH:mm}");
            grillaHistorial.AgregarColumna("PrecioPropio", "compararPrecios.precioPropio", "celda-numero", "${0:N2}");
            grillaHistorial.AgregarColumna("PrecioCompetencia", "compararPrecios.precioCompetencia", "celda-numero", "${0:N2}");
            grillaHistorial.AgregarColumna("DiferenciaPorcentaje", "compararPrecios.diferencia", "celda-numero", FormatoDiferencia);
        }

        
        /// Carga la comparación sin filtros. Es el estado inicial de la pantalla
        /// y el que deja el botón de limpiar filtros.
        
        private void CargarComparacionCompleta()
        {
            try
            {
                // Se consulta una sola vez por request: alimenta los combos y define
                // cuál de los dos avisos del CU hay que mostrar
                List<BEProducto> productos = CargarCombos();
                List<BEComparacionPrecio> comparaciones = ConsultarComparacion(null, null, null);

                MostrarComparacion(comparaciones, MensajeSinMapeos);

                // Los avisos se resuelven acá y no al filtrar: una consulta filtrada
                // sin resultados no significa que falten datos cargados

                // Alternativa 2.1 del CU: no hay productos sincronizados
                pnlSinDatos.Visible = productos.Count == 0;

                // Alternativa 2.2 del CU: hay productos pero ninguno tiene competidores
                pnlSinMapeos.Visible = productos.Count > 0 && comparaciones.Count == 0;
            }
            catch (Exception ex)
            {
                lblError.Text = ex.Message;
            }
        }

        private void MostrarComparacion(List<BEComparacionPrecio> comparaciones, string mensajeVacio)
        {
            grillaComparacion.Cargar(comparaciones, mensajeVacio);
            MostrarCantidad();

            MostrarIndicadores(bllCompetencia.CalcularResumen(comparaciones));
        }

        private void MostrarCantidad()
        {
            int cantidad = grillaComparacion.CantidadRegistros;

            lblCantidad.Text = cantidad == 1
                ? Traductor.T("monitoreo.cantidadUno")
                : Traductor.T("monitoreo.cantidad", cantidad);
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
                : Traductor.T("compararPrecios.posicion.sinRegistro"));

            lblDetalleDiferencia.Text = Server.HtmlEncode(comparacion.DiferenciaPorcentaje.HasValue
                ? string.Format("{0} ({1})",
                    string.Format(FormatoDiferencia, comparacion.DiferenciaPorcentaje.Value),
                    Traductor.T(comparacion.Posicion))
                : Traductor.T(comparacion.Posicion));

            lblDetalleUltimaConsulta.Text = Server.HtmlEncode(comparacion.FechaConsulta.HasValue
                ? comparacion.FechaConsulta.Value.ToString("dd/MM/yyyy HH:mm")
                : Traductor.T("compararPrecios.sinConsultas"));

            MostrarPublicacion(comparacion.Url);
            MostrarHistorial(historial);

            pnlDetalle.Visible = true;
        }

        
        /// Muestra el enlace a la publicación del competidor. Si la dirección
        /// guardada no es una URL web se muestra como texto: un enlace armado
        /// con otro esquema podría ejecutar código en el navegador.
        
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

            lblPublicacion.Text = Server.HtmlEncode(string.IsNullOrWhiteSpace(url) ? Traductor.T("compararPrecios.sinPublicacion") : url);
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
                ? Traductor.T("compararPrecios.consultasUno")
                : Traductor.T("compararPrecios.consultas", historial.Count);

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

            series.Add(new SerieGrafico(Traductor.T("compararPrecios.precioPropio"), propios));
            series.Add(new SerieGrafico(Traductor.T("compararPrecios.precioCompetencia"), competencia));

            DatosHistorialJson = DatosGrafico.Serializar(etiquetas, series);

            bool hayDatos = historial.Count > 0;

            pnlGraficoHistorial.Visible = hayDatos;
            pnlSinHistorial.Visible = !hayDatos;
            lblSinHistorial.Text = Traductor.T(MensajeSinHistorial);
        }

        private void OcultarDetalle()
        {
            pnlDetalle.Visible = false;
            PublicacionSeleccionada = 0;
            grillaHistorial.Limpiar();
            DatosHistorialJson = null;
        }

        
        /// Llena los tres filtros y devuelve los productos consultados, que la
        /// pantalla reutiliza para decidir los avisos.
        
        private List<BEProducto> CargarCombos()
        {
            List<BEProducto> productos = bllProducto.TraerListaProductos();

            ddlProducto.Items.Clear();
            ddlProducto.Items.Add(new ListItem(Traductor.T("monitoreo.todosLosProductos"), string.Empty));

            foreach (BEProducto producto in productos)
            {
                ddlProducto.Items.Add(new ListItem(
                    string.Format("{0} - {1}", producto.Codigo, producto.Nombre),
                    producto.IdProducto.ToString()));
            }

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

            ddlCompetidor.Items.Clear();
            ddlCompetidor.Items.Add(new ListItem(Traductor.T("monitoreo.todosLosCompetidores"), string.Empty));

            foreach (BECompetencia competidor in bllCompetencia.TraerListaCompetidores())
            {
                ddlCompetidor.Items.Add(new ListItem(
                    competidor.Nombre,
                    competidor.IdCompetencia.ToString()));
            }

            return productos;
        }

        // La comparacion se pide al web service EstadisticasCompetencia
        private List<BEComparacionPrecio> ConsultarComparacion(int? idProducto, string categoria, int? idCompetencia)
        {
            using (ClienteEstadisticasCompetencia competencia = new ClienteEstadisticasCompetencia())
            {
                return competencia.ObtenerComparacionPreciosCompetencia(
                    idProducto ?? 0,
                    categoria,
                    idCompetencia ?? 0).ToList();
            }
        }

        private int? ObtenerId(string valor)
        {
            int id;

            return int.TryParse(valor, out id) ? (int?)id : null;
        }
    }
}
