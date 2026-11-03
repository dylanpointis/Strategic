using BE;
using Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Strategic.Componentes
{
    // Componente reutilizable para elegir un producto sincronizado. Tiene un
    // buscador que filtra el desplegable mientras se escribe, por codigo,
    // nombre, categoria o marca. Expone el producto elegido en IdProducto y
    // avisa con ProductoSeleccionado cuando cambia (si AutoPostBack esta activo).
    // Los textos se reciben como claves de traduccion y se traducen en el
    // PreRender, que corre despues de un posible cambio de idioma
    public partial class SelectorProducto : UserControl
    {
        public event EventHandler ProductoSeleccionado;

        #region Propiedades de configuracion

        public string Etiqueta
        {
            get { return Texto("Etiqueta", "monitoreo.producto"); }
            set { ViewState["Etiqueta"] = value; }
        }

        // Clave del texto de la primera opcion, la que no elige ningun producto
        public string TextoOpcionVacia
        {
            get { return Texto("TextoOpcionVacia", "monitoreo.todosLosProductos"); }
            set { ViewState["TextoOpcionVacia"] = value; }
        }

        public bool AutoPostBack
        {
            get { return ddlProducto.AutoPostBack; }
            set { ddlProducto.AutoPostBack = value; }
        }

        // Si es obligatorio, valida con un RequiredFieldValidator del grupo indicado
        public bool Obligatorio
        {
            get { return valProducto.Enabled; }
            set { valProducto.Enabled = value; }
        }

        public string MensajeObligatorio
        {
            get { return Texto("MensajeObligatorio", "selector.productoRequerido"); }
            set { ViewState["MensajeObligatorio"] = value; }
        }

        public string ValidationGroup
        {
            get { return valProducto.ValidationGroup; }
            set { valProducto.ValidationGroup = value; }
        }

        #endregion

        #region Producto elegido

        // Null si no se eligio ningun producto
        public int? IdProducto
        {
            get
            {
                int id;
                return int.TryParse(ddlProducto.SelectedValue, out id) && id > 0 ? id : (int?)null;
            }
        }

        // "Codigo - Nombre" del producto elegido, o vacio
        public string TextoSeleccionado
        {
            get { return IdProducto.HasValue ? ddlProducto.SelectedItem.Text : string.Empty; }
        }

        public int CantidadProductos
        {
            get { return ddlProducto.Items.Count - 1; }
        }

        #endregion

        #region Metodos publicos

        public void Cargar(IEnumerable<BEProducto> productos)
        {
            Hashtable busqueda = new Hashtable();

            ddlProducto.Items.Clear();
            ddlProducto.Items.Add(new ListItem(Traductor.T(TextoOpcionVacia), string.Empty));

            foreach (BEProducto producto in productos)
            {
                string valor = producto.IdProducto.ToString();

                ddlProducto.Items.Add(new ListItem(string.Format("{0} - {1}", producto.Codigo, producto.Nombre), valor));
                busqueda[valor] = string.Join(" ", producto.Codigo, producto.Nombre, producto.Categoria, producto.Marca);
            }

            // Los atributos de las opciones no se guardan en el ViewState: el
            // texto de busqueda se guarda aparte y se vuelve a poner al dibujar
            ViewState["Busqueda"] = busqueda;
            txtBuscar.Text = string.Empty;
        }

        public void Limpiar()
        {
            ddlProducto.SelectedIndex = ddlProducto.Items.Count > 0 ? 0 : -1;
            txtBuscar.Text = string.Empty;
        }

        #endregion

        #region Eventos del componente

        protected override void OnPreRender(EventArgs e)
        {
            lblEtiqueta.Text = Traductor.T(Etiqueta);
            txtBuscar.Attributes["placeholder"] = Traductor.T("selector.buscarProducto");
            valProducto.ErrorMessage = Traductor.T(MensajeObligatorio);

            if (ddlProducto.Items.Count > 0)
            {
                ddlProducto.Items[0].Text = Traductor.T(TextoOpcionVacia);
            }

            Hashtable busqueda = ViewState["Busqueda"] as Hashtable ?? new Hashtable();

            foreach (ListItem item in ddlProducto.Items)
            {
                if (busqueda.ContainsKey(item.Value))
                {
                    item.Attributes["data-busqueda"] = Convert.ToString(busqueda[item.Value]);
                }
            }

            BuscadorDeSelector.Registrar(Page, txtBuscar, ddlProducto, lblCoincidencias);

            base.OnPreRender(e);
        }

        protected void ddlProducto_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ProductoSeleccionado != null)
            {
                ProductoSeleccionado(this, EventArgs.Empty);
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
