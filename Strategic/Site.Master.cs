using BE;
using Services;
using Strategic.Seguridad;
using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace Strategic
{
    public partial class SiteMaster : MasterPage, IIdiomaObserver
    {
        // Atributo que lleva cada item del menu con el nombre del permiso que
        // lo habilita, que es el nombre del aspx al que apunta
        private const string AtributoPermiso = "data-permission";

        protected void Page_Init(object sender, EventArgs e)
        {
            Traductor.Suscribir(this);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            bool haySesion = SessionManager.HayUsuarioLogueado;

            navLogin.Visible = !haySesion;
            navInicio.Visible = haySesion;
            navProfile.Visible = haySesion;
            navBienvenida.Visible = haySesion;
            navCambiarClave.Visible = haySesion;
            navLogout.Visible = haySesion;

            ActualizarIdioma();
            FiltrarMenu(SessionManager.UsuarioActual);
        }

        protected void lnkEspanol_Click(object sender, EventArgs e)
        {
            Traductor.CambiarIdioma(Traductor.Espanol);
        }

        protected void lnkIngles_Click(object sender, EventArgs e)
        {
            Traductor.CambiarIdioma(Traductor.Ingles);
        }

        // Los textos del menu estan en el markup y se resuelven solos al
        // dibujar; aca van los que se arman desde codigo
        public void ActualizarIdioma()
        {
            lnkEspanol.CssClass = Traductor.IdiomaActual == Traductor.Espanol ? "idioma-actual" : string.Empty;
            lnkIngles.CssClass = Traductor.IdiomaActual == Traductor.Ingles ? "idioma-actual" : string.Empty;

            BEUsuario usuario = SessionManager.UsuarioActual;

            if (usuario != null)
            {
                string rol = usuario.Rol != null ? usuario.Rol.Nombre : string.Empty;

                lblBienvenida.Text = Server.HtmlEncode(Traductor.T("master.bienvenida", rol, usuario.NombreUsuario));
            }
        }

        
        /// Deja visibles solo las opciones que el rol del usuario habilita.
        ///
        /// No hace falta recorrer familias acá: el rol ya sabe responder si
        /// alcanza un permiso, porque la recursión vive en el Composite. El
        /// menú pregunta ítem por ítem y listo. Sin sesión se oculta todo.
        
        private void FiltrarMenu(BEUsuario usuario)
        {
            BERol rol = usuario != null ? usuario.Rol : null;

            OcultarItemsSinPermiso(this, rol);
            OcultarDropdownsVacios(this);
        }

        private void OcultarItemsSinPermiso(Control raiz, BERol rol)
        {
            foreach (Control control in raiz.Controls)
            {
                HtmlGenericControl item = control as HtmlGenericControl;

                if (item != null)
                {
                    string permiso = item.Attributes[AtributoPermiso];

                    if (!string.IsNullOrEmpty(permiso))
                    {
                        // Con la suscripcion vencida solo queda la opcion de renovar
                        item.Visible = rol != null
                            && rol.TienePermiso(permiso)
                            && (!SessionManager.SuscripcionVencida || permiso == PaginaSegura.PaginaConSuscripcionVencida);

                        // El atributo era para el servidor, no tiene que viajar al navegador
                        item.Attributes.Remove(AtributoPermiso);
                    }
                }

                if (control.HasControls())
                {
                    OcultarItemsSinPermiso(control, rol);
                }
            }
        }

        // Un menu desplegable sin ninguna opcion visible se oculta entero, para
        // que no quede un titulo que despliega la nada
        private void OcultarDropdownsVacios(Control raiz)
        {
            foreach (Control control in raiz.Controls)
            {
                HtmlGenericControl item = control as HtmlGenericControl;

                if (item != null && EsDropdown(item))
                {
                    item.Visible = TieneAlgunItemVisible(item);
                    continue;
                }

                if (control.HasControls())
                {
                    OcultarDropdownsVacios(control);
                }
            }
        }

        private bool EsDropdown(HtmlGenericControl item)
        {
            string clase = item.Attributes["class"] ?? string.Empty;

            return item.TagName == "li" && clase.Contains("dropdown") && !clase.Contains("profile-menu");
        }

        private bool TieneAlgunItemVisible(Control raiz)
        {
            foreach (Control control in raiz.Controls)
            {
                HtmlGenericControl item = control as HtmlGenericControl;

                if (item != null && item.TagName == "li" && item.Visible)
                {
                    return true;
                }

                if (control.HasControls() && TieneAlgunItemVisible(control))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
