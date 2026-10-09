using System;

namespace BE
{
    
    /// Comparación entre el precio de un producto propio y el último precio
    /// registrado de la publicación de la competencia asociada.
    /// Cada instancia representa un mapeo de ProductoCompetencia.
    
    public class BEComparacionPrecio
    {
        public int IdProductoCompetencia { get; set; }
        public int IdProducto { get; set; }
        public string CodigoProducto { get; set; }
        public string NombreProducto { get; set; }
        public string Categoria { get; set; }
        public int IdCompetencia { get; set; }
        public string NombreCompetidor { get; set; }
        public string Marketplace { get; set; }
        public string Url { get; set; }
        public decimal PrecioPropio { get; set; }

        
        /// Último precio relevado de la publicación. Queda en null cuando el
        /// mapeo todavía no tiene ninguna consulta registrada.
        
        public decimal? PrecioCompetencia { get; set; }

        
        /// Diferencia del precio propio contra el de la competencia.
        /// Positiva significa que el producto propio está más caro.
        
        public decimal? DiferenciaPorcentaje { get; set; }

        public DateTime? FechaConsulta { get; set; }

        public bool TienePrecioRegistrado
        {
            get { return PrecioCompetencia.HasValue; }
        }

        
        /// Resume la posición del precio propio frente al de la competencia.
        /// La grilla muestra esta columna para no obligar a interpretar el signo
        /// de la diferencia porcentual. Devuelve la clave de traducción.
        
        public string Posicion
        {
            get
            {
                if (!DiferenciaPorcentaje.HasValue)
                {
                    return "compararPrecios.posicion.sinRegistro";
                }

                if (DiferenciaPorcentaje.Value > 0)
                {
                    return "compararPrecios.posicion.masCaro";
                }

                return DiferenciaPorcentaje.Value < 0 ? "compararPrecios.posicion.masBarato" : "compararPrecios.posicion.igual";
            }
        }
    }
}
