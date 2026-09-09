using System;

namespace BE
{
    public class BEHistorialStock
    {
        public int IdHistorialStock { get; set; }
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; }
        public int Stock { get; set; }
        public DateTime Fecha { get; set; }
    }
}
