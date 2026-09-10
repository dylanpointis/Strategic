using BE;
using BLL;
using Services;
using Strategic.Componentes;
using System;
using System.Collections.Generic;
using System.Web.UI;

namespace Strategic
{
    // Consultar Competidores
    // La baja y la reactivacion se disparan desde este listado
    public partial class ConsultarCompetidores : Page
    {
        private const string MensajeSinCompetidores = "No existen competidores registrados";
        private const string MensajeSinResultados = "No se encontraron datos con los filtros ingresados";

        private readonly BLLCompetencia bllCompetencia = new BLLCompetencia();

        protected void Page_Init(object sender, EventArgs e)
        {
            // Las columnas se arman en el Init para que la grilla pueda
            // reconstruirse en cada postback a partir de su ViewState
            ConfigurarGrilla();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Solo WebMaster y Administrador, igual que la gestion de usuarios
            BEUsuario usuario = SessionManager.UsuarioActual;

            if (usuario == null || (usuario.CodRol != 1 && usuario.CodRol != 2))
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

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

            grillaCompetidores.AgregarColumna("Nombre", "Nombre", "celda-usuario");
            grillaCompetidores.AgregarColumna("Marketplace", "Marketplace");
            grillaCompetidores.AgregarColumna("Descripcion", "Descripción");
            grillaCompetidores.AgregarColumna("Estado", "Estado");
            grillaCompetidores.AgregarColumnaAccion("Modificar", "Modificar", "Modificar");

            // El texto sale de la fila: un competidor activo se da de baja y
            // uno inactivo se reactiva
            grillaCompetidores.AgregarColumnaAccion("Estado", "CambiarEstado", "Cambiar estado", "AccionEstado");
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
                    lblAvisoFiltros.Text = MensajeSinResultados;
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

            lblCantidad.Text = competidores.Count == 1
                ? "1 competidor"
                : string.Format("{0} competidores", competidores.Count);
        }

        /// <summary>
        /// El alta, la baja y la modificación vuelven a este listado con un
        /// mensaje para mostrar, porque el resultado de la operación se
        /// confirma acá.
        /// </summary>
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
