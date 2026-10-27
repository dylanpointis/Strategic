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
    // CU-006-031 - Consultar Historial de Sincronizaciones
    public partial class HistorialSincronizaciones : PaginaSegura
    {
        private const string MensajeSinSincronizaciones = "historialSincronizaciones.sinSincronizaciones";
        private const string MensajeSinResultados = "comun.sinResultadosFiltros";

        private static readonly string[] Estados =
        {
            BEResultadoIntegracion.EstadoExitosa,
            BEResultadoIntegracion.EstadoConErrores,
            BEResultadoIntegracion.EstadoFallida
        };

        private readonly BLLIntegracion bllIntegracion = new BLLIntegracion();

        public override void ActualizarIdioma()
        {
            Title = Traductor.T("historialSincronizaciones.titulo");
            lblTipo.Text = Traductor.T("integracion.tipoArchivo");
            lblEstado.Text = Traductor.T("comun.estado");
            btnFiltrar.Text = Traductor.T("comun.filtrar");
            btnLimpiar.Text = Traductor.T("comun.limpiarFiltros");
            btnCerrarDetalle.Text = Traductor.T("comun.cerrar");
            lblSinErrores.Text = Traductor.T("historialSincronizaciones.sinErrores");

            MostrarCantidad();

            // Las opciones se agregan desde codigo: en el Init de un postback
            // todavia no se recuperaron del ViewState
            if (ddlTipo.Items.Count > 0)
            {
                TraducirOpciones(ddlTipo, "integracion.tipo.");
                TraducirOpciones(ddlEstado, "integracion.estado.");
            }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            grillaHistorial.ClavePrimaria = "IdSincronizacion";
            grillaHistorial.FilasPorPagina = 10;
            grillaHistorial.MostrarSelectorFilas = true;

            grillaHistorial.AgregarColumna("FechaEjecucion", "comun.fecha", "celda-fecha", "{0:dd/MM/yyyy HH:mm}");
            grillaHistorial.AgregarColumnaTraducida("TipoTexto", "integracion.tipoArchivo");
            grillaHistorial.AgregarColumna("RegistrosImportados", "integracion.importados", "celda-numero");
            grillaHistorial.AgregarColumna("RegistrosActualizados", "integracion.actualizados", "celda-numero");
            grillaHistorial.AgregarColumna("RegistrosError", "integracion.conError", "celda-numero");
            grillaHistorial.AgregarColumnaTraducida("EstadoTexto", "comun.estado");
            grillaHistorial.AgregarColumnaAccion("comun.detalle", "VerDetalle", "historialSincronizaciones.verDetalle");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarOpciones(ddlTipo, BLLIntegracion.TiposArchivo, "integracion.tipo.");
                CargarOpciones(ddlEstado, Estados, "integracion.estado.");

                MostrarHistorial(bllIntegracion.TraerHistorialSincronizaciones(), MensajeSinSincronizaciones);
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
                List<BEHistorialSincronizacion> historial = bllIntegracion.FiltrarHistorial(
                    bllIntegracion.TraerHistorialSincronizaciones(),
                    ddlTipo.SelectedValue,
                    ddlEstado.SelectedValue,
                    filtroFechas.FechaDesde,
                    filtroFechas.FechaHasta);

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
            ddlTipo.SelectedIndex = 0;
            ddlEstado.SelectedIndex = 0;

            MostrarHistorial(bllIntegracion.TraerHistorialSincronizaciones(), MensajeSinSincronizaciones);
        }

        // Pasos 5 y 6: detalle de la sincronizacion elegida
        protected void grillaHistorial_AccionSeleccionada(object sender, AccionGrillaEventArgs e)
        {
            if (e.Comando != "VerDetalle")
            {
                return;
            }

            int idSincronizacion = Convert.ToInt32(e.ObtenerClave("IdSincronizacion"));

            BEHistorialSincronizacion sincronizacion = bllIntegracion.TraerHistorialSincronizaciones()
                .Find(s => s.IdSincronizacion == idSincronizacion);

            if (sincronizacion == null)
            {
                pnlDetalle.Visible = false;
                return;
            }

            lblDetalleEstado.Text = Server.HtmlEncode(Traductor.T(sincronizacion.EstadoTexto));
            lblDetalleFecha.Text = sincronizacion.FechaEjecucion.ToString("dd/MM/yyyy HH:mm");
            lblDetalleTipo.Text = Server.HtmlEncode(Traductor.T(sincronizacion.TipoTexto));
            lblDetalleProcesados.Text = sincronizacion.RegistrosProcesados.ToString("N0");
            lblDetalleImportados.Text = sincronizacion.RegistrosImportados.ToString("N0");
            lblDetalleActualizados.Text = sincronizacion.RegistrosActualizados.ToString("N0");
            lblDetalleConError.Text = sincronizacion.RegistrosError.ToString("N0");

            blDetalle.Items.Clear();

            foreach (string linea in sincronizacion.LineasDetalle)
            {
                blDetalle.Items.Add(new ListItem(linea));
            }

            blDetalle.Visible = blDetalle.Items.Count > 0;
            lblSinErrores.Visible = blDetalle.Items.Count == 0;
            pnlDetalle.Visible = true;
        }

        protected void btnCerrarDetalle_Click(object sender, EventArgs e)
        {
            pnlDetalle.Visible = false;
        }

        // Alternativas 2.1 [#ERR030] y 4.1 [#ERR013]: la grilla muestra el mensaje vacio
        private void MostrarHistorial(List<BEHistorialSincronizacion> historial, string mensajeVacio)
        {
            pnlDetalle.Visible = false;
            grillaHistorial.Cargar(historial, mensajeVacio);
            MostrarCantidad();
        }

        private void MostrarCantidad()
        {
            int cantidad = grillaHistorial.CantidadRegistros;

            lblCantidad.Text = cantidad == 1
                ? Traductor.T("historialSincronizaciones.cantidadUno")
                : Traductor.T("historialSincronizaciones.cantidad", cantidad);
        }

        private void CargarOpciones(DropDownList lista, string[] valores, string prefijo)
        {
            lista.Items.Clear();
            lista.Items.Add(new ListItem(Traductor.T("comun.todos"), string.Empty));

            foreach (string valor in valores)
            {
                lista.Items.Add(new ListItem(Traductor.T(prefijo + valor), valor));
            }
        }

        private void TraducirOpciones(DropDownList lista, string prefijo)
        {
            lista.Items[0].Text = Traductor.T("comun.todos");

            for (int i = 1; i < lista.Items.Count; i++)
            {
                lista.Items[i].Text = Traductor.T(prefijo + lista.Items[i].Value);
            }
        }
    }
}
