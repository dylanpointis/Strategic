using BE;
using BLL;
using Services;
using Strategic.Componentes;
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Strategic
{
    // CU-007-037 - Gestionar Monitoreo de publicaciones competidoras
    public partial class MonitoreoPublicacionesCompetidoras : PaginaBase
    {
        private const string MensajeSinResultados = "comun.sinResultadosFiltros";

        private readonly BLLCompetencia bllCompetencia = new BLLCompetencia();
        private readonly BLLProducto bllProducto = new BLLProducto();

        protected void Page_Init(object sender, EventArgs e)
        {
            // Las columnas se arman en el Init para que la grilla pueda
            // reconstruirse en cada postback a partir de su ViewState
            ConfigurarGrilla();
        }

        public override void ActualizarIdioma()
        {
            Title = Traductor.T("monitoreo.titulo");
            lblEstado.Text = Traductor.T("comun.estado");
            ddlEstado.Items[0].Text = Traductor.T("comun.todos");
            ddlEstado.Items[1].Text = Traductor.T("publicacion.estado.activa");
            ddlEstado.Items[2].Text = Traductor.T("publicacion.estado.pausada");
            ddlEstado.Items[3].Text = Traductor.T("publicacion.estado.finalizada");
            ddlEstado.Items[4].Text = Traductor.T("publicacion.estado.noEncontrada");
            ddlEstado.Items[5].Text = Traductor.T("publicacion.estado.error");
            btnFiltrar.Text = Traductor.T("comun.filtrar");
            btnLimpiar.Text = Traductor.T("comun.limpiarFiltros");
            MostrarCantidad();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Solo WebMaster y Administrador, igual que el resto de la gestion de competencia
            BEUsuario usuario = SessionManager.UsuarioActual;

            if (usuario == null || (usuario.CodRol != 1 && usuario.CodRol != 2))
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                CargarPantalla();
            }
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            Consultar(true);
        }

        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            selectorProducto.Limpiar();
            selectorCompetidor.Limpiar();
            ddlEstado.SelectedIndex = 0;

            Consultar(false);
        }

        protected void grillaPublicaciones_AccionSeleccionada(object sender, AccionGrillaEventArgs e)
        {
            string idProductoCompetencia = e.ObtenerClave("IdProductoCompetencia");

            if (string.IsNullOrEmpty(idProductoCompetencia))
            {
                return;
            }

            if (e.Comando == "PausarReactivar" || e.Comando == "Desactivar")
            {
                Response.Redirect(string.Format(
                    "~/GestionarPublicacionCompetencia.aspx?id={0}&accion={1}",
                    Server.UrlEncode(idProductoCompetencia),
                    Server.UrlEncode(e.Comando)));
            }
        }

        private void ConfigurarGrilla()
        {
            grillaPublicaciones.ClavePrimaria = "IdProductoCompetencia";
            grillaPublicaciones.FilasPorPagina = 10;
            grillaPublicaciones.MostrarSelectorFilas = true;

            grillaPublicaciones.AgregarColumna("NombreProducto", "mapearProductos.productoPropio", "celda-usuario");
            grillaPublicaciones.AgregarColumna("NombreCompetidor", "mapearProductos.competidor");
            grillaPublicaciones.AgregarColumna("Url", "monitoreo.url");
            grillaPublicaciones.AgregarColumna("UltimoPrecio", "monitoreo.ultimoPrecio", "celda-numero", "${0:N2}");
            grillaPublicaciones.AgregarColumna("FechaUltimaVerificacion", "monitoreo.ultimaVerificacion", "celda-fecha", "{0:dd/MM/yyyy HH:mm}");
            grillaPublicaciones.AgregarColumnaTraducida("EstadoTexto", "comun.estado");

            // El texto sale de la fila: una publicacion activa se pausa y una
            // pausada (o con error) se reactiva
            grillaPublicaciones.AgregarColumnaAccion("monitoreo.monitoreo", "PausarReactivar", "comun.accion.pausar", "AccionPausarReactivar");
            grillaPublicaciones.AgregarColumnaAccion("monitoreo.desactivar", "Desactivar", "monitoreo.desactivar");
        }

        
        /// Alternativa 2.1 del CU: si no hay ningun mapeo registrado se muestra
        /// el estado vacio con el acceso directo a CU-007-036, sin filtros.
        
        private void CargarPantalla()
        {
            List<BEPublicacionCompetencia> publicaciones = bllCompetencia.TraerListaPublicacionesMonitoreo();

            if (publicaciones.Count == 0)
            {
                pnlSinDatos.Visible = true;
                pnlContenido.Visible = false;
                return;
            }

            pnlSinDatos.Visible = false;
            pnlContenido.Visible = true;

            CargarCombos();
            MostrarMensajeDeOtraPantalla();
            MostrarPublicaciones(publicaciones, MensajeSinResultados);
        }

        private void Consultar(bool vieneDeFiltros)
        {
            try
            {
                List<BEPublicacionCompetencia> publicaciones = bllCompetencia.FiltrarPublicacionesMonitoreo(
                    selectorProducto.IdProducto,
                    selectorCompetidor.IdCompetencia,
                    ddlEstado.SelectedValue);

                MostrarPublicaciones(publicaciones, MensajeSinResultados);

                if (vieneDeFiltros && publicaciones.Count == 0)
                {
                    lblAvisoFiltros.Text = Traductor.T(MensajeSinResultados);
                }
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(ex.Message);
                MostrarPublicaciones(new List<BEPublicacionCompetencia>(), MensajeSinResultados);
            }
        }

        private void MostrarPublicaciones(List<BEPublicacionCompetencia> publicaciones, string mensajeVacio)
        {
            grillaPublicaciones.Cargar(publicaciones, mensajeVacio);
            MostrarCantidad();
        }

        private void MostrarCantidad()
        {
            int cantidad = grillaPublicaciones.CantidadRegistros;

            lblCantidad.Text = cantidad == 1
                ? Traductor.T("monitoreo.cantidadUno")
                : Traductor.T("monitoreo.cantidad", cantidad);
        }

        
        /// La confirmación de pausar, reactivar o desactivar vuelve a este
        /// listado con un mensaje para mostrar.
        
        private void MostrarMensajeDeOtraPantalla()
        {
            string mensaje = Convert.ToString(Session["MensajePublicaciones"]);

            if (string.IsNullOrEmpty(mensaje))
            {
                return;
            }

            Session.Remove("MensajePublicaciones");
            lblExito.Text = Server.HtmlEncode(mensaje);
        }

        private void CargarCombos()
        {
            selectorProducto.Cargar(bllProducto.TraerListaProductos());

            // Se listan todos, no solo los activos: una publicacion mapeada
            // puede seguir apareciendo aunque su competidor haya sido dado de baja
            selectorCompetidor.Cargar(bllCompetencia.FiltrarCompetidores(null, null, null));
        }
    }
}
