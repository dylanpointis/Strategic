using Services;
using System;
using System.Web.UI;

namespace Strategic
{
    public partial class Logout : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SessionManager.CerrarSesion();
            Response.Redirect("~/Login.aspx");
        }
    }
}
