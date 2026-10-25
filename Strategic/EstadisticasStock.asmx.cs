using BE;
using BLL;
using System.Collections.Generic;
using System.Web.Services;
using System.Web.Services.Protocols;

namespace Strategic
{
    // Web service de metricas de inventario. Lo consume Dashboard.aspx.
    // Solo atiende pedidos locales: no pide sesion
    [WebService(Namespace = "http://strategic.local/servicios/", Description = "Metricas del estado del inventario.")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class EstadisticasStock : WebService
    {
        private readonly BLLDashboard bllDashboard = new BLLDashboard();

        [WebMethod(Description = "Retorna los productos cuyo stock es menor o igual al stock minimo.")]
        public List<BEProducto> ObtenerProductosBajoStock(string categoria)
        {
            if (!Context.Request.IsLocal)
            {
                throw new SoapException("El servicio solo atiende pedidos locales.", SoapException.ClientFaultCode);
            }

            return bllDashboard.TraerProductosBajoStock(categoria);
        }
    }
}
