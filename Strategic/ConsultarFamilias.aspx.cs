using BE;
using BE.Composite;
using BLL;
using Services;
using Strategic.Componentes;
using System;
using System.Collections.Generic;
using System.Web.UI;

namespace Strategic
{
    // CU-005-025 - Consultar Familias
    // La baja y la reactivacion del CU-005-027 se disparan desde el detalle
    public partial class ConsultarFamilias : Page
    {
        private const string MensajeSinFamilias = "No existen familias registradas";
        private const string MensajeSinResultados = "No se encontraron datos con los filtros ingresados";

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

        protected void Page_Load(object sender, EventArgs e)
        {
            // Consultar familias lo pueden hacer WebMaster y Administrador
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
            try
            {
                BEFamilia familia = bllPermiso.TraerFamiliaConHijos(FamiliaSeleccionada);

                if (familia == null)
                {
                    lblError.Text = "No se encontró la familia seleccionada";
                    return;
                }

                lblConfirmacion.Text = Server.HtmlEncode(string.Format(
                    familia.Activo
                        ? "¿Confirmás dar de baja la familia {0}? No va a poder asignarse a nuevos roles."
                        : "¿Confirmás reactivar la familia {0}?",
                    familia.Nombre));

                btnConfirmar.Text = familia.Activo ? "Dar de baja" : "Reactivar";
                pnlConfirmacion.Visible = true;
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(ex.Message);
            }
        }

        protected void btnConfirmar_Click(object sender, EventArgs e)
        {
            pnlConfirmacion.Visible = false;

            try
            {
                BEFamilia familia = bllPermiso.TraerFamiliaConHijos(FamiliaSeleccionada);

                if (familia == null)
                {
                    lblError.Text = "No se encontró la familia seleccionada";
                    return;
                }

                BEUsuario enSesion = SessionManager.UsuarioActual;
                bool nuevoEstado = !familia.Activo;

                bllPermiso.CambiarEstadoFamilia(familia.CodPermiso, nuevoEstado, enSesion.NombreUsuario);

                lblExito.Text = Server.HtmlEncode(string.Format(
                    nuevoEstado ? "Se reactivó la familia {0}" : "Se dio de baja la familia {0}",
                    familia.Nombre));

                OcultarDetalle();
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(ex.Message);
            }

            Consultar(HayFiltrosAplicados());
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            pnlConfirmacion.Visible = false;
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

            grillaFamilias.AgregarColumna("Nombre", "Familia");
            grillaFamilias.AgregarColumna("Descripcion", "Descripción");
            grillaFamilias.AgregarColumna("CantidadComponentesAsignados", "Elementos", "celda-numero");
            grillaFamilias.AgregarColumna("EstadoTexto", "Estado");
            grillaFamilias.AgregarColumnaAccion("Detalle", "VerDetalle", "Ver composición");
        }

        private void Consultar(bool vieneDeFiltros)
        {
            try
            {
                List<BEFamilia> familias = bllPermiso.FiltrarFamilias(txtNombre.Text, ObtenerEstadoElegido());

                grillaFamilias.Cargar(familias, vieneDeFiltros ? MensajeSinResultados : MensajeSinFamilias);

                lblCantidad.Text = familias.Count == 1
                    ? "1 familia"
                    : string.Format("{0} familias", familias.Count);

                if (vieneDeFiltros && familias.Count == 0)
                {
                    lblAvisoFiltros.Text = MensajeSinResultados;
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
                    lblError.Text = "No se encontró la familia seleccionada";
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
                lblDetalleEstado.Text = Server.HtmlEncode(familia.EstadoTexto);

                divArbol.InnerHtml = ArbolPermisos.Renderizar(familia.ObtenerHijos());

                btnCambiarEstado.Text = familia.AccionEstado;
                pnlConfirmacion.Visible = false;
                pnlDetalle.Visible = true;
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(ex.Message);
            }
        }

        private void OcultarDetalle()
        {
            pnlDetalle.Visible = false;
            pnlConfirmacion.Visible = false;
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
