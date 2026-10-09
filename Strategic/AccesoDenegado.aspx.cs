using Services;
using Strategic.Componentes;
using System;

namespace Strategic
{
    
    /// Pantalla a la que llega un usuario logueado cuando intenta abrir una
    /// página que su rol no habilita. No hereda de PaginaSegura porque no
    /// tiene permiso propio: la ve cualquiera con sesión.
    
    public partial class AccesoDenegado : PaginaBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SessionManager.HayUsuarioLogueado)
            {
                Response.Redirect("~/Login.aspx");
            }
        }

        public override void ActualizarIdioma()
        {
            Title = Traductor.T("accesoDenegado.titulo");
            btnInicio.Text = Traductor.T("accesoDenegado.irAlInicio");

            string pantalla = Request.QueryString["pantalla"];

            lblMensaje.Text = Server.HtmlEncode(string.IsNullOrWhiteSpace(pantalla)
                ? Traductor.T("accesoDenegado.mensaje")
                : Traductor.T("accesoDenegado.mensajePantalla", pantalla));
        }

        protected void btnInicio_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Default.aspx");
        }
    }
}
