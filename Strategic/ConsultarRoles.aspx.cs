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
    // CU-005-021 - Consultar Roles
    // La baja y la reactivacion del CU-005-023 se disparan desde el detalle
    public partial class ConsultarRoles : PaginaSegura
    {
        private const string MensajeSinRoles = "consultarRoles.sinRoles";
        private const string MensajeSinResultados = "comun.sinResultadosFiltros";

        private readonly BLLRol bllRol = new BLLRol();

        private int RolSeleccionado
        {
            get { return ViewState["RolSeleccionado"] == null ? 0 : Convert.ToInt32(ViewState["RolSeleccionado"]); }
            set { ViewState["RolSeleccionado"] = value; }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            // Las columnas se arman en el Init para que la grilla pueda
            // reconstruirse en cada postback a partir de su ViewState
            ConfigurarGrilla();
        }

        public override void ActualizarIdioma()
        {
            Title = Traductor.T("consultarRoles.titulo");
            lblNombre.Text = Traductor.T("comun.nombre");
            txtNombre.Attributes["placeholder"] = Traductor.T("comun.todos");
            lblEstado.Text = Traductor.T("comun.estado");
            ddlEstado.Items[0].Text = Traductor.T("comun.activos");
            ddlEstado.Items[1].Text = Traductor.T("comun.inactivos");
            ddlEstado.Items[2].Text = Traductor.T("comun.todos");
            btnFiltrar.Text = Traductor.T("comun.filtrar");
            btnLimpiar.Text = Traductor.T("comun.limpiarFiltros");
            btnNuevo.Text = Traductor.T("consultarRoles.nuevo");
            btnModificar.Text = Traductor.T("comun.modificar");
            btnCerrarDetalle.Text = Traductor.T("comun.cerrar");
            MostrarCantidad();

            // El detalle abierto se conoce recien al recuperar el ViewState
            if (RolSeleccionado > 0)
            {
                MostrarDetalle(RolSeleccionado);
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
            Response.Redirect("~/AltaRol.aspx");
        }

        protected void grillaRoles_AccionSeleccionada(object sender, AccionGrillaEventArgs e)
        {
            if (e.Comando != "VerDetalle")
            {
                return;
            }

            int codRol;

            if (int.TryParse(e.ObtenerClave("CodRol"), out codRol))
            {
                MostrarDetalle(codRol);
            }
        }

        protected void btnModificar_Click(object sender, EventArgs e)
        {
            if (RolSeleccionado > 0)
            {
                Response.Redirect("~/ModificarRol.aspx?rol=" + RolSeleccionado);
            }
        }

        protected void btnCambiarEstado_Click(object sender, EventArgs e)
        {
            if (RolSeleccionado > 0)
            {
                Response.Redirect("~/BajaRol.aspx?rol=" + RolSeleccionado);
            }
        }

        protected void btnCerrarDetalle_Click(object sender, EventArgs e)
        {
            OcultarDetalle();
        }

        private void ConfigurarGrilla()
        {
            grillaRoles.ClavePrimaria = "CodRol";
            grillaRoles.FilasPorPagina = 10;
            grillaRoles.MostrarSelectorFilas = true;

            grillaRoles.AgregarColumna("Nombre", "usuario.rol");
            grillaRoles.AgregarColumna("CantidadComponentesAsignados", "consultarRoles.componentes", "celda-numero");
            grillaRoles.AgregarColumna("UsuariosActivos", "consultarRoles.usuariosActivos", "celda-numero");
            grillaRoles.AgregarColumnaTraducida("EstadoTexto", "comun.estado");
            grillaRoles.AgregarColumnaAccion("comun.detalle", "VerDetalle", "consultarRoles.verPermisos");

            btnNuevo.Visible = TienePermiso("AltaRol");
        }

        private void Consultar(bool vieneDeFiltros)
        {
            try
            {
                List<BERol> roles = bllRol.FiltrarRoles(txtNombre.Text, ObtenerEstadoElegido());

                grillaRoles.Cargar(roles, vieneDeFiltros ? MensajeSinResultados : MensajeSinRoles);
                MostrarCantidad();

                if (vieneDeFiltros && roles.Count == 0)
                {
                    lblAvisoFiltros.Text = Traductor.T(MensajeSinResultados);
                }
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(ex.Message);
            }
        }

        private void MostrarDetalle(int codRol)
        {
            try
            {
                BERol rol = bllRol.TraerRolConComponentes(codRol);

                if (rol == null)
                {
                    lblError.Text = Traductor.T("rol.noEncontrado");
                    return;
                }

                RolSeleccionado = rol.CodRol;

                lblDetalleTitulo.Text = Server.HtmlEncode(rol.Nombre);
                lblDetalleComponentes.Text = rol.CantidadComponentes.ToString();

                // Los permisos simples alcanzados salen de aplanar el arbol,
                // sin importar si llegan sueltos o dentro de una familia
                lblDetallePantallas.Text = rol.ObtenerPermisosSimples().Count.ToString();
                lblDetalleUsuarios.Text = rol.UsuariosActivos.ToString();
                lblDetalleEstado.Text = Server.HtmlEncode(Traductor.T(rol.EstadoTexto));

                // El arbol se arma como listas anidadas: un div runat=server
                // deja insertar ese html sin envolverlo en un span
                divArbol.InnerHtml = ArbolPermisos.Renderizar(rol.Componentes.ObtenerHijos());

                // El ABM de roles es del WebMaster: un Administrador ve el
                // detalle pero no las acciones que no puede ejecutar
                btnModificar.Visible = TienePermiso("ModificarRol");
                btnCambiarEstado.Visible = TienePermiso("BajaRol");
                btnCambiarEstado.Text = Traductor.T(rol.AccionEstado);
                pnlDetalle.Visible = true;
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(ex.Message);
            }
        }

        private void MostrarCantidad()
        {
            int cantidad = grillaRoles.CantidadRegistros;

            lblCantidad.Text = cantidad == 1
                ? Traductor.T("consultarRoles.cantidadUno")
                : Traductor.T("consultarRoles.cantidad", cantidad);
        }

        private void OcultarDetalle()
        {
            pnlDetalle.Visible = false;
            RolSeleccionado = 0;
        }

        private void MostrarMensajeDeOtraPantalla()
        {
            string mensaje = Convert.ToString(Session["MensajeRoles"]);

            if (string.IsNullOrEmpty(mensaje))
            {
                return;
            }

            Session.Remove("MensajeRoles");
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
