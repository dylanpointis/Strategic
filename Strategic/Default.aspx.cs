using Services;
using System;
using System.Web.UI;

namespace Strategic
{
    /// <summary>
    /// Pantalla de entrada del sitio. No tiene contenido propio: el usuario logueado
    /// va directo al Dashboard (CU-002-004) y el que no tiene sesión va al Login,
    /// para no duplicar el dashboard en dos direcciones distintas.
    /// </summary>
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (SessionManager.HayUsuarioLogueado)
            {
                Response.Redirect("~/Dashboard.aspx");
                return;
            }

            Response.Redirect("~/Login.aspx");
        }
    }
}
