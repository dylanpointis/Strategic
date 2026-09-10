using System;

namespace BE
{
    /// <summary>
    /// Precio relevado de una publicación de la competencia en una fecha.
    /// Guarda también el precio propio de ese momento para poder comparar
    /// la evolución de los dos precios.
    /// </summary>
    public class BEHistorialPrecioCompetencia
    {
        public int IdPrecioCompetencia { get; set; }
        public int IdProductoCompetencia { get; set; }
        public decimal PrecioCompetencia { get; set; }
        public decimal PrecioPropio { get; set; }
        public decimal? DiferenciaPorcentaje { get; set; }
        public DateTime FechaConsulta { get; set; }
    }
}
