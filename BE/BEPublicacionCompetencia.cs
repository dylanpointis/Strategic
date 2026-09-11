using System;

namespace BE
{
    /// <summary>
    /// Publicación de un competidor mapeada a un producto propio, tal como se
    /// gestiona desde el panel de monitoreo (CU-007-037). Cada instancia
    /// representa un mapeo de ProductoCompetencia con su estado operativo.
    /// </summary>
    public class BEPublicacionCompetencia
    {
        public int IdProductoCompetencia { get; set; }
        public int IdProducto { get; set; }
        public string CodigoProducto { get; set; }
        public string NombreProducto { get; set; }
        public int IdCompetencia { get; set; }
        public string NombreCompetidor { get; set; }
        public string Marketplace { get; set; }
        public string Url { get; set; }
        public string Estado { get; set; }

        /// <summary>
        /// Último precio relevado de la publicación. Queda en null cuando
        /// todavía no se registró ninguna consulta.
        /// </summary>
        public decimal? UltimoPrecio { get; set; }

        public DateTime? FechaUltimaVerificacion { get; set; }

        /// <summary>
        /// Texto del botón que pausa o reactiva el monitoreo, según el estado
        /// actual de la fila.
        /// </summary>
        public string AccionPausarReactivar
        {
            get { return Estado == "Activa" ? "Pausar" : "Reactivar"; }
        }
    }
}
