using BE;
using DAL;
using System;
using System.Collections.Generic;

namespace BLL
{
    public class BLLVenta
    {
        private readonly DALVenta dalVenta = new DALVenta();

        public List<BEVenta> TraerListaVentas()
        {
            return dalVenta.TraerListaVentas();
        }

        public List<BEVenta> FiltrarVentas(DateTime? fechaInicio, DateTime? fechaFin, int? idProducto, string categoria, string estado)
        {
            if (fechaInicio.HasValue && fechaFin.HasValue && fechaFin.Value < fechaInicio.Value)
            {
                throw new Exception("La fecha hasta no puede ser anterior a la fecha desde");
            }

            return dalVenta.FiltrarVentas(
                fechaInicio,
                fechaFin,
                idProducto.HasValue && idProducto.Value > 0 ? idProducto : null,
                Normalizar(categoria),
                Normalizar(estado));
        }

        /// <summary>
        /// Agrupa las ventas por dia para alimentar el grafico de facturacion.
        /// </summary>
        public List<KeyValuePair<DateTime, decimal>> AgruparFacturacionPorDia(List<BEVenta> ventas)
        {
            Dictionary<DateTime, decimal> acumulado = new Dictionary<DateTime, decimal>();

            foreach (BEVenta venta in ventas)
            {
                DateTime dia = venta.Fecha.Date;

                if (acumulado.ContainsKey(dia))
                {
                    acumulado[dia] = acumulado[dia] + venta.MontoTotal;
                }
                else
                {
                    acumulado.Add(dia, venta.MontoTotal);
                }
            }

            List<DateTime> dias = new List<DateTime>(acumulado.Keys);
            dias.Sort();

            List<KeyValuePair<DateTime, decimal>> facturacion = new List<KeyValuePair<DateTime, decimal>>();

            foreach (DateTime dia in dias)
            {
                facturacion.Add(new KeyValuePair<DateTime, decimal>(dia, acumulado[dia]));
            }

            return facturacion;
        }

        /// <summary>
        /// Monto total facturado por el conjunto de ventas recibido.
        /// </summary>
        public decimal CalcularFacturacionTotal(List<BEVenta> ventas)
        {
            decimal total = 0;

            foreach (BEVenta venta in ventas)
            {
                total = total + venta.MontoTotal;
            }

            return total;
        }

        /// <summary>
        /// Monto promedio por venta (ticket promedio).
        /// </summary>
        public decimal CalcularTicketPromedio(List<BEVenta> ventas)
        {
            if (ventas.Count == 0)
            {
                return 0;
            }

            return CalcularFacturacionTotal(ventas) / ventas.Count;
        }

        private string Normalizar(string valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }
    }
}
