using BE;
using BLL;
using Services;
using Strategic.Seguridad;
using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Strategic
{
    // Modificacion de Competidor
    public partial class ModificarCompetidor : PaginaSegura
    {
        private readonly BLLCompetencia bllCompetencia = new BLLCompetencia();

        
        /// Competidor que se está editando. Se guarda en el ViewState y no se
        /// vuelve a leer de la query string en cada postback.
        
        private int CompetidorEditado
        {
            get { return ViewState["CompetidorEditado"] == null ? 0 : Convert.ToInt32(ViewState["CompetidorEditado"]); }
            set { ViewState["CompetidorEditado"] = value; }
        }

        // Clave de traduccion del estado del competidor editado
        private string EstadoEditado
        {
            get { return Convert.ToString(ViewState["EstadoEditado"]); }
            set { ViewState["EstadoEditado"] = value; }
        }

        public override void ActualizarIdioma()
        {
            Title = Traductor.T("modificarCompetidor.titulo");
            lblNombre.Text = Traductor.T("comun.nombre");
            valNombre.ErrorMessage = Traductor.T("competidor.nombreRequerido");
            lblMarketplace.Text = Traductor.T("competidor.marketplace");
            valMarketplace.ErrorMessage = Traductor.T("competidor.marketplaceRequerido");
            lblDescripcion.Text = Traductor.T("familia.descripcion");
            btnCancelar.Text = Traductor.T("comun.cancelar");
            btnGuardar.Text = Traductor.T("comun.guardarCambios");
            btnVolver.Text = Traductor.T("comun.volverAlListado");

            // El estado se conoce recien al recuperar el ViewState
            if (!string.IsNullOrEmpty(EstadoEditado))
            {
                lblEstadoActual.Text = Server.HtmlEncode(Traductor.T(EstadoEditado));
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarCompetidor(Request.QueryString["id"]);
            }
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!IsValid)
            {
                return;
            }

            try
            {
                BECompetencia competidor = new BECompetencia();

                competidor.IdCompetencia = CompetidorEditado;
                competidor.Nombre = txtNombre.Text.Trim();
                competidor.Marketplace = ddlMarketplace.SelectedValue;
                competidor.Descripcion = txtDescripcion.Text.Trim();

                BEUsuario enSesion = SessionManager.UsuarioActual;

                bllCompetencia.ModificarCompetidor(competidor, enSesion.NombreUsuario);

                // El listado es el que confirma el resultado de la operacion
                Session["MensajeCompetidores"] = Traductor.T("modificarCompetidor.exito", competidor.Nombre);
                Response.Redirect("~/ConsultarCompetidores.aspx");
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(ex.Message);
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/ConsultarCompetidores.aspx");
        }

        private void CargarCompetidor(string idTexto)
        {
            int idCompetencia;

            if (!int.TryParse(idTexto, out idCompetencia) || idCompetencia <= 0)
            {
                MostrarSinCompetidor(Traductor.T("modificarCompetidor.sinCompetidor"));
                return;
            }

            try
            {
                BECompetencia competidor = bllCompetencia.TraerCompetidorPorId(idCompetencia);

                if (competidor == null)
                {
                    MostrarSinCompetidor(Traductor.T("competidor.noEncontrado"));
                    return;
                }

                CompetidorEditado = competidor.IdCompetencia;
                EstadoEditado = competidor.EstadoTexto;

                lblEstadoActual.Text = Server.HtmlEncode(Traductor.T(competidor.EstadoTexto));
                txtNombre.Text = competidor.Nombre;
                SeleccionarMarketplace(competidor.Marketplace);
                txtDescripcion.Text = competidor.Descripcion;
            }
            catch (Exception ex)
            {
                MostrarSinCompetidor(ex.Message);
            }
        }

        
        /// Si el competidor tiene un marketplace que no está entre las opciones
        /// del desplegable (dato viejo cargado como texto libre) se agrega para
        /// no perder el valor actual al mostrar el formulario.
        
        private void SeleccionarMarketplace(string marketplace)
        {
            if (ddlMarketplace.Items.FindByValue(marketplace) == null)
            {
                ddlMarketplace.Items.Add(new ListItem(marketplace, marketplace));
            }

            ddlMarketplace.SelectedValue = marketplace;
        }

        private void MostrarSinCompetidor(string mensaje)
        {
            lblSinCompetidor.Text = Server.HtmlEncode(mensaje);

            pnlFormulario.Visible = false;
            pnlSinCompetidor.Visible = true;
        }
    }
}
