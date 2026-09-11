using Services;
using System;
using System.Web.UI;

namespace Strategic
{
    /// <summary>
    /// Pantalla a la que llega un usuario logueado cuando intenta abrir una
    /// página que su rol no habilita. No hereda de PaginaSegura porque no
    /// tiene permiso propio: la ve cualquiera con sesión.
    /// </summary>
    public partial class AccesoDenegado : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SessionManager.HayUsuarioLogueado)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            string pantalla = Request.QueryString["pantalla"];

            lblMensaje.Text = Server.HtmlEncode(string.IsNullOrWhiteSpace(pantalla)
                ? "No tenés permiso para acceder a la pantalla solicitada."
                : string.Format("No tenés permiso para acceder a {0}. Si creés que deberías tenerlo, pedile a un administrador que revise tu rol.", pantalla));
        }

        protected void btnInicio_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Default.aspx");
        }
    }
}
