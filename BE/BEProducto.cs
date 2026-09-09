using System;

namespace BE
{
    public class BEProducto
    {
        public int IdProducto { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Estado { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public int? StockMaximo { get; set; }
        public int? StockMinimo { get; set; }
        public string Categoria { get; set; }
        public bool BorradoLogico { get; set; }
        public DateTime? FechaSincronizacion { get; set; }
        public string Marca { get; set; }

        // Indica si el stock quedo por debajo del minimo definido por el cliente
        public bool StockBajoMinimo
        {
            get { return StockMinimo.HasValue && Stock <= StockMinimo.Value; }
        }
    }
}
