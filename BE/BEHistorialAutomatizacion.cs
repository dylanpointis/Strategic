using System;

namespace BE
{
    // Resultado de ejecutar una automatizacion sobre un producto
    public class BEHistorialAutomatizacion
    {
        public const string EstadoExitosa = "Exitosa";
        public const string EstadoConError = "ConError";

        public int IdHistorial { get; set; }
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; }
        public int IdAutomatizacion { get; set; }
        public string NombreAutomatizacion { get; set; }
        public string AccionEjecutada { get; set; }
        public string ValorAnterior { get; set; }
        public string ValorNuevo { get; set; }
        public string Estado { get; set; }
        public DateTime FechaEjecucion { get; set; }

        // Clave de traduccion del resultado, para mostrarlo en pantalla
        public string EstadoTexto
        {
            get { return "automatizacion.resultado." + Estado; }
        }
    }
}
