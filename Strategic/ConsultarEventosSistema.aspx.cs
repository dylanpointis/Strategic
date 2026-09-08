using BE;
using BLL;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Strategic
{
    public partial class ConsultarEventosSistema : Page
    {
        private readonly BLLEvento bllEvento = new BLLEvento();
        private readonly BLLUsuario bllUsuario = new BLLUsuario();

        protected void Page_Load(object sender, EventArgs e)
        {
            // Solo puede entrar el WebMaster o el Administrador
            BEUsuario usuario = SessionManager.UsuarioActual;

            if (usuario == null || (usuario.CodRol != 1 && usuario.CodRol != 2))
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                List<Evento> eventos = bllEvento.TraerListaEventos();

                CargarModulos(eventos);
                CargarGrilla(eventos, "No existen eventos registrados en el sistema");
            }
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            try
            {
                List<Evento> eventos = bllEvento.FiltrarEventos(
                    txtUsuario.Text.Trim(),
                    ddlModulo.SelectedValue,
                    txtEvento.Text.Trim(),
                    ObtenerFecha(txtFechaDesde.Text),
                    ObtenerFecha(txtFechaHasta.Text));

                OcultarDetalle();
                CargarGrilla(eventos, "No se encontraron eventos para los filtros aplicados");
            }
            catch (Exception ex)
            {
                lblError.Text = ex.Message;
            }
        }

        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            txtUsuario.Text = string.Empty;
            txtEvento.Text = string.Empty;
            txtFechaDesde.Text = string.Empty;
            txtFechaHasta.Text = string.Empty;
            ddlModulo.SelectedIndex = 0;

            List<Evento> eventos = bllEvento.TraerListaEventos();

            OcultarDetalle();
            CargarModulos(eventos);
            CargarGrilla(eventos, "No existen eventos registrados en el sistema");
        }

        protected void gvEventos_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "VerUsuario")
            {
                return;
            }

            string nombreUsuario = Convert.ToString(e.CommandArgument);

            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                OcultarDetalle();
                lblError.Text = "El evento seleccionado no tiene un usuario asociado";
                return;
            }

            try
            {
                BEUsuario usuario = bllUsuario.ValidarUsuario(nombreUsuario, string.Empty);

                if (usuario == null)
                {
                    OcultarDetalle();
                    lblError.Text = "No se encontró el usuario que ejecutó el evento";
                    return;
                }

                MostrarDetalle(usuario);
            }
            catch (Exception ex)
            {
                lblError.Text = ex.Message;
            }
        }

        protected void btnCerrarDetalle_Click(object sender, EventArgs e)
        {
            OcultarDetalle();
        }

        private void CargarGrilla(List<Evento> eventos, string mensajeVacio)
        {
            gvEventos.DataSource = eventos;
            gvEventos.DataBind();

            if (gvEventos.HeaderRow != null)
            {
                gvEventos.HeaderRow.TableSection = TableRowSection.TableHeader;
            }

            bool hayEventos = eventos.Count > 0;

            gvEventos.Visible = hayEventos;
            pnlSinResultados.Visible = !hayEventos;
            litSinResultados.Text = mensajeVacio;

            lblCantidad.Text = eventos.Count == 1
                ? "1 evento"
                : string.Format("{0} eventos", eventos.Count);
        }

        private void CargarModulos(List<Evento> eventos)
        {
            ddlModulo.Items.Clear();
            ddlModulo.Items.Add(new ListItem("Todos", string.Empty));

            List<string> modulos = eventos
                .Select(evento => evento.Modulo)
                .Where(modulo => !string.IsNullOrWhiteSpace(modulo))
                .Distinct()
                .OrderBy(modulo => modulo)
                .ToList();

            foreach (string modulo in modulos)
            {
                ddlModulo.Items.Add(new ListItem(modulo, modulo));
            }
        }

        private void MostrarDetalle(BEUsuario usuario)
        {
            litNombreUsuario.Text = Server.HtmlEncode(usuario.NombreUsuario);
            litNombreApellido.Text = Server.HtmlEncode(usuario.Nombre + " " + usuario.Apellido);
            litEmail.Text = Server.HtmlEncode(usuario.Email);
            litRol.Text = Server.HtmlEncode(usuario.Rol != null ? usuario.Rol.Nombre : string.Empty);
            litEstado.Text = ObtenerEstado(usuario);

            pnlDetalleUsuario.Visible = true;
        }

        private void OcultarDetalle()
        {
            pnlDetalleUsuario.Visible = false;
        }

        private string ObtenerEstado(BEUsuario usuario)
        {
            if (!usuario.Activo)
            {
                return "Inactivo";
            }

            return usuario.Bloqueado ? "Bloqueado" : "Activo";
        }

        private DateTime? ObtenerFecha(string valor)
        {
            DateTime fecha;

            if (DateTime.TryParse(valor, out fecha))
            {
                return fecha;
            }

            return null;
        }
    }
}
