using BE;
using BLL;
using Services;
using Strategic.Seguridad;
using System;
using System.Web.UI;

namespace Strategic
{
    // Alta de Competidor
    public partial class AltaCompetidor : PaginaSegura
    {
        private readonly BLLCompetencia bllCompetencia = new BLLCompetencia();

        protected void btnCrear_Click(object sender, EventArgs e)
        {
            if (!IsValid)
            {
                return;
            }

            try
            {
                BECompetencia nuevo = new BECompetencia();

                nuevo.Nombre = txtNombre.Text.Trim();
                nuevo.Marketplace = ddlMarketplace.SelectedValue;
                nuevo.Descripcion = txtDescripcion.Text.Trim();

                BEUsuario enSesion = SessionManager.UsuarioActual;

                bllCompetencia.AltaCompetidor(nuevo, enSesion.NombreUsuario);

                // El listado es el que confirma el resultado de la operacion
                Session["MensajeCompetidores"] = string.Format("Se creó el competidor {0}", nuevo.Nombre);
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
    }
}
