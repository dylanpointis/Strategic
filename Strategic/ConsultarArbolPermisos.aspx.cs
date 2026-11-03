using BE;
using BE.Composite;
using BLL;
using Services;
using Strategic.Componentes;
using Strategic.Seguridad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Strategic
{
    // Arbol de permisos: cada rol activo con su arbol de familias y permisos
    // sueltos. Es de solo lectura, para el WebMaster y el Administrador
    public partial class ConsultarArbolPermisos : PaginaSegura
    {
        private readonly BLLRol bllRol = new BLLRol();

        public override void ActualizarIdioma()
        {
            Title = Traductor.T("consultarArbolPermisos.titulo");

            // Las tarjetas se arman desde codigo: al cambiar el idioma se vuelven a cargar
            if (IsPostBack)
            {
                CargarRoles();
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarRoles();
            }
        }

        protected void rptRoles_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
            {
                return;
            }

            BERol rol = (BERol)e.Item.DataItem;
            List<BEComponente> asignados = rol.Componentes.ObtenerHijos();

            // El rol mezcla familias y permisos sueltos: se muestran por separado
            List<BEComponente> familias = asignados.Where(c => c.EsFamilia).ToList();
            List<BEComponente> sueltos = asignados.Where(c => !c.EsFamilia).ToList();

            ((Label)e.Item.FindControl("lblNombre")).Text = Server.HtmlEncode(rol.Nombre);
            ((Label)e.Item.FindControl("lblResumen")).Text = Server.HtmlEncode(rol.UsuariosActivos == 1
                ? Traductor.T("consultarArbolPermisos.resumenUnUsuario", rol.ObtenerPermisosSimples().Count)
                : Traductor.T("consultarArbolPermisos.resumen", rol.ObtenerPermisosSimples().Count, rol.UsuariosActivos));

            ((Label)e.Item.FindControl("lblTituloFamilias")).Text = Traductor.T("consultarArbolPermisos.familias");
            ((Label)e.Item.FindControl("lblTituloSueltos")).Text = Traductor.T("consultarArbolPermisos.permisosSueltos");

            MostrarArbol(e.Item, "divFamilias", "lblSinFamilias", familias, "consultarArbolPermisos.sinFamilias");
            MostrarArbol(e.Item, "divSueltos", "lblSinSueltos", sueltos, "consultarArbolPermisos.sinPermisosSueltos");
        }

        private void CargarRoles()
        {
            try
            {
                List<BERol> roles = bllRol.TraerRolesConArbol();

                lblSinRoles.Text = roles.Count == 0 ? Traductor.T("consultarArbolPermisos.sinRoles") : string.Empty;

                rptRoles.DataSource = roles;
                rptRoles.DataBind();
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(ex.Message);
            }
        }

        // El arbol se arma como listas anidadas: un div runat=server deja
        // insertar ese html sin envolverlo en un span
        private void MostrarArbol(RepeaterItem item, string idArbol, string idVacio, List<BEComponente> componentes, string mensajeVacio)
        {
            HtmlGenericControl arbol = (HtmlGenericControl)item.FindControl(idArbol);
            Label vacio = (Label)item.FindControl(idVacio);

            arbol.InnerHtml = ArbolPermisos.Renderizar(componentes);
            arbol.Visible = componentes.Count > 0;
            vacio.Text = Traductor.T(mensajeVacio);
            vacio.Visible = componentes.Count == 0;
        }
    }
}
