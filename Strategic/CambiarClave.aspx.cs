using System;
using System.Web.UI;

namespace Strategic
{
    public partial class CambiarClave : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Default.aspx");
        }
    }
}
