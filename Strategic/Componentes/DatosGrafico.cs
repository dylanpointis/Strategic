using Newtonsoft.Json;
using System.Collections.Generic;

namespace Strategic.Componentes
{
    
    /// Arma el JSON que consumen los gráficos de Chart.js desde las páginas.
    /// Con una sola serie el formato es { "etiquetas": [...], "valores": [...] }
    /// y con varias { "etiquetas": [...], "series": [{ "nombre", "valores", "punteada" }] }.
    
    public static class DatosGrafico
    {
        public static string Serializar(List<string> etiquetas, List<decimal> valores)
        {
            return Serializar(new { etiquetas = etiquetas, valores = valores });
        }

        
        /// Serializa un gráfico de varias series, como la comparación entre el
        /// precio propio y el de un competidor. Un valor null deja un hueco en
        /// la línea: así una serie puede ocupar solo una parte del eje.
        
        public static string Serializar(List<string> etiquetas, List<SerieGrafico> series)
        {
            List<object> seriesJson = new List<object>();

            // Se proyectan a objetos anónimos para que las propiedades viajen
            // con el mismo nombre en minúscula que usan el resto de los gráficos
            foreach (SerieGrafico serie in series)
            {
                seriesJson.Add(new { nombre = serie.Nombre, valores = serie.Valores, punteada = serie.Punteada });
            }

            return Serializar(new { etiquetas = etiquetas, series = seriesJson });
        }

        private static string Serializar(object datos)
        {
            JsonSerializerSettings ajustes = new JsonSerializerSettings();

            // Escapa < > & para que el JSON se pueda incrustar sin riesgo dentro de un <script>
            ajustes.StringEscapeHandling = StringEscapeHandling.EscapeHtml;

            return JsonConvert.SerializeObject(datos, ajustes);
        }
    }

    
    /// Una de las series de un gráfico de líneas.
    
    public class SerieGrafico
    {
        public SerieGrafico(string nombre, List<decimal> valores)
        {
            Nombre = nombre;
            Valores = new List<decimal?>();

            foreach (decimal valor in valores)
            {
                Valores.Add(valor);
            }
        }

        
        /// Serie que puede tener huecos y dibujarse punteada, como la parte
        /// proyectada de una predicción.
        
        public SerieGrafico(string nombre, List<decimal?> valores, bool punteada)
        {
            Nombre = nombre;
            Valores = valores;
            Punteada = punteada;
        }

        public string Nombre { get; private set; }

        public List<decimal?> Valores { get; private set; }

        public bool Punteada { get; private set; }
    }
}
