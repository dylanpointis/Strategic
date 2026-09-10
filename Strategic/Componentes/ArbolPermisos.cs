using BE.Composite;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Strategic.Componentes
{
    /// <summary>
    /// Dibuja un árbol de componentes como listas anidadas.
    ///
    /// El recorrido es el mismo para hojas y familias: se pide el nombre y se
    /// piden los hijos. Una hoja devuelve la lista vacía y la recursión corta
    /// sola, sin preguntar de qué tipo es cada nodo.
    /// </summary>
    public static class ArbolPermisos
    {
        public static string Renderizar(List<BEComponente> componentes)
        {
            if (componentes == null || componentes.Count == 0)
            {
                return string.Empty;
            }

            StringBuilder html = new StringBuilder();

            EscribirNivel(html, componentes);

            return html.ToString();
        }

        private static void EscribirNivel(StringBuilder html, List<BEComponente> componentes)
        {
            html.Append("<ul>");

            foreach (BEComponente componente in componentes)
            {
                EscribirNodo(html, componente);
            }

            html.Append("</ul>");
        }

        private static void EscribirNodo(StringBuilder html, BEComponente componente)
        {
            html.Append("<li>");
            html.AppendFormat("<span class=\"{0}\">{1}</span>",
                componente.EsFamilia ? "nodo-familia" : "nodo-simple",
                HttpUtility.HtmlEncode(componente.Nombre));

            if (componente.EsFamilia)
            {
                html.AppendFormat("<span class=\"nodo-etiqueta\">{0} permisos</span>",
                    componente.CantidadPermisosSimples);
            }

            if (!componente.Activo)
            {
                html.Append("<span class=\"nodo-etiqueta nodo-baja\">dado de baja</span>");
            }

            List<BEComponente> hijos = componente.ObtenerHijos();

            if (hijos.Count > 0)
            {
                EscribirNivel(html, hijos);
            }

            html.Append("</li>");
        }
    }
}
