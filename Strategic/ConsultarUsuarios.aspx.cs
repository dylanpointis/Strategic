using BE;
using BLL;
using Services;
using Strategic.Componentes;
using Strategic.Seguridad;
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Strategic
{
    // CU-005-017 - Consultar Usuarios
    // La baja y la reactivacion del CU-005-019 se disparan desde este listado
    public partial class ConsultarUsuarios : PaginaSegura
    {
        private const string MensajeSinUsuarios = "consultarUsuarios.sinUsuarios";
        private const string MensajeSinResultados = "comun.sinResultadosFiltros";

        private readonly BLLUsuario bllUsuario = new BLLUsuario();
        private readonly BLLRol bllRol = new BLLRol();

        protected void Page_Init(object sender, EventArgs e)
        {
            // Las columnas se arman en el Init para que la grilla pueda
            // reconstruirse en cada postback a partir de su ViewState
            ConfigurarGrilla();
        }

        public override void ActualizarIdioma()
        {
            Title = Traductor.T("consultarUsuarios.titulo");
            lblTexto.Text = Traductor.T("consultarUsuarios.filtroTexto");
            txtTexto.Attributes["placeholder"] = Traductor.T("comun.todos");
            lblRol.Text = Traductor.T("usuario.rol");
            lblEstado.Text = Traductor.T("comun.estado");
            ddlEstado.Items[0].Text = Traductor.T("comun.activos");
            ddlEstado.Items[1].Text = Traductor.T("comun.inactivos");
            ddlEstado.Items[2].Text = Traductor.T("comun.todos");
            btnFiltrar.Text = Traductor.T("comun.filtrar");
            btnLimpiar.Text = Traductor.T("comun.limpiarFiltros");
            btnNuevo.Text = Traductor.T("consultarUsuarios.nuevo");
            MostrarCantidad();

            // La opcion "Todos los roles" se agrega desde codigo y en el Init
            // de un postback todavia no se recupero del ViewState
            if (ddlRol.Items.Count > 0)
            {
                ddlRol.Items[0].Text = Traductor.T("consultarUsuarios.todosLosRoles");
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarRoles();
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
            ddlRol.SelectedIndex = 0;
            ddlEstado.SelectedIndex = 0;

            Consultar(false);
        }

        protected void btnNuevo_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/AltaUsuario.aspx");
        }

        protected void grillaUsuarios_AccionSeleccionada(object sender, AccionGrillaEventArgs e)
        {
            string nombreUsuario = e.ObtenerClave("NombreUsuario");

            if (string.IsNullOrEmpty(nombreUsuario))
            {
                return;
            }

            if (e.Comando == "Modificar")
            {
                Response.Redirect("~/ModificarUsuario.aspx?usuario=" + Server.UrlEncode(nombreUsuario));
                return;
            }

            if (e.Comando == "CambiarEstado")
            {
                Response.Redirect("~/BajaUsuario.aspx?usuario=" + Server.UrlEncode(nombreUsuario));
            }
        }

        private void ConfigurarGrilla()
        {
            grillaUsuarios.ClavePrimaria = "NombreUsuario";
            grillaUsuarios.FilasPorPagina = 10;
            grillaUsuarios.MostrarSelectorFilas = true;

            grillaUsuarios.AgregarColumna("NombreUsuario", "usuario.usuario", "celda-usuario");
            grillaUsuarios.AgregarColumna("Nombre", "usuario.nombre");
            grillaUsuarios.AgregarColumna("Apellido", "usuario.apellido");
            grillaUsuarios.AgregarColumna("Email", "usuario.email");
            grillaUsuarios.AgregarColumna("NombreRol", "usuario.rol");
            grillaUsuarios.AgregarColumnaTraducida("EstadoTexto", "comun.estado");

            // Cada accion lleva a una pantalla con su propio permiso: la columna
            // solo se arma si el rol la alcanza
            if (TienePermiso("ModificarUsuario"))
            {
                grillaUsuarios.AgregarColumnaAccion("comun.modificar", "Modificar", "comun.modificar");
            }

            // El texto sale de la fila: un usuario activo se da de baja y uno
            // inactivo se reactiva
            if (TienePermiso("BajaUsuario"))
            {
                grillaUsuarios.AgregarColumnaAccion("comun.estado", "CambiarEstado", "comun.cambiarEstado", "AccionEstado");
            }

            btnNuevo.Visible = TienePermiso("AltaUsuario");
        }

        private void Consultar(bool vieneDeFiltros)
        {
            try
            {
                List<BEUsuario> usuarios = vieneDeFiltros || HayFiltrosAplicados()
                    ? bllUsuario.FiltrarUsuarios(txtTexto.Text, ObtenerRolElegido(), ObtenerEstadoElegido())
                    : bllUsuario.TraerListaUsuarios();

                MostrarUsuarios(usuarios, vieneDeFiltros ? MensajeSinResultados : MensajeSinUsuarios);

                if (vieneDeFiltros && usuarios.Count == 0)
                {
                    lblAvisoFiltros.Text = Traductor.T(MensajeSinResultados);
                }
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(ex.Message);
                MostrarUsuarios(new List<BEUsuario>(), MensajeSinResultados);
            }
        }

        private void MostrarUsuarios(List<BEUsuario> usuarios, string mensajeVacio)
        {
            grillaUsuarios.Cargar(usuarios, mensajeVacio);
            MostrarCantidad();
        }

        private void MostrarCantidad()
        {
            int cantidad = grillaUsuarios.CantidadRegistros;

            lblCantidad.Text = cantidad == 1
                ? Traductor.T("consultarUsuarios.cantidadUno")
                : Traductor.T("consultarUsuarios.cantidad", cantidad);
        }

        
        /// El alta y la modificación vuelven a este listado con un mensaje para
        /// mostrar, porque el resultado de la operación se confirma acá.
        
        private void MostrarMensajeDeOtraPantalla()
        {
            string mensaje = Convert.ToString(Session["MensajeUsuarios"]);

            if (string.IsNullOrEmpty(mensaje))
            {
                return;
            }

            Session.Remove("MensajeUsuarios");
            lblExito.Text = Server.HtmlEncode(mensaje);
        }

        private void CargarRoles()
        {
            ddlRol.Items.Clear();
            ddlRol.Items.Add(new ListItem(Traductor.T("consultarUsuarios.todosLosRoles"), string.Empty));

            foreach (BERol rol in bllRol.TraerListaRoles())
            {
                ddlRol.Items.Add(new ListItem(rol.Nombre, rol.CodRol.ToString()));
            }
        }

        private bool HayFiltrosAplicados()
        {
            return !string.IsNullOrWhiteSpace(txtTexto.Text)
                || ObtenerRolElegido().HasValue
                || ObtenerEstadoElegido().HasValue;
        }

        private int? ObtenerRolElegido()
        {
            int codRol;

            return int.TryParse(ddlRol.SelectedValue, out codRol) ? (int?)codRol : null;
        }

        private bool? ObtenerEstadoElegido()
        {
            // El CU pide que el listado arranque mostrando los usuarios activos
            if (string.IsNullOrEmpty(ddlEstado.SelectedValue))
            {
                return null;
            }

            return ddlEstado.SelectedValue == "1";
        }
    }
}
