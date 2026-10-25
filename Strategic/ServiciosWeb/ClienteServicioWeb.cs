using Services;
using System;
using System.Globalization;
using System.Net;
using System.Web;
using System.Web.Services.Protocols;

namespace Strategic.ServiciosWeb
{
    // Base de los proxies que usan las pantallas para consumir los web services
    // por SOAP, como los que genera "Agregar referencia web". La direccion del
    // .asmx sale del pedido actual, asi que funciona en cualquier puerto o servidor
    public abstract class ClienteServicioWeb : SoapHttpClientProtocol
    {
        // Espacio de nombres XML de los web services y formato de las fechas
        public const string EspacioNombres = "http://strategic.local/servicios/";
        public const string FormatoFecha = "yyyy-MM-dd";

        protected ClienteServicioWeb(string archivoAsmx)
        {
            HttpRequest pedido = HttpContext.Current.Request;

            Url = new Uri(pedido.Url, VirtualPathUtility.ToAbsolute("~/" + archivoAsmx)).ToString();

            // El servicio responde los errores en el idioma que eligio el usuario
            CookieContainer = new CookieContainer();
            CookieContainer.Add(new Cookie("idioma", Traductor.IdiomaActual, "/", pedido.Url.Host));
        }

        public static string FormatearFecha(DateTime? fecha)
        {
            return fecha.HasValue ? fecha.Value.ToString(FormatoFecha, CultureInfo.InvariantCulture) : string.Empty;
        }

        protected object Llamar(string metodo, params object[] parametros)
        {
            try
            {
                return Invoke(metodo, parametros)[0];
            }
            catch (Exception ex) when (ex is SoapException || ex is WebException)
            {
                // Los datos ya se validan en la pantalla antes de llamar: si el
                // servicio falla es algo inesperado, como la base caida
                throw new Exception(Traductor.T("error.servicioWebNoDisponible"));
            }
        }
    }
}
