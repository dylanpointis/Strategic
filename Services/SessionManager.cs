using BE;
using System.Web;

namespace Services
{
    public class SessionManager
    {
        private static readonly SessionManager instancia = new SessionManager();

        // Constructor privado para evitar instanciar desde otras clases
        private SessionManager()
        {
        }

        // única instancia de SessionManager que va a existir.
        public static SessionManager Instance
        {
            get { return instancia; }
        }


        // Representa al usuario que está actualmente guardado en la Session del usuario.
        public static BEUsuario UsuarioActual
        {
            // GET: Obtiene el usuario de la Session.
            get
            {
                return HttpContext.Current.Session["Usuario"] as BEUsuario;
            }
            // SET: Guarda el usuario en la Session.
            set
            {
                HttpContext.Current.Session["Usuario"] = value;
            }
        }

        public static bool HayUsuarioLogueado
        {
            get
            {
                return UsuarioActual != null;
            }
        }

        public static void CerrarSesion()
        {
            HttpContext.Current.Session.Clear();
            HttpContext.Current.Session.Abandon();
        }
    }
}