using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Caching;

namespace Services
{

    /// Traduce los textos del sistema y avisa del cambio de idioma (Observable).
    ///
    /// Las traducciones viven en App_Data/Idiomas/{idioma}.json como pares
    /// clave/texto. Se leen una sola vez y quedan en cache hasta que se
    /// modifique el archivo.
    ///
    /// El idioma elegido se guarda en una cookie, asi que cada pagina nueva
    /// ya se arma en ese idioma. Los observers solo hacen falta en el request
    /// en que se cambia el idioma: la pagina y el master ya cargaron sus
    /// textos y hay que avisarles para que los vuelvan a poner. Por eso la
    /// lista de suscriptores vive en el request y no es global.

    public static class Traductor
    {
        public const string Espanol = "es";
        public const string Ingles = "en";

        private const string NombreCookie = "idioma";
        private const string ClaveIdioma = "Traductor.Idioma";
        private const string ClaveObservers = "Traductor.Observers";

        public static string IdiomaActual
        {
            get
            {
                HttpContext contexto = HttpContext.Current;

                if (contexto == null)
                {
                    return Espanol;
                }

                // Si se cambio en este mismo request todavia no llego la cookie nueva
                string idioma = contexto.Items[ClaveIdioma] as string;

                if (idioma == null)
                {
                    HttpCookie cookie = contexto.Request.Cookies[NombreCookie];
                    idioma = cookie != null ? cookie.Value : Espanol;
                }

                return idioma == Ingles ? Ingles : Espanol;
            }
        }


        /// Texto de la clave en el idioma actual. Si falta, usa el español y,
        /// si tampoco esta, devuelve la clave para que se note en pantalla.

        public static string T(string clave)
        {
            string texto;

            if (ObtenerDiccionario(IdiomaActual).TryGetValue(clave, out texto))
            {
                return texto;
            }

            if (ObtenerDiccionario(Espanol).TryGetValue(clave, out texto))
            {
                return texto;
            }

            return clave;
        }


        /// Texto con datos intercalados, por ejemplo "{0} usuarios".

        public static string T(string clave, params object[] valores)
        {
            return string.Format(T(clave), valores);
        }

        public static void Suscribir(IIdiomaObserver observer)
        {
            List<IIdiomaObserver> observers = Observers;

            if (!observers.Contains(observer))
            {
                observers.Add(observer);
            }
        }

        public static void CambiarIdioma(string idioma)
        {
            HttpContext contexto = HttpContext.Current;

            HttpCookie cookie = new HttpCookie(NombreCookie, idioma);
            cookie.Expires = DateTime.Now.AddYears(1);
            cookie.HttpOnly = true;
            contexto.Response.Cookies.Add(cookie);

            contexto.Items[ClaveIdioma] = idioma;

            foreach (IIdiomaObserver observer in Observers)
            {
                observer.ActualizarIdioma();
            }
        }

        private static List<IIdiomaObserver> Observers
        {
            get
            {
                List<IIdiomaObserver> observers = HttpContext.Current.Items[ClaveObservers] as List<IIdiomaObserver>;

                if (observers == null)
                {
                    observers = new List<IIdiomaObserver>();
                    HttpContext.Current.Items[ClaveObservers] = observers;
                }

                return observers;
            }
        }

        private static Dictionary<string, string> ObtenerDiccionario(string idioma)
        {
            string claveCache = "Traductor." + idioma;
            Dictionary<string, string> diccionario = HttpRuntime.Cache[claveCache] as Dictionary<string, string>;

            if (diccionario == null)
            {
                string ruta = HttpRuntime.AppDomainAppPath + @"App_Data\Idiomas\" + idioma + ".json";

                diccionario = File.Exists(ruta)
                    ? JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(ruta))
                    : new Dictionary<string, string>();

                // Si se edita el json la cache se descarta sola y se vuelve a leer
                HttpRuntime.Cache.Insert(claveCache, diccionario, File.Exists(ruta) ? new CacheDependency(ruta) : null);
            }

            return diccionario;
        }
    }
}
