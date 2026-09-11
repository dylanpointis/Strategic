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

        /// <summary>
        /// Competidor que se está editando. Se guarda en el ViewState y no se
        /// vuelve a leer de la query string en cada postback.
        /// </summary>
        private int CompetidorEditado
        {
            get { return ViewState["CompetidorEditado"] == null ? 0 : Convert.ToInt32(ViewState["CompetidorEditado"]); }
            set { ViewState["CompetidorEditado"] = value; }
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
                Session["MensajeCompetidores"] = string.Format("Se actualizaron los datos del competidor {0}", competidor.Nombre);
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
                MostrarSinCompetidor("No se indicó qué competidor modificar");
                return;
            }

            try
            {
                BECompetencia competidor = bllCompetencia.TraerCompetidorPorId(idCompetencia);

                if (competidor == null)
                {
                    MostrarSinCompetidor("No se encontró el competidor indicado");
                    return;
                }

                CompetidorEditado = competidor.IdCompetencia;

                lblEstadoActual.Text = Server.HtmlEncode(competidor.Estado);
                txtNombre.Text = competidor.Nombre;
                SeleccionarMarketplace(competidor.Marketplace);
                txtDescripcion.Text = competidor.Descripcion;
            }
            catch (Exception ex)
            {
                MostrarSinCompetidor(ex.Message);
            }
        }

        /// <summary>
        /// Si el competidor tiene un marketplace que no está entre las opciones
        /// del desplegable (dato viejo cargado como texto libre) se agrega para
        /// no perder el valor actual al mostrar el formulario.
        /// </summary>
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
