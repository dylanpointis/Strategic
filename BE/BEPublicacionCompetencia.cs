using System;

namespace BE
{
    
    /// Publicación de un competidor mapeada a un producto propio, tal como se
    /// gestiona desde el panel de monitoreo (CU-007-037). Cada instancia
    /// representa un mapeo de ProductoCompetencia con su estado operativo.
    
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


        /// Clave de traducción del estado, para mostrarlo en pantalla.

        public string EstadoTexto
        {
            get
            {
                switch (Estado)
                {
                    case "Activa":
                        return "publicacion.estado.activa";
                    case "Pausada":
                        return "publicacion.estado.pausada";
                    case "Finalizada":
                        return "publicacion.estado.finalizada";
                    case "No encontrada":
                        return "publicacion.estado.noEncontrada";
                    case "Error":
                        return "publicacion.estado.error";
                    default:
                        return Estado;
                }
            }
        }

        
        /// Último precio relevado de la publicación. Queda en null cuando
        /// todavía no se registró ninguna consulta.
        
        public decimal? UltimoPrecio { get; set; }

        public DateTime? FechaUltimaVerificacion { get; set; }

        
        /// Clave de traducción del botón que pausa o reactiva el monitoreo,
        /// según el estado actual de la fila.
        
        public string AccionPausarReactivar
        {
            get { return Estado == "Activa" ? "comun.accion.pausar" : "comun.accion.reactivar"; }
        }
    }
}
