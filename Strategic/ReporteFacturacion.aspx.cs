using BE;
using BLL;
using Services;
using Strategic.Componentes;
using Strategic.Seguridad;
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;

namespace Strategic
{
    // Reporte de facturacion de la suscripcion de esta instancia. Lo usa el
    // personal de Strategic (rol Soporte Strategic), no el cliente
    public partial class ReporteFacturacion : PaginaSegura
    {
        private const string MensajeSinPagos = "reporteFacturacion.sinPagos";
        private const string MensajeSinResultados = "comun.sinResultadosFiltros";

        private readonly BLLSuscripcion bllSuscripcion = new BLLSuscripcion();

        // JSON que consume Chart.js. Viaja en el ViewState para que el grafico
        // sobreviva a los postbacks del paginado de la grilla
        protected string DatosGraficoJson
        {
            get
            {
                string json = Convert.ToString(ViewState["DatosGraficoJson"]);
                return string.IsNullOrEmpty(json) ? "null" : json;
            }
            private set { ViewState["DatosGraficoJson"] = value; }
        }

        // Clave de traduccion del estado de la suscripcion que se esta mostrando
        private string EstadoActual
        {
            get { return ViewState["EstadoActual"] as string; }
            set { ViewState["EstadoActual"] = value; }
        }

        public override void ActualizarIdioma()
        {
            Title = Traductor.T("reporteFacturacion.titulo");
            lblAgrupacion.Text = Traductor.T("reporteFacturacion.agrupar");
            btnFiltrar.Text = Traductor.T("comun.filtrar");
            btnLimpiar.Text = Traductor.T("comun.limpiarFiltros");

            lblSinGrafico.Text = Traductor.T(grillaPagos.MensajeVacio);
            MostrarCantidad();

            // Las opciones del combo se agregan desde codigo: en el Init de un
            // postback todavia no se recuperaron del ViewState
            if (ddlAgrupacion.Items.Count > 0)
            {
                ddlAgrupacion.Items[0].Text = Traductor.T("reporteFacturacion.porMes");
                ddlAgrupacion.Items[1].Text = Traductor.T("reporteFacturacion.porSemana");
                MostrarTituloGrafico();
            }

            if (EstadoActual != null)
            {
                lblEstadoSuscripcion.Text = Traductor.T(EstadoActual);
            }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            ConfigurarGrilla();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarAgrupaciones();
                MostrarEstadoSuscripcion();
                MostrarPagos(bllSuscripcion.FiltrarPagos(null, null), MensajeSinPagos);
            }
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            if (!filtroFechas.Validar())
            {
                return;
            }

            try
            {
                List<BESuscripcion> pagos = bllSuscripcion.FiltrarPagos(filtroFechas.FechaDesde, filtroFechas.FechaHasta);

                MostrarPagos(pagos, MensajeSinResultados);
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(ex.Message);
            }
        }

        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            filtroFechas.Limpiar();
            ddlAgrupacion.SelectedIndex = 0;

            MostrarPagos(bllSuscripcion.FiltrarPagos(null, null), MensajeSinPagos);
        }

        private void ConfigurarGrilla()
        {
            grillaPagos.ClavePrimaria = "IdSuscripcion";
            grillaPagos.FilasPorPagina = 10;
            grillaPagos.MostrarSelectorFilas = true;

            grillaPagos.AgregarColumna("FechaPago", "suscripcion.fechaPago", "celda-fecha", "{0:dd/MM/yyyy HH:mm}");
            grillaPagos.AgregarColumna("Periodo", "suscripcion.periodo", "celda-fecha");
            grillaPagos.AgregarColumna("Plan", "suscripcion.plan");
            grillaPagos.AgregarColumna("Monto", "suscripcion.monto", "celda-numero", "${0:N2}");
            grillaPagos.AgregarColumnaTraducida("EstadoTexto", "comun.estado");

            // Completo y desencriptado: es lo que Strategic compara contra su
            // cuenta para verificar que la transferencia llego
            grillaPagos.AgregarColumna("NumeroOperacion", "suscripcion.numeroOperacion", "celda-codigo");
        }

        private void CargarAgrupaciones()
        {
            ddlAgrupacion.Items.Clear();
            ddlAgrupacion.Items.Add(new ListItem(Traductor.T("reporteFacturacion.porMes"), BLLSuscripcion.AgrupacionMes));
            ddlAgrupacion.Items.Add(new ListItem(Traductor.T("reporteFacturacion.porSemana"), BLLSuscripcion.AgrupacionSemana));
        }

        private void MostrarPagos(List<BESuscripcion> pagos, string mensajeVacio)
        {
            grillaPagos.Cargar(pagos, mensajeVacio);
            MostrarCantidad();

            lblFacturacionTotal.Text = string.Format("${0:N2}", bllSuscripcion.CalcularFacturacionTotal(pagos));
            lblCantidadPagos.Text = pagos.Count.ToString("N0");
            lblPagoPromedio.Text = string.Format("${0:N2}", bllSuscripcion.CalcularPagoPromedio(pagos));

            ArmarGrafico(pagos, mensajeVacio);
        }

        // Estado de la suscripcion hoy, sin importar el filtro de fechas
        private void MostrarEstadoSuscripcion()
        {
            BESuscripcion actual = bllSuscripcion.TraerSuscripcionActual(bllSuscripcion.TraerListaSuscripciones());

            if (actual == null)
            {
                EstadoActual = "reporteFacturacion.sinSuscripcion";
                lblEstadoSuscripcion.Text = Traductor.T(EstadoActual);
                return;
            }

            EstadoActual = actual.EstadoTexto;
            lblEstadoSuscripcion.Text = Traductor.T(EstadoActual);
            lblVencimiento.Text = Server.HtmlEncode(Traductor.T(
                "reporteFacturacion.venceEl",
                actual.FechaVencimiento.ToString("dd/MM/yyyy")));
        }

        private void MostrarCantidad()
        {
            int cantidad = grillaPagos.CantidadRegistros;

            lblCantidad.Text = cantidad == 1
                ? Traductor.T("reporteFacturacion.cantidadUno")
                : Traductor.T("reporteFacturacion.cantidad", cantidad);
        }

        private bool AgrupaPorSemana
        {
            get { return ddlAgrupacion.SelectedValue == BLLSuscripcion.AgrupacionSemana; }
        }

        private void MostrarTituloGrafico()
        {
            lblTituloGrafico.Text = Traductor.T(AgrupaPorSemana
                ? "reporteFacturacion.facturacionPorSemana"
                : "reporteFacturacion.facturacionPorMes");
        }

        private void ArmarGrafico(List<BESuscripcion> pagos, string mensajeVacio)
        {
            List<KeyValuePair<DateTime, decimal>> facturacion = bllSuscripcion.AgruparFacturacion(
                pagos,
                ddlAgrupacion.SelectedValue);

            List<string> etiquetas = new List<string>();
            List<decimal> valores = new List<decimal>();

            foreach (KeyValuePair<DateTime, decimal> periodo in facturacion)
            {
                etiquetas.Add(AgrupaPorSemana
                    ? periodo.Key.ToString("dd/MM/yyyy")
                    : periodo.Key.ToString("MM/yyyy"));

                valores.Add(periodo.Value);
            }

            DatosGraficoJson = DatosGrafico.Serializar(etiquetas, valores);
            MostrarTituloGrafico();

            bool hayDatos = facturacion.Count > 0;

            pnlGrafico.Visible = hayDatos;
            pnlSinGrafico.Visible = !hayDatos;
            lblSinGrafico.Text = Traductor.T(mensajeVacio);
        }
    }
}
