using BE;
using Services;
using System;
using System.Web.UI;

namespace Strategic
{
    
    /// Pantalla de entrada del sitio. No tiene contenido propio: el usuario logueado
    /// va directo al Dashboard (CU-002-004) y el que no tiene sesión va al Login,
    /// para no duplicar el dashboard en dos direcciones distintas.
    
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (SessionManager.HayUsuarioLogueado)
            {
                // El rol Soporte Strategic no tiene dashboard: su entrada es la facturacion
                BEUsuario usuario = SessionManager.UsuarioActual;
                bool sinDashboard = usuario.Rol != null
                    && !usuario.Rol.TienePermiso("Dashboard")
                    && usuario.Rol.TienePermiso("ReporteFacturacion");

                Response.Redirect(sinDashboard ? "~/ReporteFacturacion.aspx" : "~/Dashboard.aspx");
                return;
            }

            Response.Redirect("~/Login.aspx");
        }
    }
}
