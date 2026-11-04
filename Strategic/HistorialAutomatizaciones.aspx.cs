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
    // CU-003-012 - Ver Historial de Automatizaciones Ejecutadas
    public partial class HistorialAutomatizaciones : PaginaSegura
    {
        private const string MensajeSinEjecuciones = "historialAutomatizaciones.sinEjecuciones";
        private const string MensajeSinResultados = "comun.sinResultadosFiltros";

        private readonly BLLAutomatizacion bllAutomatizacion = new BLLAutomatizacion();
        private readonly BLLProducto bllProducto = new BLLProducto();

        public override void ActualizarIdioma()
        {
            Title = Traductor.T("historialAutomatizaciones.titulo");
            lblAutomatizacion.Text = Traductor.T("automatizacion.automatizacion");
            lblResultado.Text = Traductor.T("automatizacion.resultado");
            ddlAutomatizacion.Items[0].Text = Traductor.T("comun.todas");
            ddlResultado.Items[0].Text = Traductor.T("comun.todos");
            ddlResultado.Items[1].Text = Traductor.T("automatizacion.resultado.Exitosa");
            ddlResultado.Items[2].Text = Traductor.T("automatizacion.resultado.ConError");
            btnFiltrar.Text = Traductor.T("comun.filtrar");
            btnLimpiar.Text = Traductor.T("comun.limpiarFiltros");
            btnCerrarDetalle.Text = Traductor.T("comun.cerrar");

            MostrarCantidad();
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            grillaHistorial.ClavePrimaria = "IdHistorial";
            grillaHistorial.FilasPorPagina = 10;
            grillaHistorial.MostrarSelectorFilas = true;

            grillaHistorial.AgregarColumna("FechaEjecucion", "comun.fecha", "celda-fecha", "{0:dd/MM/yyyy HH:mm}");
            grillaHistorial.AgregarColumna("NombreAutomatizacion", "automatizacion.automatizacion");
            grillaHistorial.AgregarColumna("NombreProducto", "monitoreo.producto");
            grillaHistorial.AgregarColumna("AccionEjecutada", "automatizacion.accion");
            grillaHistorial.AgregarColumnaTraducida("EstadoTexto", "automatizacion.resultado");
            grillaHistorial.AgregarColumnaAccion("comun.detalle", "VerDetalle", "comun.detalle");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                foreach (BEAutomatizacion automatizacion in bllAutomatizacion.TraerListaAutomatizaciones())
                {
                    ddlAutomatizacion.Items.Add(new ListItem(automatizacion.Nombre, automatizacion.IdAutomatizacion.ToString()));
                }

                selectorProducto.Cargar(bllProducto.TraerListaProductos());

                MostrarHistorial(bllAutomatizacion.TraerHistorial(), MensajeSinEjecuciones);
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
                int? idAutomatizacion = null;

                if (!string.IsNullOrEmpty(ddlAutomatizacion.SelectedValue))
                {
                    idAutomatizacion = Convert.ToInt32(ddlAutomatizacion.SelectedValue);
                }

                List<BEHistorialAutomatizacion> historial = bllAutomatizacion.FiltrarHistorial(
                    bllAutomatizacion.TraerHistorial(),
                    idAutomatizacion,
                    selectorProducto.IdProducto,
                    ddlResultado.SelectedValue,
                    filtroFechas.FechaDesde,
                    filtroFechas.FechaHasta);

                // Alternativa 4.1 [#ERR013]: la grilla muestra el mensaje vacio
                MostrarHistorial(historial, MensajeSinResultados);
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(ex.Message);
            }
        }

        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            filtroFechas.Limpiar();
            selectorProducto.Limpiar();
            ddlAutomatizacion.SelectedIndex = 0;
            ddlResultado.SelectedIndex = 0;

            MostrarHistorial(bllAutomatizacion.TraerHistorial(), MensajeSinEjecuciones);
        }

        // Pasos 5 y 6: detalle de la ejecucion elegida
        protected void grillaHistorial_AccionSeleccionada(object sender, AccionGrillaEventArgs e)
        {
            if (e.Comando != "VerDetalle")
            {
                return;
            }

            BEHistorialAutomatizacion historial = e.Elemento as BEHistorialAutomatizacion;

            if (historial == null)
            {
                pnlDetalle.Visible = false;
                return;
            }

            lblDetalleResultado.Text = Server.HtmlEncode(Traductor.T(historial.EstadoTexto));
            lblDetalleFecha.Text = historial.FechaEjecucion.ToString("dd/MM/yyyy HH:mm");
            lblDetalleAutomatizacion.Text = Server.HtmlEncode(historial.NombreAutomatizacion);
            lblDetalleProducto.Text = Server.HtmlEncode(historial.NombreProducto);
            lblDetalleAccion.Text = Server.HtmlEncode(historial.AccionEjecutada);
            lblDetalleValorAnterior.Text = Server.HtmlEncode(historial.ValorAnterior);
            lblDetalleValorNuevo.Text = Server.HtmlEncode(historial.ValorNuevo);

            pnlDetalle.Visible = true;
        }

        protected void btnCerrarDetalle_Click(object sender, EventArgs e)
        {
            pnlDetalle.Visible = false;
        }

        private void MostrarHistorial(List<BEHistorialAutomatizacion> historial, string mensajeVacio)
        {
            pnlDetalle.Visible = false;
            grillaHistorial.Cargar(historial, mensajeVacio);
            MostrarCantidad();
        }

        private void MostrarCantidad()
        {
            int cantidad = grillaHistorial.CantidadRegistros;

            lblCantidad.Text = cantidad == 1
                ? Traductor.T("historialAutomatizaciones.cantidadUno")
                : Traductor.T("historialAutomatizaciones.cantidad", cantidad);
        }
    }
}
