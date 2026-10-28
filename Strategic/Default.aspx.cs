using Services;
using System;
using System.Globalization;
using System.Web.UI;

namespace Strategic
{
    // Pantalla de entrada del sitio. El usuario logueado va directo al Dashboard
    // (CU-002-004) y el que no tiene sesion ve la bienvenida de Strategic
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (SessionManager.HayUsuarioLogueado)
            {
                Response.Redirect("~/Dashboard.aspx");
                return;
            }
        }

        // En PreRender para que tome el idioma elegido en el postback del selector
        protected void Page_PreRender(object sender, EventArgs e)
        {
            Title = Traductor.T("bienvenida.tituloPagina");
        }

        // Precios de referencia en dolares, con el separador de miles del idioma actual
        protected string Precio(decimal monto)
        {
            CultureInfo cultura = Traductor.IdiomaActual == Traductor.Ingles
                ? CultureInfo.GetCultureInfo("en-US")
                : CultureInfo.GetCultureInfo("es-AR");

            return "USD " + monto.ToString("N0", cultura);
        }
    }
}
