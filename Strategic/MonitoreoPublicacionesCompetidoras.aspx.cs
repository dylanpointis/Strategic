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
    public partial class MonitoreoPublicacionesCompetidoras : Page
    {
        private const string MensajeSinResultados = "No se encontraron datos con los filtros ingresados";

        private readonly BLLCompetencia bllCompetencia = new BLLCompetencia();
        private readonly BLLProducto bllProducto = new BLLProducto();

        protected void Page_Init(object sender, EventArgs e)
        {
            // Las columnas se arman en el Init para que la grilla pueda
            // reconstruirse en cada postback a partir de su ViewState
            ConfigurarGrilla();
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
            ddlProducto.SelectedIndex = 0;
            ddlCompetidor.SelectedIndex = 0;
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

            grillaPublicaciones.AgregarColumna("NombreProducto", "Producto propio", "celda-usuario");
            grillaPublicaciones.AgregarColumna("NombreCompetidor", "Competidor");
            grillaPublicaciones.AgregarColumna("Url", "URL");
            grillaPublicaciones.AgregarColumna("UltimoPrecio", "Último precio", "celda-numero", "${0:N2}");
            grillaPublicaciones.AgregarColumna("FechaUltimaVerificacion", "Última verificación", "celda-fecha", "{0:dd/MM/yyyy HH:mm}");
            grillaPublicaciones.AgregarColumna("Estado", "Estado");

            // El texto sale de la fila: una publicacion activa se pausa y una
            // pausada (o con error) se reactiva
            grillaPublicaciones.AgregarColumnaAccion("Monitoreo", "PausarReactivar", "Pausar", "AccionPausarReactivar");
            grillaPublicaciones.AgregarColumnaAccion("Desactivar", "Desactivar", "Desactivar");
        }

        /// <summary>
        /// Alternativa 2.1 del CU: si no hay ningun mapeo registrado se muestra
        /// el estado vacio con el acceso directo a CU-007-036, sin filtros.
        /// </summary>
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
                    ObtenerId(ddlProducto.SelectedValue),
                    ObtenerId(ddlCompetidor.SelectedValue),
                    ddlEstado.SelectedValue);

                MostrarPublicaciones(publicaciones, MensajeSinResultados);

                if (vieneDeFiltros && publicaciones.Count == 0)
                {
                    lblAvisoFiltros.Text = MensajeSinResultados;
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

            lblCantidad.Text = publicaciones.Count == 1
                ? "1 publicación"
                : string.Format("{0} publicaciones", publicaciones.Count);
        }

        /// <summary>
        /// La confirmación de pausar, reactivar o desactivar vuelve a este
        /// listado con un mensaje para mostrar.
        /// </summary>
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
            ddlProducto.Items.Clear();
            ddlProducto.Items.Add(new ListItem("Todos los productos", string.Empty));

            foreach (BEProducto producto in bllProducto.TraerListaProductos())
            {
                ddlProducto.Items.Add(new ListItem(
                    string.Format("{0} - {1}", producto.Codigo, producto.Nombre),
                    producto.IdProducto.ToString()));
            }

            ddlCompetidor.Items.Clear();
            ddlCompetidor.Items.Add(new ListItem("Todos los competidores", string.Empty));

            // Se listan todos, no solo los activos: una publicacion mapeada
            // puede seguir apareciendo aunque su competidor haya sido dado de baja
            foreach (BECompetencia competidor in bllCompetencia.FiltrarCompetidores(null, null, null))
            {
                ddlCompetidor.Items.Add(new ListItem(competidor.Nombre, competidor.IdCompetencia.ToString()));
            }
        }

        private int? ObtenerId(string valor)
        {
            int id;

            return int.TryParse(valor, out id) ? (int?)id : null;
        }
    }
}
