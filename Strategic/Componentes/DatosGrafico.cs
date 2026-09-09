using Newtonsoft.Json;
using System.Collections.Generic;

namespace Strategic.Componentes
{
    /// <summary>
    /// Arma el JSON que consumen los gráficos de Chart.js desde las páginas.
    /// El formato de salida es { "etiquetas": [...], "valores": [...] }.
    /// </summary>
    public static class DatosGrafico
    {
        public static string Serializar(List<string> etiquetas, List<decimal> valores)
        {
            JsonSerializerSettings ajustes = new JsonSerializerSettings();

            // Escapa < > & para que el JSON se pueda incrustar sin riesgo dentro de un <script>
            ajustes.StringEscapeHandling = StringEscapeHandling.EscapeHtml;

            return JsonConvert.SerializeObject(new { etiquetas = etiquetas, valores = valores }, ajustes);
        }
    }
}
