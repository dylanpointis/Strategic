using BE;
using Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Strategic.Componentes
{
    // Componente reutilizable para elegir un competidor registrado. Tiene un
    // buscador que filtra el desplegable mientras se escribe, por nombre o
    // marketplace. Expone el competidor elegido en IdCompetencia y avisa con
    // CompetidorSeleccionado cuando cambia (si AutoPostBack esta activo).
    // Los textos se reciben como claves de traduccion y se traducen en el PreRender
    public partial class SelectorCompetidor : UserControl
    {
        public event EventHandler CompetidorSeleccionado;

        #region Propiedades de configuracion

        public string Etiqueta
        {
            get { return Texto("Etiqueta", "mapearProductos.competidor"); }
            set { ViewState["Etiqueta"] = value; }
        }

        // Clave del texto de la primera opcion, la que no elige ningun competidor
        public string TextoOpcionVacia
        {
            get { return Texto("TextoOpcionVacia", "monitoreo.todosLosCompetidores"); }
            set { ViewState["TextoOpcionVacia"] = value; }
        }

        // Muestra el marketplace al lado del nombre: "Nombre (Marketplace)"
        public bool MostrarMarketplace
        {
            get { return Convert.ToBoolean(ViewState["MostrarMarketplace"]); }
            set { ViewState["MostrarMarketplace"] = value; }
        }

        public bool AutoPostBack
        {
            get { return ddlCompetidor.AutoPostBack; }
            set { ddlCompetidor.AutoPostBack = value; }
        }

        // Si es obligatorio, valida con un RequiredFieldValidator del grupo indicado
        public bool Obligatorio
        {
            get { return valCompetidor.Enabled; }
            set { valCompetidor.Enabled = value; }
        }

        public string MensajeObligatorio
        {
            get { return Texto("MensajeObligatorio", "selector.competidorRequerido"); }
            set { ViewState["MensajeObligatorio"] = value; }
        }

        public string ValidationGroup
        {
            get { return valCompetidor.ValidationGroup; }
            set { valCompetidor.ValidationGroup = value; }
        }

        #endregion

        #region Competidor elegido

        // Null si no se eligio ningun competidor
        public int? IdCompetencia
        {
            get
            {
                int id;
                return int.TryParse(ddlCompetidor.SelectedValue, out id) && id > 0 ? id : (int?)null;
            }
        }

        public string TextoSeleccionado
        {
            get { return IdCompetencia.HasValue ? ddlCompetidor.SelectedItem.Text : string.Empty; }
        }

        public int CantidadCompetidores
        {
            get { return ddlCompetidor.Items.Count - 1; }
        }

        #endregion

        #region Metodos publicos

        public void Cargar(IEnumerable<BECompetencia> competidores)
        {
            Hashtable busqueda = new Hashtable();

            ddlCompetidor.Items.Clear();
            ddlCompetidor.Items.Add(new ListItem(Traductor.T(TextoOpcionVacia), string.Empty));

            foreach (BECompetencia competidor in competidores)
            {
                string valor = competidor.IdCompetencia.ToString();
                string texto = MostrarMarketplace
                    ? string.Format("{0} ({1})", competidor.Nombre, competidor.Marketplace)
                    : competidor.Nombre;

                ddlCompetidor.Items.Add(new ListItem(texto, valor));
                busqueda[valor] = string.Join(" ", competidor.Nombre, competidor.Marketplace);
            }

            // Los atributos de las opciones no se guardan en el ViewState: el
            // texto de busqueda se guarda aparte y se vuelve a poner al dibujar
            ViewState["Busqueda"] = busqueda;
            txtBuscar.Text = string.Empty;
        }

        public void Limpiar()
        {
            ddlCompetidor.SelectedIndex = ddlCompetidor.Items.Count > 0 ? 0 : -1;
            txtBuscar.Text = string.Empty;
        }

        #endregion

        #region Eventos del componente

        protected override void OnPreRender(EventArgs e)
        {
            lblEtiqueta.Text = Traductor.T(Etiqueta);
            txtBuscar.Attributes["placeholder"] = Traductor.T("selector.buscarCompetidor");
            valCompetidor.ErrorMessage = Traductor.T(MensajeObligatorio);

            if (ddlCompetidor.Items.Count > 0)
            {
                ddlCompetidor.Items[0].Text = Traductor.T(TextoOpcionVacia);
            }

            Hashtable busqueda = ViewState["Busqueda"] as Hashtable ?? new Hashtable();

            foreach (ListItem item in ddlCompetidor.Items)
            {
                if (busqueda.ContainsKey(item.Value))
                {
                    item.Attributes["data-busqueda"] = Convert.ToString(busqueda[item.Value]);
                }
            }

            BuscadorDeSelector.Registrar(Page, txtBuscar, ddlCompetidor, lblCoincidencias);

            base.OnPreRender(e);
        }

        protected void ddlCompetidor_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CompetidorSeleccionado != null)
            {
                CompetidorSeleccionado(this, EventArgs.Empty);
            }
        }

        #endregion

        private string Texto(string clave, string porDefecto)
        {
            string texto = Convert.ToString(ViewState[clave]);
            return string.IsNullOrEmpty(texto) ? porDefecto : texto;
        }
    }
}
