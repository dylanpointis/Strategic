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
        // Largo de la columna Producto.Categoria
        private const int LargoMaximoCategoria = 100;

        private readonly BLLDashboard bllDashboard = new BLLDashboard();

        [WebMethod(Description = "Retorna los 10 productos mas vendidos en el periodo.")]
        public List<BETopProducto> ObtenerTopProductos(string fechaDesde, string fechaHasta, string categoria)
        {
            ValidarPedidoLocal();

            DateTime? desde = ConvertirFecha(fechaDesde, "fechaDesde");
            DateTime? hasta = ConvertirFecha(fechaHasta, "fechaHasta");
            ValidarPeriodo(desde, hasta);

            return bllDashboard.TraerTopProductos(desde, hasta, ValidarCategoria(categoria));
        }

        [WebMethod(Description = "Retorna la facturacion total del periodo.")]
        public decimal ObtenerFacturacion(string fechaDesde, string fechaHasta, string categoria)
        {
            ValidarPedidoLocal();

            DateTime? desde = ConvertirFecha(fechaDesde, "fechaDesde");
            DateTime? hasta = ConvertirFecha(fechaHasta, "fechaHasta");
            ValidarPeriodo(desde, hasta);

            return bllDashboard.TraerResumen(desde, hasta, ValidarCategoria(categoria)).FacturacionTotal;
        }

        [WebMethod(Description = "Retorna la cantidad de ventas del periodo.")]
        public int ObtenerCantidadVentas(string fechaDesde, string fechaHasta, string categoria)
        {
            ValidarPedidoLocal();

            DateTime? desde = ConvertirFecha(fechaDesde, "fechaDesde");
            DateTime? hasta = ConvertirFecha(fechaHasta, "fechaHasta");
            ValidarPeriodo(desde, hasta);

            return bllDashboard.TraerResumen(desde, hasta, ValidarCategoria(categoria)).CantidadVentas;
        }

        [WebMethod(Description = "Retorna las ventas agrupadas por categoria.")]
        public List<BEVentaPorCategoria> ObtenerProductosVendidosPorCategoria(string fechaDesde, string fechaHasta, string categoria)
        {
            ValidarPedidoLocal();

            DateTime? desde = ConvertirFecha(fechaDesde, "fechaDesde");
            DateTime? hasta = ConvertirFecha(fechaHasta, "fechaHasta");
            ValidarPeriodo(desde, hasta);

            return bllDashboard.TraerVentasPorCategoria(desde, hasta, ValidarCategoria(categoria));
        }

        private void ValidarPedidoLocal()
        {
            if (!Context.Request.IsLocal)
            {
                throw new SoapException("El servicio solo atiende pedidos locales.", SoapException.ClientFaultCode);
            }
        }

        private DateTime? ConvertirFecha(string fecha, string parametro)
        {
            if (string.IsNullOrWhiteSpace(fecha))
            {
                return null;
            }

            DateTime resultado;

            if (!DateTime.TryParseExact(fecha.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out resultado))
            {
                throw new SoapException("El parametro " + parametro + " debe tener el formato yyyy-MM-dd.", SoapException.ClientFaultCode);
            }

            return resultado;
        }

        private void ValidarPeriodo(DateTime? desde, DateTime? hasta)
        {
            if (desde.HasValue && hasta.HasValue)
            {
                if (desde.Value > hasta.Value)
                {
                    throw new SoapException("La fecha desde no puede ser posterior a la fecha hasta.", SoapException.ClientFaultCode);
                }
            }
        }

        private string ValidarCategoria(string categoria)
        {
            if (categoria == null)
            {
                return string.Empty;
            }

            string valor = categoria.Trim();

            if (valor.Length > LargoMaximoCategoria)
            {
                throw new SoapException("La categoria no puede superar los " + LargoMaximoCategoria + " caracteres.", SoapException.ClientFaultCode);
            }

            return valor;
        }
    }
}
