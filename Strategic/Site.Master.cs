using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Services;

namespace Strategic
{
    public partial class SiteMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            navLogin.Visible = !SessionManager.HayUsuarioLogueado;
            navLogout.Visible = SessionManager.HayUsuarioLogueado;
        }
    }
}
