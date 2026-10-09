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
    // CU-002-006 - Generar Predicciones
    public partial class Predicciones : PaginaSegura
    {
        private readonly BLLPrediccion bllPrediccion = new BLLPrediccion();
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

        // Solo puede ser "$" o vacio, no es dato del usuario
        protected string PrefijoGrafico
        {
            get { return Convert.ToString(ViewState["PrefijoGrafico"]); }
            private set { ViewState["PrefijoGrafico"] = value; }
        }

        public override void ActualizarIdioma()
        {
            Title = Traductor.T("predicciones.titulo");
            lblTipo.Text = Traductor.T("predicciones.tipo");
            lblProducto.Text = Traductor.T("monitoreo.producto");
            lblHorizonte.Text = Traductor.T("predicciones.horizonte");
            btnGenerar.Text = Traductor.T("predicciones.generar");
            btnLimpiar.Text = Traductor.T("comun.limpiar");

            foreach (ListItem dias in ddlHorizonte.Items)
            {
                dias.Text = Traductor.T("predicciones.dias", dias.Value);
            }

            // Lo que se arma desde codigo solo existe una vez recuperado el
            // ViewState, por eso se pregunta antes de traducirlo
            if (ddlProducto.Items.Count > 0)
            {
                ddlProducto.Items[0].Text = Traductor.T("historial.seleccioneProducto");

                foreach (KeyValuePair<string, string> tipo in bllPrediccion.TraerTipos())
                {
                    ddlTipo.Items.FindByValue(tipo.Key).Text = tipo.Value;
                }
            }

            // El resultado se vuelve a generar para que sus textos salgan en el idioma nuevo
            if (pnlResultado.Visible)
            {
                GenerarPrediccion();
            }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            // Las columnas se arman en el Init para que la grilla pueda
            // reconstruirse en cada postback a partir de su ViewState
            ConfigurarGrilla();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarCombos();
            }
        }

        protected void btnGenerar_Click(object sender, EventArgs e)
        {
            GenerarPrediccion();
        }

        private void GenerarPrediccion()
        {
            int idProducto;
            int horizonte;

            int.TryParse(ddlProducto.SelectedValue, out idProducto);
            int.TryParse(ddlHorizonte.SelectedValue, out horizonte);

            try
            {
                BEPrediccion prediccion = bllPrediccion.Generar(ddlTipo.SelectedValue, idProducto, horizonte);

                MostrarPrediccion(prediccion);
            }
            catch (Exception ex)
            {
                pnlResultado.Visible = false;
                lblError.Text = Server.HtmlEncode(ex.Message);
            }
        }

        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            ddlTipo.SelectedIndex = 0;
            ddlProducto.SelectedIndex = 0;
            ddlHorizonte.ClearSelection();
            ddlHorizonte.Items.FindByValue("30").Selected = true;

            pnlResultado.Visible = false;
            grillaProyeccion.Limpiar();
            DatosGraficoJson = null;
        }

        private void ConfigurarGrilla()
        {
            grillaProyeccion.ClavePrimaria = "Fecha";
            grillaProyeccion.FilasPorPagina = 10;
            grillaProyeccion.MostrarSelectorFilas = true;

            grillaProyeccion.AgregarColumna("Fecha", "comun.fecha", "celda-fecha", "{0:dd/MM/yyyy}");
            grillaProyeccion.AgregarColumna("Valor", "predicciones.valorProyectado", "celda-numero", "{0:N2}");
        }

        private void MostrarPrediccion(BEPrediccion prediccion)
        {
            string prefijo = prediccion.Prefijo;

            lblResumen.Text = Server.HtmlEncode(string.Format("{0} - {1}", prediccion.NombreTipo, prediccion.NombreProducto));

            lblValorActual.Text = Server.HtmlEncode(Formatear(prefijo, prediccion.ValorActual));
            lblFechaBase.Text = Server.HtmlEncode(Traductor.T("predicciones.al", prediccion.FechaBase.ToString("dd/MM/yyyy")));

            lblValorPredicho.Text = Server.HtmlEncode(Formatear(prefijo, prediccion.ValorPredicho));
            lblFechaFin.Text = Server.HtmlEncode(Traductor.T("predicciones.alConDias", prediccion.FechaFin.ToString("dd/MM/yyyy"), prediccion.HorizonteDias));

            lblVariacion.Text = Server.HtmlEncode(string.Format("{0:+#,##0.0;-#,##0.0;0.0}%", prediccion.VariacionPorcentaje));
            lblTendencia.Text = Server.HtmlEncode(prediccion.Tendencia);

            lblConfianza.Text = Server.HtmlEncode(prediccion.Confianza);
            lblR2.Text = Server.HtmlEncode(Traductor.T("predicciones.r2", prediccion.R2, prediccion.CantidadPuntos));

            lblDetalleRecta.Text = Server.HtmlEncode(Traductor.T("predicciones.pendiente",
                prefijo, prediccion.Pendiente, prediccion.Unidad));

            pnlQuiebre.Visible = prediccion.FechaQuiebre.HasValue;

            if (prediccion.FechaQuiebre.HasValue)
            {
                int dias = (int)(prediccion.FechaQuiebre.Value - DateTime.Today).TotalDays;

                lblQuiebre.Text = Server.HtmlEncode(Traductor.T(
                    "predicciones.quiebre",
                    prediccion.FechaQuiebre.Value.ToString("dd/MM/yyyy"),
                    dias <= 0 ? Traductor.T("predicciones.yaEnCero") : Traductor.T("predicciones.enDias", dias)));
            }

            grillaProyeccion.Cargar(prediccion.Proyeccion, "predicciones.sinPuntos");
            lblCantidad.Text = Server.HtmlEncode(Traductor.T("predicciones.puntos", prediccion.Proyeccion.Count));

            ArmarGrafico(prediccion);

            pnlResultado.Visible = true;
        }

        
        /// Dos series sobre el mismo eje: el histórico y, a continuación, la
        /// proyección punteada. La proyección arranca en el último dato real
        /// para que la línea no quede cortada.
        
        private void ArmarGrafico(BEPrediccion prediccion)
        {
            List<string> etiquetas = new List<string>();
            List<decimal?> historico = new List<decimal?>();
            List<decimal?> proyeccion = new List<decimal?>();

            for (int i = 0; i < prediccion.Historial.Count; i++)
            {
                BEPuntoPrediccion punto = prediccion.Historial[i];
                bool esElUltimo = i == prediccion.Historial.Count - 1;

                etiquetas.Add(punto.Fecha.ToString("dd/MM"));
                historico.Add(punto.Valor);
                proyeccion.Add(esElUltimo ? punto.Valor : (decimal?)null);
            }

            foreach (BEPuntoPrediccion punto in prediccion.Proyeccion)
            {
                etiquetas.Add(punto.Fecha.ToString("dd/MM"));
                historico.Add(null);
                proyeccion.Add(punto.Valor);
            }

            List<SerieGrafico> series = new List<SerieGrafico>();

            series.Add(new SerieGrafico(Traductor.T("predicciones.historico"), historico, false));
            series.Add(new SerieGrafico(Traductor.T("predicciones.proyeccion"), proyeccion, true));

            DatosGraficoJson = DatosGrafico.Serializar(etiquetas, series);
            PrefijoGrafico = prediccion.Prefijo;
        }

        private void CargarCombos()
        {
            ddlTipo.Items.Clear();

            foreach (KeyValuePair<string, string> tipo in bllPrediccion.TraerTipos())
            {
                ddlTipo.Items.Add(new ListItem(tipo.Value, tipo.Key));
            }

            List<BEProducto> productos = bllProducto.TraerListaProductos();

            ddlProducto.Items.Clear();
            ddlProducto.Items.Add(new ListItem(Traductor.T("historial.seleccioneProducto"), "0"));

            foreach (BEProducto producto in productos)
            {
                ddlProducto.Items.Add(new ListItem(
                    string.Format("{0} - {1}", producto.Codigo, producto.Nombre),
                    producto.IdProducto.ToString()));
            }

            // Sin productos sincronizados no hay historico que proyectar
            pnlSinDatos.Visible = productos.Count == 0;
        }

        private string Formatear(string prefijo, decimal valor)
        {
            return prefijo + valor.ToString("N2");
        }
    }
}
