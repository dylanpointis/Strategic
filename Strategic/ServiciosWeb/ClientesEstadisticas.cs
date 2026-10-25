using BE;
using System.Web.Services;
using System.Web.Services.Description;
using System.Web.Services.Protocols;

namespace Strategic.ServiciosWeb
{
    // Proxy de EstadisticasVentas.asmx
    [WebServiceBinding(Name = "EstadisticasVentasSoap", Namespace = ClienteServicioWeb.EspacioNombres)]
    public class ClienteEstadisticasVentas : ClienteServicioWeb
    {
        private const string Ns = ClienteServicioWeb.EspacioNombres;

        public ClienteEstadisticasVentas() : base("EstadisticasVentas.asmx")
        {
        }

        [SoapDocumentMethod(Ns + "ObtenerTopProductos", RequestNamespace = Ns, ResponseNamespace = Ns, Use = SoapBindingUse.Literal, ParameterStyle = SoapParameterStyle.Wrapped)]
        public BETopProducto[] ObtenerTopProductos(string fechaDesde, string fechaHasta, string categoria)
        {
            return (BETopProducto[])Llamar("ObtenerTopProductos", fechaDesde, fechaHasta, categoria);
        }

        [SoapDocumentMethod(Ns + "ObtenerFacturacion", RequestNamespace = Ns, ResponseNamespace = Ns, Use = SoapBindingUse.Literal, ParameterStyle = SoapParameterStyle.Wrapped)]
        public decimal ObtenerFacturacion(string fechaDesde, string fechaHasta, string categoria)
        {
            return (decimal)Llamar("ObtenerFacturacion", fechaDesde, fechaHasta, categoria);
        }

        [SoapDocumentMethod(Ns + "ObtenerCantidadVentas", RequestNamespace = Ns, ResponseNamespace = Ns, Use = SoapBindingUse.Literal, ParameterStyle = SoapParameterStyle.Wrapped)]
        public int ObtenerCantidadVentas(string fechaDesde, string fechaHasta, string categoria)
        {
            return (int)Llamar("ObtenerCantidadVentas", fechaDesde, fechaHasta, categoria);
        }

        [SoapDocumentMethod(Ns + "ObtenerProductosVendidosPorCategoria", RequestNamespace = Ns, ResponseNamespace = Ns, Use = SoapBindingUse.Literal, ParameterStyle = SoapParameterStyle.Wrapped)]
        public BEVentaPorCategoria[] ObtenerProductosVendidosPorCategoria(string fechaDesde, string fechaHasta, string categoria)
        {
            return (BEVentaPorCategoria[])Llamar("ObtenerProductosVendidosPorCategoria", fechaDesde, fechaHasta, categoria);
        }
    }

    // Proxy de EstadisticasStock.asmx
    [WebServiceBinding(Name = "EstadisticasStockSoap", Namespace = ClienteServicioWeb.EspacioNombres)]
    public class ClienteEstadisticasStock : ClienteServicioWeb
    {
        private const string Ns = ClienteServicioWeb.EspacioNombres;

        public ClienteEstadisticasStock() : base("EstadisticasStock.asmx")
        {
        }

        [SoapDocumentMethod(Ns + "ObtenerProductosBajoStock", RequestNamespace = Ns, ResponseNamespace = Ns, Use = SoapBindingUse.Literal, ParameterStyle = SoapParameterStyle.Wrapped)]
        public BEProducto[] ObtenerProductosBajoStock(string categoria)
        {
            return (BEProducto[])Llamar("ObtenerProductosBajoStock", categoria);
        }
    }

    // Proxy de EstadisticasCompetencia.asmx
    [WebServiceBinding(Name = "EstadisticasCompetenciaSoap", Namespace = ClienteServicioWeb.EspacioNombres)]
    public class ClienteEstadisticasCompetencia : ClienteServicioWeb
    {
        private const string Ns = ClienteServicioWeb.EspacioNombres;

        public ClienteEstadisticasCompetencia() : base("EstadisticasCompetencia.asmx")
        {
        }

        [SoapDocumentMethod(Ns + "ObtenerComparacionPreciosCompetencia", RequestNamespace = Ns, ResponseNamespace = Ns, Use = SoapBindingUse.Literal, ParameterStyle = SoapParameterStyle.Wrapped)]
        public BEComparacionPrecio[] ObtenerComparacionPreciosCompetencia(int idProducto, string categoria, int idCompetencia)
        {
            return (BEComparacionPrecio[])Llamar("ObtenerComparacionPreciosCompetencia", idProducto, categoria, idCompetencia);
        }
    }
}
