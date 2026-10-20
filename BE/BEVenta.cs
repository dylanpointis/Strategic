using System;
using System.Collections.Generic;

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

        // Solo se carga cuando la consulta trae el detalle (TraerListaVentaPorFecha)
        public List<BEItemVenta> Items { get; set; }

        public BEVenta()
        {
            Items = new List<BEItemVenta>();
        }
    }
}
