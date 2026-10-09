using BE;
using BLL;
using Services;
using Strategic.Componentes;
using Strategic.Seguridad;
using System;
using System.Collections.Generic;
using System.Web.UI;

namespace Strategic
{
    // Consultar Competidores
    // La baja y la reactivacion se disparan desde este listado
    public partial class ConsultarCompetidores : PaginaSegura
    {
        private const string MensajeSinCompetidores = "consultarCompetidores.sinCompetidores";
        private const string MensajeSinResultados = "comun.sinResultadosFiltros";

        private readonly BLLCompetencia bllCompetencia = new BLLCompetencia();

        protected void Page_Init(object sender, EventArgs e)
        {
            // Las columnas se arman en el Init para que la grilla pueda
            // reconstruirse en cada postback a partir de su ViewState
            ConfigurarGrilla();
        }

        public override void ActualizarIdioma()
        {
            Title = Traductor.T("consultarCompetidores.titulo");
            lblTexto.Text = Traductor.T("comun.nombre");
            txtTexto.Attributes["placeholder"] = Traductor.T("comun.todos");
            lblMarketplace.Text = Traductor.T("competidor.marketplace");
            txtMarketplace.Attributes["placeholder"] = Traductor.T("comun.todos");
            lblEstado.Text = Traductor.T("comun.estado");
            ddlEstado.Items[0].Text = Traductor.T("comun.activos");
            ddlEstado.Items[1].Text = Traductor.T("comun.inactivos");
            ddlEstado.Items[2].Text = Traductor.T("comun.todos");
            btnFiltrar.Text = Traductor.T("comun.filtrar");
            btnLimpiar.Text = Traductor.T("comun.limpiarFiltros");
            btnNuevo.Text = Traductor.T("consultarCompetidores.nuevo");
            MostrarCantidad();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                MostrarMensajeDeOtraPantalla();
                Consultar(false);
            }
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            Consultar(true);
        }

        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            txtTexto.Text = string.Empty;
            txtMarketplace.Text = string.Empty;
            ddlEstado.SelectedIndex = 0;

            Consultar(false);
        }

        protected void btnNuevo_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/AltaCompetidor.aspx");
        }

        protected void grillaCompetidores_AccionSeleccionada(object sender, AccionGrillaEventArgs e)
        {
            string idCompetencia = e.ObtenerClave("IdCompetencia");

            if (string.IsNullOrEmpty(idCompetencia))
            {
                return;
            }

            if (e.Comando == "Modificar")
            {
                Response.Redirect("~/ModificarCompetidor.aspx?id=" + Server.UrlEncode(idCompetencia));
                return;
            }

            if (e.Comando == "CambiarEstado")
            {
                Response.Redirect("~/BajaCompetidor.aspx?id=" + Server.UrlEncode(idCompetencia));
            }
        }

        private void ConfigurarGrilla()
        {
            grillaCompetidores.ClavePrimaria = "IdCompetencia";
            grillaCompetidores.FilasPorPagina = 10;
            grillaCompetidores.MostrarSelectorFilas = true;

            grillaCompetidores.AgregarColumna("Nombre", "comun.nombre", "celda-usuario");
            grillaCompetidores.AgregarColumna("Marketplace", "competidor.marketplace");
            grillaCompetidores.AgregarColumna("Descripcion", "familia.descripcion");
            grillaCompetidores.AgregarColumnaTraducida("EstadoTexto", "comun.estado");

            // Cada accion lleva a una pantalla con su propio permiso: la columna
            // solo se arma si el rol la alcanza
            if (TienePermiso("ModificarCompetidor"))
            {
                grillaCompetidores.AgregarColumnaAccion("comun.modificar", "Modificar", "comun.modificar");
            }

            // El texto sale de la fila: un competidor activo se da de baja y
            // uno inactivo se reactiva
            if (TienePermiso("BajaCompetidor"))
            {
                grillaCompetidores.AgregarColumnaAccion("comun.estado", "CambiarEstado", "comun.cambiarEstado", "AccionEstado");
            }

            btnNuevo.Visible = TienePermiso("AltaCompetidor");
        }

        private void Consultar(bool vieneDeFiltros)
        {
            try
            {
                List<BECompetencia> competidores = vieneDeFiltros || HayFiltrosAplicados()
                    ? bllCompetencia.FiltrarCompetidores(txtTexto.Text, txtMarketplace.Text, ObtenerEstadoElegido())
                    : bllCompetencia.TraerListaCompetidores();

                MostrarCompetidores(competidores, vieneDeFiltros ? MensajeSinResultados : MensajeSinCompetidores);

                if (vieneDeFiltros && competidores.Count == 0)
                {
                    lblAvisoFiltros.Text = Traductor.T(MensajeSinResultados);
                }
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(ex.Message);
                MostrarCompetidores(new List<BECompetencia>(), MensajeSinResultados);
            }
        }

        private void MostrarCompetidores(List<BECompetencia> competidores, string mensajeVacio)
        {
            grillaCompetidores.Cargar(competidores, mensajeVacio);
            MostrarCantidad();
        }

        private void MostrarCantidad()
        {
            int cantidad = grillaCompetidores.CantidadRegistros;

            lblCantidad.Text = cantidad == 1
                ? Traductor.T("consultarCompetidores.cantidadUno")
                : Traductor.T("consultarCompetidores.cantidad", cantidad);
        }

        
        /// El alta, la baja y la modificación vuelven a este listado con un
        /// mensaje para mostrar, porque el resultado de la operación se
        /// confirma acá.
        
        private void MostrarMensajeDeOtraPantalla()
        {
            string mensaje = Convert.ToString(Session["MensajeCompetidores"]);

            if (string.IsNullOrEmpty(mensaje))
            {
                return;
            }

            Session.Remove("MensajeCompetidores");
            lblExito.Text = Server.HtmlEncode(mensaje);
        }

        private bool HayFiltrosAplicados()
        {
            return !string.IsNullOrWhiteSpace(txtTexto.Text)
                || !string.IsNullOrWhiteSpace(txtMarketplace.Text)
                || ObtenerEstadoElegido().HasValue;
        }

        private bool? ObtenerEstadoElegido()
        {
            // El listado arranca mostrando los competidores activos
            if (string.IsNullOrEmpty(ddlEstado.SelectedValue))
            {
                return null;
            }

            return ddlEstado.SelectedValue == "1";
        }
    }
}
