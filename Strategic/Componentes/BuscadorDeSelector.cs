using Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Strategic.Componentes
{
    // Conecta el buscador de SelectorProducto y SelectorCompetidor con
    // Scripts/strategic-selector.js, que filtra el desplegable en el navegador
    public static class BuscadorDeSelector
    {
        private const string ClaveScript = "strategic-selector";

        public static void Registrar(Page pagina, TextBox buscador, DropDownList lista, Label aviso)
        {
            // Mismo tipo y clave: el archivo se incluye una sola vez aunque haya varios selectores
            pagina.ClientScript.RegisterClientScriptInclude(typeof(BuscadorDeSelector), ClaveScript,
                pagina.ResolveUrl("~/Scripts/strategic-selector.js"));

            aviso.Attributes["data-coincidencias"] = Traductor.T("selector.coincidencias");
            aviso.Attributes["data-una-coincidencia"] = Traductor.T("selector.unaCoincidencia");
            aviso.Attributes["data-sin-coincidencias"] = Traductor.T("selector.sinCoincidencias");

            pagina.ClientScript.RegisterStartupScript(typeof(BuscadorDeSelector), lista.ClientID,
                string.Format("Strategic.selector.iniciar('{0}', '{1}', '{2}');", buscador.ClientID, lista.ClientID, aviso.ClientID), true);
        }
    }
}
