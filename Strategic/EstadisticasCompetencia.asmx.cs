using BE;
using BLL;
using System.Collections.Generic;
using System.Web.Services;
using System.Web.Services.Protocols;

namespace Strategic
{
    // Web service de comparacion de precios con la competencia. Lo consume
    // CompararPrecios.aspx. Id en 0 y categoria vacia significan todos.
    // Solo atiende pedidos locales: no pide sesion
    [WebService(Namespace = "http://strategic.local/servicios/", Description = "Comparacion de precios propios con los de la competencia.")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class EstadisticasCompetencia : WebService
    {
        // Largo de la columna Producto.Categoria
        private const int LargoMaximoCategoria = 100;

        private readonly BLLCompetencia bllCompetencia = new BLLCompetencia();

        [WebMethod(Description = "Retorna la comparacion de precios entre productos propios y productos competidores.")]
        public List<BEComparacionPrecio> ObtenerComparacionPreciosCompetencia(int idProducto, string categoria, int idCompetencia)
        {
            if (!Context.Request.IsLocal)
            {
                throw new SoapException("El servicio solo atiende pedidos locales.", SoapException.ClientFaultCode);
            }

            if (idProducto < 0)
            {
                throw new SoapException("El parametro idProducto no puede ser negativo.", SoapException.ClientFaultCode);
            }

            if (idCompetencia < 0)
            {
                throw new SoapException("El parametro idCompetencia no puede ser negativo.", SoapException.ClientFaultCode);
            }

            string valorCategoria = string.Empty;

            if (categoria != null)
            {
                valorCategoria = categoria.Trim();
            }

            if (valorCategoria.Length > LargoMaximoCategoria)
            {
                throw new SoapException("La categoria no puede superar los " + LargoMaximoCategoria + " caracteres.", SoapException.ClientFaultCode);
            }

            int? producto = null;
            int? competencia = null;

            if (idProducto > 0)
            {
                producto = idProducto;
            }

            if (idCompetencia > 0)
            {
                competencia = idCompetencia;
            }

            return bllCompetencia.FiltrarComparacionPrecios(producto, valorCategoria, competencia);
        }
    }
}
