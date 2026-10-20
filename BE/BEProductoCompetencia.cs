using System;

namespace BE
{
    // Publicacion de un competidor asociada a un producto propio, con el
    // ultimo precio relevado. La usan las reglas de precio del CU-002-005
    public class BEProductoCompetencia
    {
        public int IdProductoCompetencia { get; set; }
        public int IdProducto { get; set; }
        public int IdCompetencia { get; set; }
        public string NombreCompetidor { get; set; }
        public string Marketplace { get; set; }
        public string Url { get; set; }
        public string Estado { get; set; }

        // Queda en null si la publicacion todavia no tiene consultas de precio
        public decimal? UltimoPrecio { get; set; }
        public DateTime? FechaUltimoPrecio { get; set; }
    }
}
