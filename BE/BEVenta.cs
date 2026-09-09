using System;

namespace BE
{
    public class BEVenta
    {
        public int IdVenta { get; set; }
        public string NroVenta { get; set; }
        public DateTime Fecha { get; set; }
        public decimal MontoTotal { get; set; }
        public string Estado { get; set; }
        public int CantidadItems { get; set; }
        public int UnidadesVendidas { get; set; }
    }
}
