using Services;
using System;
using System.Web.UI;

namespace Strategic.Componentes
{
    // Campo de clave reutilizable. Tiene un boton para mostrar u ocultar lo
    // escrito y, opcionalmente, la lista de requisitos de la politica de claves
    // (se marca en el navegador mientras se escribe) y un segundo campo para
    // confirmarla. La politica se valida igual en el servidor con los
    // validadores del control y despues en BLLUsuario.
    // Los textos se reciben como claves de traduccion y se traducen en el PreRender
    public partial class CampoClave : UserControl
    {
        private const string ClaveScript = "strategic-campo-clave";

        #region Propiedades de configuracion

        public string Etiqueta
        {
            get { return Texto("Etiqueta", "login.clave"); }
            set { ViewState["Etiqueta"] = value; }
        }

        public string MensajeObligatorio
        {
            get { return Texto("MensajeObligatorio", "login.claveRequerida"); }
            set { ViewState["MensajeObligatorio"] = value; }
        }

        public string EtiquetaConfirmacion
        {
            get { return Texto("EtiquetaConfirmacion", "cambiarClave.confirmarClave"); }
            set { ViewState["EtiquetaConfirmacion"] = value; }
        }

        // Muestra los requisitos y valida el formato de la clave
        public bool MostrarRequisitos
        {
            get { return ViewState["MostrarRequisitos"] != null && (bool)ViewState["MostrarRequisitos"]; }
            set { ViewState["MostrarRequisitos"] = value; }
        }

        // Agrega el campo para repetir la clave
        public bool PedirConfirmacion
        {
            get { return ViewState["PedirConfirmacion"] != null && (bool)ViewState["PedirConfirmacion"]; }
            set { ViewState["PedirConfirmacion"] = value; }
        }

        // Valor del atributo autocomplete: current-password o new-password
        public string Autocompletar
        {
            get { return Texto("Autocompletar", "current-password"); }
            set { ViewState["Autocompletar"] = value; }
        }

        // Clase de los mensajes de error, para igualar a los otros campos de la pantalla
        public string ClaseError
        {
            get { return valClave.CssClass; }
            set
            {
                valClave.CssClass = value;
                valFormato.CssClass = value;
                valConfirmacion.CssClass = value;
                valCoinciden.CssClass = value;
            }
        }

        public string ValidationGroup
        {
            get { return valClave.ValidationGroup; }
            set
            {
                valClave.ValidationGroup = value;
                valFormato.ValidationGroup = value;
                valConfirmacion.ValidationGroup = value;
                valCoinciden.ValidationGroup = value;
            }
        }

        #endregion

        #region Valores ingresados

        public string Clave
        {
            get { return txtClave.Text; }
        }

        public string Confirmacion
        {
            get { return txtConfirmacion.Text; }
        }

        #endregion

        protected override void OnLoad(EventArgs e)
        {
            // Los validadores de las partes que no se usan quedan apagados
            // antes de que la pagina procese el boton
            valFormato.Enabled = MostrarRequisitos;
            valConfirmacion.Enabled = PedirConfirmacion;
            valCoinciden.Enabled = PedirConfirmacion;

            base.OnLoad(e);
        }

        protected override void OnPreRender(EventArgs e)
        {
            lblEtiqueta.Text = Traductor.T(Etiqueta);
            valClave.ErrorMessage = Traductor.T(MensajeObligatorio);
            valFormato.ErrorMessage = Traductor.T("cambiarClave.claveNuevaFormato");
            txtClave.Attributes["autocomplete"] = Autocompletar;

            pnlRequisitos.Visible = MostrarRequisitos;
            lblRequisitos.Text = Traductor.T("campoClave.requisitos");
            lblReglaLargo.Text = Traductor.T("campoClave.reglaLargo");
            lblReglaMayuscula.Text = Traductor.T("campoClave.reglaMayuscula");
            lblReglaNumero.Text = Traductor.T("campoClave.reglaNumero");
            lblReglaEspecial.Text = Traductor.T("campoClave.reglaEspecial");

            pnlConfirmacion.Visible = PedirConfirmacion;
            lblConfirmacion.Text = Traductor.T(EtiquetaConfirmacion);
            valConfirmacion.ErrorMessage = Traductor.T("cambiarClave.confirmarClaveRequerida");
            valCoinciden.ErrorMessage = Traductor.T("error.claveConfirmacion");

            // Textos que usa el script del navegador
            divCampoClave.Attributes["data-mostrar"] = Traductor.T("campoClave.mostrar");
            divCampoClave.Attributes["data-ocultar"] = Traductor.T("campoClave.ocultar");
            divCampoClave.Attributes["data-coinciden"] = Traductor.T("campoClave.coinciden");
            divCampoClave.Attributes["data-no-coinciden"] = Traductor.T("error.claveConfirmacion");

            // Mismo tipo y clave: el archivo se incluye una sola vez aunque haya varios campos
            Page.ClientScript.RegisterClientScriptInclude(typeof(CampoClave), ClaveScript,
                Page.ResolveUrl("~/Scripts/strategic-campo-clave.js"));
            Page.ClientScript.RegisterStartupScript(typeof(CampoClave), ClientID,
                string.Format("Strategic.campoClave.iniciar('{0}');", divCampoClave.ClientID), true);

            base.OnPreRender(e);
        }

        public void Limpiar()
        {
            txtClave.Text = string.Empty;
            txtConfirmacion.Text = string.Empty;
        }

        private string Texto(string clave, string porDefecto)
        {
            string texto = Convert.ToString(ViewState[clave]);

            return string.IsNullOrEmpty(texto) ? porDefecto : texto;
        }
    }
}
