using BE;
using BLL;
using Services;
using Strategic.Componentes;
using Strategic.Seguridad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

namespace Strategic
{
    // CU-003-009 - Consultar Automatizaciones
    // Pausar y modificar se disparan desde este listado. El boton Ejecutar
    // ahora corre las automatizaciones activas sobre los productos
    public partial class ConsultarAutomatizaciones : PaginaSegura
    {
        private const string MensajeSinAutomatizaciones = "consultarAutomatizaciones.sinAutomatizaciones";
        private const string MensajeSinResultados = "comun.sinResultadosFiltros";

        private readonly BLLAutomatizacion bllAutomatizacion = new BLLAutomatizacion();
        private readonly BLLEjecucionAutomatizaciones bllEjecucion = new BLLEjecucionAutomatizaciones();

        protected void Page_Init(object sender, EventArgs e)
        {
            grillaAutomatizaciones.ClavePrimaria = "IdAutomatizacion";
            grillaAutomatizaciones.FilasPorPagina = 10;
            grillaAutomatizaciones.MostrarSelectorFilas = false;

            grillaAutomatizaciones.AgregarColumna("Nombre", "comun.nombre", "celda-usuario");
            grillaAutomatizaciones.AgregarColumna("Tipo", "automatizacion.tipo");
            grillaAutomatizaciones.AgregarColumna("Condicion", "automatizacion.condicion");
            grillaAutomatizaciones.AgregarColumnaTraducida("EstadoTexto", "comun.estado");
            grillaAutomatizaciones.AgregarColumna("FechaUltimaEjecucion", "automatizacion.ultimaEjecucion", "celda-fecha", "{0:dd/MM/yyyy HH:mm}");

            // Cada accion lleva a una pantalla con su propio permiso
            if (TienePermiso("PausarReactivarAutomatizacion"))
            {
                grillaAutomatizaciones.AgregarColumnaAccion("comun.cambiarEstado", "CambiarEstado", "comun.cambiarEstado", "AccionEstado");
            }

            if (TienePermiso("ModificarParametroAutomatizacion"))
            {
                grillaAutomatizaciones.AgregarColumnaAccion("automatizacion.parametros", "Modificar", "comun.modificar");
            }

            btnEjecutar.Visible = TienePermiso("ModificarParametroAutomatizacion");
        }

        public override void ActualizarIdioma()
        {
            Title = Traductor.T("consultarAutomatizaciones.titulo");
            lblTipo.Text = Traductor.T("automatizacion.tipo");
            lblEstado.Text = Traductor.T("comun.estado");
            ddlTipo.Items[0].Text = Traductor.T("comun.todos");
            ddlEstado.Items[0].Text = Traductor.T("comun.todos");
            ddlEstado.Items[1].Text = Traductor.T("automatizacion.estado.Activa");
            ddlEstado.Items[2].Text = Traductor.T("automatizacion.estado.Pausada");
            btnFiltrar.Text = Traductor.T("comun.filtrar");
            btnLimpiar.Text = Traductor.T("comun.limpiarFiltros");
            btnEjecutar.Text = Traductor.T("consultarAutomatizaciones.ejecutar");
            btnConfirmarEjecucion.Text = Traductor.T("consultarAutomatizaciones.ejecutar");
            btnCancelarEjecucion.Text = Traductor.T("comun.cancelar");
            MostrarCantidad();

            // Solo con el idioma cambiado en un postback la grilla ya tiene
            // datos: se recarga para que la condicion salga en el idioma nuevo
            if (grillaAutomatizaciones.CantidadRegistros > 0)
            {
                Consultar(false);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                MostrarMensajeDeOtraPantalla();
                CargarTipos();
                Consultar(false);
            }
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            Consultar(true);
        }

        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            ddlTipo.SelectedIndex = 0;
            ddlEstado.SelectedIndex = 0;

            Consultar(false);
        }

        protected void btnEjecutar_Click(object sender, EventArgs e)
        {
            pnlConfirmarEjecucion.Visible = true;
        }

        protected void btnCancelarEjecucion_Click(object sender, EventArgs e)
        {
            pnlConfirmarEjecucion.Visible = false;
        }

        protected void btnConfirmarEjecucion_Click(object sender, EventArgs e)
        {
            pnlConfirmarEjecucion.Visible = false;

            try
            {
                List<BEHistorialAutomatizacion> resultados = bllEjecucion.EjecutarAutomatizaciones(SessionManager.UsuarioActual.NombreUsuario);

                int conError = resultados.Count(r => r.Estado == BEHistorialAutomatizacion.EstadoConError);

                lblExito.Text = Server.HtmlEncode(resultados.Count == 0
                    ? Traductor.T("consultarAutomatizaciones.ejecucionSinAcciones")
                    : Traductor.T("consultarAutomatizaciones.ejecucionFinalizada", resultados.Count, conError));
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(ex.Message);
            }

            Consultar(false);
        }

        // Pasos 5 y 6: cada accion redirige a su caso de uso
        protected void grillaAutomatizaciones_AccionSeleccionada(object sender, AccionGrillaEventArgs e)
        {
            string idAutomatizacion = e.ObtenerClave("IdAutomatizacion");

            if (e.Comando == "CambiarEstado")
            {
                Response.Redirect("~/PausarReactivarAutomatizacion.aspx?id=" + Server.UrlEncode(idAutomatizacion));
            }

            if (e.Comando == "Modificar")
            {
                Response.Redirect("~/ModificarParametroAutomatizacion.aspx?id=" + Server.UrlEncode(idAutomatizacion));
            }
        }

        private void CargarTipos()
        {
            try
            {
                foreach (BEAutomatizacion automatizacion in bllAutomatizacion.TraerListaAutomatizaciones())
                {
                    ddlTipo.Items.Add(new ListItem(automatizacion.Nombre, automatizacion.Tipo));
                }
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(ex.Message);
            }
        }

        private void Consultar(bool vieneDeFiltros)
        {
            try
            {
                List<BEAutomatizacion> automatizaciones = bllAutomatizacion.FiltrarAutomatizaciones(
                    bllAutomatizacion.TraerListaAutomatizaciones(),
                    ddlTipo.SelectedValue,
                    ddlEstado.SelectedValue);

                grillaAutomatizaciones.Cargar(automatizaciones, vieneDeFiltros ? MensajeSinResultados : MensajeSinAutomatizaciones);
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(ex.Message);
                grillaAutomatizaciones.Cargar(new List<BEAutomatizacion>(), MensajeSinResultados);
            }

            MostrarCantidad();
        }

        private void MostrarCantidad()
        {
            int cantidad = grillaAutomatizaciones.CantidadRegistros;

            lblCantidad.Text = cantidad == 1
                ? Traductor.T("consultarAutomatizaciones.cantidadUno")
                : Traductor.T("consultarAutomatizaciones.cantidad", cantidad);
        }

        private void MostrarMensajeDeOtraPantalla()
        {
            string mensaje = Convert.ToString(Session["MensajeAutomatizaciones"]);

            if (string.IsNullOrEmpty(mensaje))
            {
                return;
            }

            Session.Remove("MensajeAutomatizaciones");
            lblExito.Text = Server.HtmlEncode(mensaje);
        }
    }
}
