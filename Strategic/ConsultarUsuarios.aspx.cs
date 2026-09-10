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
    // CU-005-017 - Consultar Usuarios
    // La baja y la reactivacion del CU-005-019 se disparan desde este listado
    public partial class ConsultarUsuarios : Page
    {
        private const string MensajeSinUsuarios = "No existen usuarios registrados";
        private const string MensajeSinResultados = "No se encontraron datos con los filtros ingresados";

        private readonly BLLUsuario bllUsuario = new BLLUsuario();
        private readonly BLLRol bllRol = new BLLRol();

        /// <summary>
        /// Usuario sobre el que se está pidiendo confirmación de baja o
        /// reactivación. Viaja en el ViewState entre el clic y la confirmación.
        /// </summary>
        private string UsuarioAConfirmar
        {
            get { return Convert.ToString(ViewState["UsuarioAConfirmar"]); }
            set { ViewState["UsuarioAConfirmar"] = value; }
        }

        private bool EstadoAConfirmar
        {
            get { return ViewState["EstadoAConfirmar"] != null && Convert.ToBoolean(ViewState["EstadoAConfirmar"]); }
            set { ViewState["EstadoAConfirmar"] = value; }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            // Las columnas se arman en el Init para que la grilla pueda
            // reconstruirse en cada postback a partir de su ViewState
            ConfigurarGrilla();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Solo WebMaster y Administrador segun el CU
            BEUsuario usuario = SessionManager.UsuarioActual;

            if (usuario == null || (usuario.CodRol != 1 && usuario.CodRol != 2))
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                CargarRoles();
                MostrarMensajeDeOtraPantalla();
                Consultar(false);
            }
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            OcultarConfirmacion();
            Consultar(true);
        }

        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            txtTexto.Text = string.Empty;
            ddlRol.SelectedIndex = 0;
            ddlEstado.SelectedIndex = 0;

            OcultarConfirmacion();
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
                PedirConfirmacion(nombreUsuario);
            }
        }

        protected void btnConfirmar_Click(object sender, EventArgs e)
        {
            string nombreUsuario = UsuarioAConfirmar;
            bool activo = EstadoAConfirmar;

            OcultarConfirmacion();

            if (string.IsNullOrEmpty(nombreUsuario))
            {
                return;
            }

            try
            {
                BEUsuario enSesion = SessionManager.UsuarioActual;

                bllUsuario.CambiarEstadoUsuario(nombreUsuario, activo, enSesion.NombreUsuario);

                lblExito.Text = Server.HtmlEncode(string.Format(
                    activo ? "Se reactivó el usuario {0}" : "Se dio de baja al usuario {0}",
                    nombreUsuario));
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(ex.Message);
            }

            Consultar(HayFiltrosAplicados());
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            OcultarConfirmacion();
        }

        private void ConfigurarGrilla()
        {
            grillaUsuarios.ClavePrimaria = "NombreUsuario";
            grillaUsuarios.FilasPorPagina = 10;
            grillaUsuarios.MostrarSelectorFilas = true;

            grillaUsuarios.AgregarColumna("NombreUsuario", "Usuario", "celda-usuario");
            grillaUsuarios.AgregarColumna("Nombre", "Nombre");
            grillaUsuarios.AgregarColumna("Apellido", "Apellido");
            grillaUsuarios.AgregarColumna("Email", "Email");
            grillaUsuarios.AgregarColumna("NombreRol", "Rol");
            grillaUsuarios.AgregarColumna("EstadoTexto", "Estado");
            grillaUsuarios.AgregarColumnaAccion("Modificar", "Modificar", "Modificar");

            // El texto sale de la fila: un usuario activo se da de baja y uno
            // inactivo se reactiva
            grillaUsuarios.AgregarColumnaAccion("Estado", "CambiarEstado", "Cambiar estado", "AccionEstado");
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
                    lblAvisoFiltros.Text = MensajeSinResultados;
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

            lblCantidad.Text = usuarios.Count == 1
                ? "1 usuario"
                : string.Format("{0} usuarios", usuarios.Count);
        }

        private void PedirConfirmacion(string nombreUsuario)
        {
            try
            {
                BEUsuario usuario = bllUsuario.TraerUsuarioPorNombre(nombreUsuario);

                if (usuario == null)
                {
                    lblError.Text = "No se encontró el usuario seleccionado";
                    return;
                }

                UsuarioAConfirmar = usuario.NombreUsuario;
                EstadoAConfirmar = !usuario.Activo;

                lblConfirmacion.Text = Server.HtmlEncode(string.Format(
                    usuario.Activo
                        ? "¿Confirmás dar de baja al usuario {0}? No va a poder iniciar sesión."
                        : "¿Confirmás reactivar al usuario {0}?",
                    usuario.NombreUsuario));

                btnConfirmar.Text = usuario.Activo ? "Dar de baja" : "Reactivar";
                btnConfirmar.CssClass = usuario.Activo ? "btn btn-strategic-danger" : "btn btn-strategic";
                pnlConfirmacion.Visible = true;
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(ex.Message);
            }
        }

        private void OcultarConfirmacion()
        {
            pnlConfirmacion.Visible = false;
            UsuarioAConfirmar = null;
        }

        /// <summary>
        /// El alta y la modificación vuelven a este listado con un mensaje para
        /// mostrar, porque el resultado de la operación se confirma acá.
        /// </summary>
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
            ddlRol.Items.Add(new ListItem("Todos los roles", string.Empty));

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
