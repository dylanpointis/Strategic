using BE;
using BLL;
using Services;
using Strategic.Componentes;
using Strategic.Seguridad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Strategic
{
    public partial class ConsultarEventosSistema : PaginaSegura
    {
        private readonly BLLEvento bllEvento = new BLLEvento();
        private readonly BLLUsuario bllUsuario = new BLLUsuario();

        // Clave de traduccion del estado del usuario que se muestra en el detalle
        private string EstadoDetalle
        {
            get { return Convert.ToString(ViewState["EstadoDetalle"]); }
            set { ViewState["EstadoDetalle"] = value; }
        }

        public override void ActualizarIdioma()
        {
            Title = Traductor.T("eventos.titulo");
            lblUsuario.Text = Traductor.T("usuario.usuario");
            txtUsuario.Attributes["placeholder"] = Traductor.T("comun.todos");
            lblModulo.Text = Traductor.T("eventos.modulo");
            lblEvento.Text = Traductor.T("eventos.evento");
            txtEvento.Attributes["placeholder"] = Traductor.T("comun.todos");
            lblCriticidad.Text = Traductor.T("eventos.criticidad");

            // Las opciones estan en el aspx: existen desde el Init
            foreach (ListItem opcion in ddlCriticidad.Items)
            {
                opcion.Text = Traductor.T(opcion.Value == string.Empty ? "comun.todas" : "eventos.criticidad." + opcion.Value);
            }
            btnFiltrar.Text = Traductor.T("comun.filtrar");
            btnLimpiar.Text = Traductor.T("comun.limpiarFiltros");
            btnExportarXml.Text = Traductor.T("eventos.exportarXml");
            btnCerrarDetalle.Text = Traductor.T("eventos.cerrarDetalle");
            btnBajaEventos.Text = Traductor.T("eventos.bajaEventos");
            MostrarCantidad();

            // Lo que se arma desde codigo solo existe una vez recuperado el
            // ViewState, por eso se pregunta antes de traducirlo
            if (ddlModulo.Items.Count > 0)
            {
                ddlModulo.Items[0].Text = Traductor.T("comun.todos");
            }

            if (!string.IsNullOrEmpty(EstadoDetalle))
            {
                lblEstado.Text = Server.HtmlEncode(Traductor.T(EstadoDetalle));
            }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            // Las columnas se arman en el Init para que la grilla pueda reconstruirse
            // en cada postback a partir de su ViewState
            ConfigurarGrilla();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                btnBajaEventos.Visible = TienePermiso("BajaEventos");

                List<Evento> eventos = bllEvento.TraerListaEventos();

                CargarModulos(eventos);
                CargarGrilla(eventos, "eventos.sinEventos");
                MostrarMensajeDeOtraPantalla();
            }
        }

        protected void btnBajaEventos_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/BajaEventos.aspx");
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            try
            {
                List<Evento> eventos = bllEvento.FiltrarEventos(
                    txtUsuario.Text.Trim(),
                    ddlModulo.SelectedValue,
                    txtEvento.Text.Trim(),
                    CriticidadElegida(),
                    filtroFechas.FechaDesde,
                    filtroFechas.FechaHasta);

                OcultarDetalle();
                CargarGrilla(eventos, "eventos.sinResultados");
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(ex.Message);
            }
        }

        // Exporta lo que hay en los filtros en este momento, aunque todavia no se haya filtrado
        protected void btnExportarXml_Click(object sender, EventArgs e)
        {
            byte[] archivo;

            try
            {
                archivo = bllEvento.ExportarEventosXml(
                    txtUsuario.Text.Trim(),
                    ddlModulo.SelectedValue,
                    txtEvento.Text.Trim(),
                    CriticidadElegida(),
                    filtroFechas.FechaDesde,
                    filtroFechas.FechaHasta,
                    SessionManager.UsuarioActual.NombreUsuario);
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(ex.Message);
                return;
            }

            Response.Clear();
            Response.ContentType = "application/xml";
            Response.AppendHeader("Content-Disposition", "attachment; filename=" + bllEvento.NombreArchivoExportacion());
            Response.AppendHeader("Content-Length", archivo.Length.ToString());
            Response.BinaryWrite(archivo);
            Response.Flush();
            Response.SuppressContent = true;
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            txtUsuario.Text = string.Empty;
            txtEvento.Text = string.Empty;
            ddlModulo.SelectedIndex = 0;
            ddlCriticidad.SelectedIndex = 0;
            filtroFechas.Limpiar();

            List<Evento> eventos = bllEvento.TraerListaEventos();

            OcultarDetalle();
            CargarModulos(eventos);
            CargarGrilla(eventos, "eventos.sinEventos");
        }

        protected void grillaEventos_AccionSeleccionada(object sender, AccionGrillaEventArgs e)
        {
            if (e.Comando != "VerUsuario")
            {
                return;
            }

            string nombreUsuario = e.ObtenerClave("NombreUsuario");

            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                OcultarDetalle();
                lblError.Text = Traductor.T("eventos.sinUsuarioAsociado");
                return;
            }

            try
            {
                BEUsuario usuario = bllUsuario.ValidarUsuario(nombreUsuario, string.Empty);

                if (usuario == null)
                {
                    OcultarDetalle();
                    lblError.Text = Traductor.T("eventos.usuarioNoEncontrado");
                    return;
                }

                MostrarDetalle(usuario);
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(ex.Message);
            }
        }

        protected void btnCerrarDetalle_Click(object sender, EventArgs e)
        {
            OcultarDetalle();
        }

        private void ConfigurarGrilla()
        {
            grillaEventos.ClavePrimaria = "CodEvento,NombreUsuario";
            grillaEventos.FilasPorPagina = 10;
            grillaEventos.MostrarSelectorFilas = true;

            grillaEventos.AgregarColumna("CodEvento", "eventos.codigo", "celda-codigo");
            grillaEventos.AgregarColumna("Fecha", "comun.fecha", "celda-fecha");
            grillaEventos.AgregarColumna("Hora", "eventos.hora", "celda-fecha");
            grillaEventos.AgregarColumna("NombreUsuario", "usuario.usuario", "celda-usuario");
            grillaEventos.AgregarColumna("Modulo", "eventos.modulo");
            grillaEventos.AgregarColumna("Descripcion", "eventos.evento");
            grillaEventos.AgregarColumna("Criticidad", "eventos.criticidad");
            grillaEventos.AgregarColumnaAccion("comun.detalle", "VerUsuario", "eventos.verUsuario");
        }

        private void CargarGrilla(List<Evento> eventos, string mensajeVacio)
        {
            grillaEventos.Cargar(eventos, mensajeVacio);
            MostrarCantidad();
        }

        private void MostrarCantidad()
        {
            int cantidad = grillaEventos.CantidadRegistros;

            lblCantidad.Text = cantidad == 1
                ? Traductor.T("eventos.cantidadUno")
                : Traductor.T("eventos.cantidad", cantidad);
        }

        private void CargarModulos(List<Evento> eventos)
        {
            ddlModulo.Items.Clear();
            ddlModulo.Items.Add(new ListItem(Traductor.T("comun.todos"), string.Empty));

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
            lblNombreUsuario.Text = Server.HtmlEncode(usuario.NombreUsuario);
            lblNombreApellido.Text = Server.HtmlEncode(usuario.Nombre + " " + usuario.Apellido);
            lblEmail.Text = Server.HtmlEncode(usuario.Email);
            lblRol.Text = Server.HtmlEncode(usuario.Rol != null ? usuario.Rol.Nombre : string.Empty);
            lblEstado.Text = Server.HtmlEncode(Traductor.T(usuario.EstadoTexto));
            EstadoDetalle = usuario.EstadoTexto;

            pnlDetalleUsuario.Visible = true;
        }

        // Exito de la baja de eventos, que se confirma en BajaEventos.aspx
        private void MostrarMensajeDeOtraPantalla()
        {
            string mensaje = Convert.ToString(Session["MensajeEventos"]);

            if (string.IsNullOrEmpty(mensaje))
            {
                return;
            }

            Session.Remove("MensajeEventos");
            lblExito.Text = Server.HtmlEncode(mensaje);
        }

        private int? CriticidadElegida()
        {
            int criticidad;
            return int.TryParse(ddlCriticidad.SelectedValue, out criticidad) ? criticidad : (int?)null;
        }

        private void OcultarDetalle()
        {
            pnlDetalleUsuario.Visible = false;
            EstadoDetalle = null;
        }
    }
}
