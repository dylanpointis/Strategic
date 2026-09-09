using System;

namespace BE
{
    public class BEHistorialPrecio
    {
        public int IdHistorialPrecio { get; set; }
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; }
        public decimal Precio { get; set; }
        public DateTime Fecha { get; set; }
    }
}
