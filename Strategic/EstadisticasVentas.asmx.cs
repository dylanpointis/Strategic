using BE;
using BLL;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Services;
using System.Web.Services.Protocols;

namespace Strategic
{
    // Web service de metricas de ventas para un periodo. Lo consume Dashboard.aspx.
    // Las fechas van como yyyy-MM-dd (vacia = sin limite) y la categoria vacia
    // significa todas. Solo atiende pedidos locales: no pide sesion
    [WebService(Namespace = "http://strategic.local/servicios/", Description = "Metricas de ventas del negocio para un periodo.")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class EstadisticasVentas : WebService
    {
        private readonly BLLDashboard bllDashboard = new BLLDashboard();

        [WebMethod(Description = "Retorna los 10 productos mas vendidos en el periodo.")]
        public List<BETopProducto> ObtenerTopProductos(string fechaDesde, string fechaHasta, string categoria)
        {
            ValidarPedidoLocal();

            return bllDashboard.TraerTopProductos(ConvertirFecha(fechaDesde), ConvertirFecha(fechaHasta), categoria);
        }

        [WebMethod(Description = "Retorna la facturacion total del periodo.")]
        public decimal ObtenerFacturacion(string fechaDesde, string fechaHasta, string categoria)
        {
            ValidarPedidoLocal();

            return bllDashboard.TraerResumen(ConvertirFecha(fechaDesde), ConvertirFecha(fechaHasta), categoria).FacturacionTotal;
        }

        [WebMethod(Description = "Retorna la cantidad de ventas del periodo.")]
        public int ObtenerCantidadVentas(string fechaDesde, string fechaHasta, string categoria)
        {
            ValidarPedidoLocal();

            return bllDashboard.TraerResumen(ConvertirFecha(fechaDesde), ConvertirFecha(fechaHasta), categoria).CantidadVentas;
        }

        [WebMethod(Description = "Retorna las ventas agrupadas por categoria.")]
        public List<BEVentaPorCategoria> ObtenerProductosVendidosPorCategoria(string fechaDesde, string fechaHasta, string categoria)
        {
            ValidarPedidoLocal();

            return bllDashboard.TraerVentasPorCategoria(ConvertirFecha(fechaDesde), ConvertirFecha(fechaHasta), categoria);
        }

        private void ValidarPedidoLocal()
        {
            if (!Context.Request.IsLocal)
            {
                throw new SoapException("El servicio solo atiende pedidos locales.", SoapException.ClientFaultCode);
            }
        }

        private DateTime? ConvertirFecha(string fecha)
        {
            if (string.IsNullOrWhiteSpace(fecha))
            {
                return null;
            }

            return DateTime.ParseExact(fecha.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
    }
}
