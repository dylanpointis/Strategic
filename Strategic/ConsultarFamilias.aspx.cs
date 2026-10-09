using BE;
using BE.Composite;
using BLL;
using Services;
using Strategic.Componentes;
using Strategic.Seguridad;
using System;
using System.Collections.Generic;
using System.Web.UI;

namespace Strategic
{
    // CU-005-025 - Consultar Familias
    // La baja y la reactivacion del CU-005-027 se disparan desde el detalle
    public partial class ConsultarFamilias : PaginaSegura
    {
        private const string MensajeSinFamilias = "consultarFamilias.sinFamilias";
        private const string MensajeSinResultados = "comun.sinResultadosFiltros";

        private readonly BLLPermiso bllPermiso = new BLLPermiso();

        private int FamiliaSeleccionada
        {
            get { return ViewState["FamiliaSeleccionada"] == null ? 0 : Convert.ToInt32(ViewState["FamiliaSeleccionada"]); }
            set { ViewState["FamiliaSeleccionada"] = value; }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            // Las columnas se arman en el Init para que la grilla pueda
            // reconstruirse en cada postback a partir de su ViewState
            ConfigurarGrilla();
        }

        public override void ActualizarIdioma()
        {
            Title = Traductor.T("consultarFamilias.titulo");
            lblNombre.Text = Traductor.T("comun.nombre");
            txtNombre.Attributes["placeholder"] = Traductor.T("comun.todas");
            lblEstado.Text = Traductor.T("comun.estado");
            ddlEstado.Items[0].Text = Traductor.T("comun.activas");
            ddlEstado.Items[1].Text = Traductor.T("comun.inactivas");
            ddlEstado.Items[2].Text = Traductor.T("comun.todas");
            btnFiltrar.Text = Traductor.T("comun.filtrar");
            btnLimpiar.Text = Traductor.T("comun.limpiarFiltros");
            btnNuevo.Text = Traductor.T("consultarFamilias.nueva");
            btnModificar.Text = Traductor.T("comun.modificar");
            btnCerrarDetalle.Text = Traductor.T("comun.cerrar");
            MostrarCantidad();

            // El detalle abierto se conoce recien al recuperar el ViewState
            if (FamiliaSeleccionada > 0)
            {
                MostrarDetalle(FamiliaSeleccionada);
            }
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
            OcultarDetalle();
            Consultar(true);
        }

        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            txtNombre.Text = string.Empty;
            ddlEstado.SelectedIndex = 0;

            OcultarDetalle();
            Consultar(false);
        }

        protected void btnNuevo_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/AltaFamilia.aspx");
        }

        protected void grillaFamilias_AccionSeleccionada(object sender, AccionGrillaEventArgs e)
        {
            if (e.Comando != "VerDetalle")
            {
                return;
            }

            int codPermiso;

            if (int.TryParse(e.ObtenerClave("CodPermiso"), out codPermiso))
            {
                MostrarDetalle(codPermiso);
            }
        }

        protected void btnModificar_Click(object sender, EventArgs e)
        {
            if (FamiliaSeleccionada > 0)
            {
                Response.Redirect("~/ModificarFamilia.aspx?familia=" + FamiliaSeleccionada);
            }
        }

        protected void btnCambiarEstado_Click(object sender, EventArgs e)
        {
            if (FamiliaSeleccionada > 0)
            {
                Response.Redirect("~/BajaFamilia.aspx?familia=" + FamiliaSeleccionada);
            }
        }

        protected void btnCerrarDetalle_Click(object sender, EventArgs e)
        {
            OcultarDetalle();
        }

        private void ConfigurarGrilla()
        {
            grillaFamilias.ClavePrimaria = "CodPermiso";
            grillaFamilias.FilasPorPagina = 10;
            grillaFamilias.MostrarSelectorFilas = true;

            grillaFamilias.AgregarColumna("Nombre", "consultarFamilias.familia");
            grillaFamilias.AgregarColumna("Descripcion", "familia.descripcion");
            grillaFamilias.AgregarColumna("CantidadComponentesAsignados", "consultarFamilias.elementos", "celda-numero");
            grillaFamilias.AgregarColumnaTraducida("EstadoTexto", "comun.estado");
            grillaFamilias.AgregarColumnaAccion("comun.detalle", "VerDetalle", "consultarFamilias.verComposicion");

            btnNuevo.Visible = TienePermiso("AltaFamilia");
        }

        private void Consultar(bool vieneDeFiltros)
        {
            try
            {
                List<BEFamilia> familias = bllPermiso.FiltrarFamilias(txtNombre.Text, ObtenerEstadoElegido());

                grillaFamilias.Cargar(familias, vieneDeFiltros ? MensajeSinResultados : MensajeSinFamilias);
                MostrarCantidad();

                if (vieneDeFiltros && familias.Count == 0)
                {
                    lblAvisoFiltros.Text = Traductor.T(MensajeSinResultados);
                }
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(ex.Message);
            }
        }

        private void MostrarDetalle(int codPermiso)
        {
            try
            {
                BEFamilia familia = bllPermiso.TraerFamiliaConHijos(codPermiso);

                if (familia == null)
                {
                    lblError.Text = Traductor.T("familia.noEncontrada");
                    return;
                }

                FamiliaSeleccionada = familia.CodPermiso;

                lblDetalleTitulo.Text = Server.HtmlEncode(familia.Nombre);
                lblDetalleDescripcion.Text = Server.HtmlEncode(
                    string.IsNullOrWhiteSpace(familia.Descripcion) ? "-" : familia.Descripcion);
                lblDetalleComponentes.Text = familia.ObtenerHijos().Count.ToString();

                // Aplanar el arbol da las pantallas que la familia habilita,
                // sumando las que aportan las subfamilias
                lblDetallePantallas.Text = familia.CantidadPermisosSimples.ToString();
                lblDetalleEstado.Text = Server.HtmlEncode(Traductor.T(familia.EstadoTexto));

                divArbol.InnerHtml = ArbolPermisos.Renderizar(familia.ObtenerHijos());

                // El ABM de familias es del WebMaster: un Administrador ve la
                // composicion pero no las acciones que no puede ejecutar
                btnModificar.Visible = TienePermiso("ModificarFamilia");
                btnCambiarEstado.Visible = TienePermiso("BajaFamilia");
                btnCambiarEstado.Text = Traductor.T(familia.AccionEstado);
                pnlDetalle.Visible = true;
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(ex.Message);
            }
        }

        private void MostrarCantidad()
        {
            int cantidad = grillaFamilias.CantidadRegistros;

            lblCantidad.Text = cantidad == 1
                ? Traductor.T("consultarFamilias.cantidadUno")
                : Traductor.T("consultarFamilias.cantidad", cantidad);
        }

        private void OcultarDetalle()
        {
            pnlDetalle.Visible = false;
            FamiliaSeleccionada = 0;
        }

        private void MostrarMensajeDeOtraPantalla()
        {
            string mensaje = Convert.ToString(Session["MensajeFamilias"]);

            if (string.IsNullOrEmpty(mensaje))
            {
                return;
            }

            Session.Remove("MensajeFamilias");
            lblExito.Text = Server.HtmlEncode(mensaje);
        }

        private bool HayFiltrosAplicados()
        {
            return !string.IsNullOrWhiteSpace(txtNombre.Text) || ObtenerEstadoElegido().HasValue;
        }

        private bool? ObtenerEstadoElegido()
        {
            if (string.IsNullOrEmpty(ddlEstado.SelectedValue))
            {
                return null;
            }

            return ddlEstado.SelectedValue == "1";
        }
    }
}
