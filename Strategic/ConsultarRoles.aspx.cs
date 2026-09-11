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
        private const string MensajeSinRoles = "No existen roles registrados";
        private const string MensajeSinResultados = "No se encontraron datos con los filtros ingresados";

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

            grillaRoles.AgregarColumna("Nombre", "Rol");
            grillaRoles.AgregarColumna("CantidadComponentesAsignados", "Componentes", "celda-numero");
            grillaRoles.AgregarColumna("UsuariosActivos", "Usuarios activos", "celda-numero");
            grillaRoles.AgregarColumna("EstadoTexto", "Estado");
            grillaRoles.AgregarColumnaAccion("Detalle", "VerDetalle", "Ver permisos");

            btnNuevo.Visible = TienePermiso("AltaRol");
        }

        private void Consultar(bool vieneDeFiltros)
        {
            try
            {
                List<BERol> roles = bllRol.FiltrarRoles(txtNombre.Text, ObtenerEstadoElegido());

                grillaRoles.Cargar(roles, vieneDeFiltros ? MensajeSinResultados : MensajeSinRoles);

                lblCantidad.Text = roles.Count == 1
                    ? "1 rol"
                    : string.Format("{0} roles", roles.Count);

                if (vieneDeFiltros && roles.Count == 0)
                {
                    lblAvisoFiltros.Text = MensajeSinResultados;
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
                    lblError.Text = "No se encontró el rol seleccionado";
                    return;
                }

                RolSeleccionado = rol.CodRol;

                lblDetalleTitulo.Text = Server.HtmlEncode(rol.Nombre);
                lblDetalleComponentes.Text = rol.CantidadComponentes.ToString();

                // Los permisos simples alcanzados salen de aplanar el arbol,
                // sin importar si llegan sueltos o dentro de una familia
                lblDetallePantallas.Text = rol.ObtenerPermisosSimples().Count.ToString();
                lblDetalleUsuarios.Text = rol.UsuariosActivos.ToString();
                lblDetalleEstado.Text = Server.HtmlEncode(rol.EstadoTexto);

                // El arbol se arma como listas anidadas: un div runat=server
                // deja insertar ese html sin envolverlo en un span
                divArbol.InnerHtml = ArbolPermisos.Renderizar(rol.Componentes.ObtenerHijos());

                // El ABM de roles es del WebMaster: un Administrador ve el
                // detalle pero no las acciones que no puede ejecutar
                btnModificar.Visible = TienePermiso("ModificarRol");
                btnCambiarEstado.Visible = TienePermiso("BajaRol");
                btnCambiarEstado.Text = rol.AccionEstado;
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
