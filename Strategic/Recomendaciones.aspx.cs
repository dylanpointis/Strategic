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
    // CU-002-005 - Generar Recomendaciones
    public partial class Recomendaciones : PaginaSegura
    {
        private const string ClaveLista = "Recomendaciones.Lista";
        private const string MensajeSinResultados = "comun.sinResultadosFiltros";

        private readonly BLLRecomendacion bllRecomendacion = new BLLRecomendacion();

        // Lista generada al abrir la pantalla. Los filtros trabajan sobre ella
        // sin volver a correr el analisis
        private List<BERecomendacion> RecomendacionesGeneradas
        {
            get { return Session[ClaveLista] as List<BERecomendacion>; }
            set { Session[ClaveLista] = value; }
        }

        // Numero de la recomendacion abierta en el detalle, 0 si no hay ninguna
        private int NumeroSeleccionado
        {
            get { return ViewState["NumeroSeleccionado"] == null ? 0 : Convert.ToInt32(ViewState["NumeroSeleccionado"]); }
            set { ViewState["NumeroSeleccionado"] = value; }
        }

        public override void ActualizarIdioma()
        {
            Title = Traductor.T("recomendaciones.titulo");
            lblTipo.Text = Traductor.T("recomendaciones.tipo");
            lblPrioridad.Text = Traductor.T("recomendaciones.prioridad");
            lblProducto.Text = Traductor.T("monitoreo.producto");
            ddlTipo.Items[0].Text = Traductor.T("comun.todos");
            ddlTipo.Items[1].Text = Traductor.T("recomendaciones.tipo.stock");
            ddlTipo.Items[2].Text = Traductor.T("recomendaciones.tipo.precio");
            ddlTipo.Items[3].Text = Traductor.T("recomendaciones.tipo.ventas");
            ddlPrioridad.Items[0].Text = Traductor.T("comun.todas");
            ddlPrioridad.Items[1].Text = Traductor.T("recomendaciones.prioridad.alta");
            ddlPrioridad.Items[2].Text = Traductor.T("recomendaciones.prioridad.media");
            ddlPrioridad.Items[3].Text = Traductor.T("recomendaciones.prioridad.baja");
            btnFiltrar.Text = Traductor.T("comun.filtrar");
            btnLimpiar.Text = Traductor.T("comun.limpiarFiltros");
            btnGuardar.Text = Traductor.T("recomendaciones.guardar");
            btnCerrarDetalle.Text = Traductor.T("comun.cerrar");
            lnkImportarDatos.Text = Traductor.T("master.menu.importarDatos");

            // El combo de productos se arma desde codigo: en el Init de un
            // postback todavia no se recupero del ViewState
            if (ddlProducto.Items.Count > 0)
            {
                ddlProducto.Items[0].Text = Traductor.T("monitoreo.todosLosProductos");
            }

            // Solo con el idioma cambiado en un postback el panel ya esta
            // visible: se regeneran para que los textos salgan en el idioma nuevo
            if (pnlContenido.Visible)
            {
                Regenerar();
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
                CargarPantalla();
            }
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            Consultar(true);
        }

        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            ddlTipo.SelectedIndex = 0;
            ddlPrioridad.SelectedIndex = 0;
            ddlProducto.SelectedIndex = 0;

            Consultar(false);
        }

        protected void grillaRecomendaciones_AccionSeleccionada(object sender, AccionGrillaEventArgs e)
        {
            int numero;

            if (e.Comando != "VerDetalle" || !int.TryParse(e.ObtenerClave("Numero"), out numero))
            {
                return;
            }

            MostrarDetalle(numero);
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            BERecomendacion recomendacion = Buscar(NumeroSeleccionado);

            if (recomendacion == null)
            {
                OcultarDetalle();
                lblError.Text = Server.HtmlEncode(Traductor.T("recomendaciones.noEncontrada"));
                return;
            }

            try
            {
                recomendacion.NombreUsuario = UsuarioEnSesion.NombreUsuario;
                bllRecomendacion.RegistrarRecomendacion(recomendacion);

                // La grilla se vuelve a cargar para que muestre el nuevo estado
                Consultar(false);
                MostrarDetalle(recomendacion.Numero);

                lblExito.Text = Server.HtmlEncode(Traductor.T("recomendaciones.guardadaOk"));
            }
            catch (Exception ex)
            {
                lblErrorDetalle.Text = Server.HtmlEncode(ex.Message);
            }
        }

        protected void btnCerrarDetalle_Click(object sender, EventArgs e)
        {
            OcultarDetalle();
        }

        private void ConfigurarGrilla()
        {
            grillaRecomendaciones.ClavePrimaria = "Numero";
            grillaRecomendaciones.FilasPorPagina = 10;
            grillaRecomendaciones.MostrarSelectorFilas = true;

            grillaRecomendaciones.AgregarColumnaTraducida("TipoTexto", "recomendaciones.tipo");
            grillaRecomendaciones.AgregarColumna("Titulo", "recomendaciones.tituloColumna", "celda-usuario");
            grillaRecomendaciones.AgregarColumna("NombreProducto", "monitoreo.producto");
            grillaRecomendaciones.AgregarColumnaTraducida("PrioridadTexto", "recomendaciones.prioridad");
            grillaRecomendaciones.AgregarColumnaTraducida("GuardadaTexto", "comun.estado");
            grillaRecomendaciones.AgregarColumnaAccion("comun.detalle", "VerDetalle", "recomendaciones.verDetalle");
        }

        private void CargarPantalla()
        {
            if (!Generar())
            {
                return;
            }

            CargarComboProductos();
            Consultar(false);
        }

        // Corre el analisis. Devuelve false si no se pudo y deja el aviso en pantalla
        private bool Generar()
        {
            try
            {
                RecomendacionesGeneradas = bllRecomendacion.GenerarRecomendaciones();

                pnlSinDatos.Visible = false;
                pnlContenido.Visible = true;

                return true;
            }
            catch (DatosInsuficientesException ex)
            {
                // Alternativa 2.1 del CU: se sugiere sincronizar
                RecomendacionesGeneradas = null;
                pnlContenido.Visible = false;
                pnlSinDatos.Visible = true;
                lblSinDatos.Text = Server.HtmlEncode(ex.Message);
                lnkImportarDatos.Visible = TienePermiso("ImportarDatos");

                return false;
            }
            catch (Exception ex)
            {
                RecomendacionesGeneradas = null;
                pnlContenido.Visible = false;
                lblErrorAnalisis.Text = Server.HtmlEncode(ex.Message);

                return false;
            }
        }

        // Despues de cambiar el idioma: se vuelve a generar sin perder las que
        // ya se guardaron, los filtros elegidos ni el detalle abierto
        private void Regenerar()
        {
            List<BERecomendacion> anteriores = RecomendacionesGeneradas;

            if (!Generar())
            {
                return;
            }

            if (anteriores != null)
            {
                foreach (BERecomendacion recomendacion in RecomendacionesGeneradas)
                {
                    recomendacion.Guardada = anteriores.Exists(a => a.Guardada
                        && a.IdProducto == recomendacion.IdProducto
                        && a.Regla == recomendacion.Regla);
                }
            }

            Consultar(false);

            if (pnlDetalle.Visible)
            {
                MostrarDetalle(NumeroSeleccionado);
            }
        }

        private void Consultar(bool vieneDeFiltros)
        {
            List<BERecomendacion> recomendaciones = RecomendacionesGeneradas ?? new List<BERecomendacion>();

            try
            {
                List<BERecomendacion> filtradas = bllRecomendacion.FiltrarRecomendaciones(
                    recomendaciones,
                    ddlTipo.SelectedValue,
                    ddlPrioridad.SelectedValue,
                    ObtenerId(ddlProducto.SelectedValue));

                // Sin ninguna recomendacion el aviso es otro: no hay nada que filtrar
                MostrarRecomendaciones(filtradas, recomendaciones.Count == 0
                    ? "recomendaciones.sinRecomendaciones"
                    : MensajeSinResultados);

                // Alternativa 4.1 del CU [#ERR013]
                if (vieneDeFiltros && filtradas.Count == 0 && recomendaciones.Count > 0)
                {
                    lblAvisoFiltros.Text = Server.HtmlEncode(Traductor.T(MensajeSinResultados));
                }
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(ex.Message);
                MostrarRecomendaciones(new List<BERecomendacion>(), MensajeSinResultados);
            }
        }

        private void MostrarRecomendaciones(List<BERecomendacion> recomendaciones, string mensajeVacio)
        {
            grillaRecomendaciones.Cargar(recomendaciones, mensajeVacio);

            lblTotal.Text = recomendaciones.Count.ToString("N0");
            lblAlta.Text = bllRecomendacion.ContarPorPrioridad(recomendaciones, BERecomendacion.PrioridadAlta).ToString("N0");
            lblMedia.Text = bllRecomendacion.ContarPorPrioridad(recomendaciones, BERecomendacion.PrioridadMedia).ToString("N0");
            lblBaja.Text = bllRecomendacion.ContarPorPrioridad(recomendaciones, BERecomendacion.PrioridadBaja).ToString("N0");

            lblCantidad.Text = recomendaciones.Count == 1
                ? Traductor.T("recomendaciones.cantidadUno")
                : Traductor.T("recomendaciones.cantidad", recomendaciones.Count);
        }

        private void MostrarDetalle(int numero)
        {
            BERecomendacion recomendacion = Buscar(numero);

            if (recomendacion == null)
            {
                OcultarDetalle();
                lblError.Text = Server.HtmlEncode(Traductor.T("recomendaciones.noEncontrada"));
                return;
            }

            NumeroSeleccionado = numero;

            lblDetalleTitulo.Text = Server.HtmlEncode(recomendacion.Titulo);
            lblDetalleEstado.Text = Server.HtmlEncode(Traductor.T(recomendacion.GuardadaTexto));
            lblDetalleProducto.Text = Server.HtmlEncode(string.Format("{0} - {1}", recomendacion.CodigoProducto, recomendacion.NombreProducto));
            lblDetalleTipo.Text = Server.HtmlEncode(Traductor.T(recomendacion.TipoTexto));
            lblDetallePrioridad.Text = Server.HtmlEncode(Traductor.T(recomendacion.PrioridadTexto));
            lblDetalleDescripcion.Text = Server.HtmlEncode(recomendacion.Descripcion);
            lblDetalleJustificacion.Text = Server.HtmlEncode(recomendacion.Justificacion);
            lblDetalleAccion.Text = Server.HtmlEncode(recomendacion.AccionSugerida);

            // Una recomendacion ya guardada no se puede volver a guardar
            btnGuardar.Enabled = !recomendacion.Guardada;
            btnGuardar.Text = recomendacion.Guardada
                ? Traductor.T("recomendaciones.guardada")
                : Traductor.T("recomendaciones.guardar");

            pnlDetalle.Visible = true;
        }

        private void OcultarDetalle()
        {
            NumeroSeleccionado = 0;
            pnlDetalle.Visible = false;
        }

        private BERecomendacion Buscar(int numero)
        {
            List<BERecomendacion> recomendaciones = RecomendacionesGeneradas;

            return recomendaciones == null ? null : recomendaciones.Find(r => r.Numero == numero);
        }

        // Solo se ofrecen los productos que tienen alguna recomendacion
        private void CargarComboProductos()
        {
            SortedDictionary<string, int> productos = new SortedDictionary<string, int>();

            foreach (BERecomendacion recomendacion in RecomendacionesGeneradas)
            {
                string texto = string.Format("{0} - {1}", recomendacion.CodigoProducto, recomendacion.NombreProducto);

                if (!productos.ContainsKey(texto))
                {
                    productos.Add(texto, recomendacion.IdProducto);
                }
            }

            ddlProducto.Items.Clear();
            ddlProducto.Items.Add(new ListItem(Traductor.T("monitoreo.todosLosProductos"), string.Empty));

            foreach (KeyValuePair<string, int> producto in productos)
            {
                ddlProducto.Items.Add(new ListItem(producto.Key, producto.Value.ToString()));
            }
        }

        private int? ObtenerId(string valor)
        {
            int id;

            return int.TryParse(valor, out id) ? (int?)id : null;
        }
    }
}
