using BE.Composite;
using Services;
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Strategic.Componentes
{
    
    /// Componente reutilizable que arma la composición de un rol o de una
    /// familia: a la izquierda los permisos simples y familias disponibles,
    /// a la derecha lo que se le va asignando.
    ///
    /// Trabaja contra BEComponente, así que no distingue entre hojas y
    /// familias en ningún momento. Lo único que consulta del árbol es
    /// Contiene(), para avisar cuando un permiso que se está por agregar ya
    /// venía incluido dentro de una familia elegida.
    
    public partial class SelectorPermisos : UserControl, IIdiomaObserver
    {
        
        /// Componentes disponibles con su subárbol cargado, guardados en la
        /// Session igual que hace GrillaGenerica con su origen de datos.
        
        private Dictionary<int, BEComponente> Catalogo
        {
            get
            {
                Dictionary<int, BEComponente> catalogo = Session[ClaveCatalogo] as Dictionary<int, BEComponente>;

                return catalogo ?? new Dictionary<int, BEComponente>();
            }
            set { Session[ClaveCatalogo] = value; }
        }

        private string ClaveCatalogo
        {
            get { return string.Concat("SelectorPermisos_", Page.GetType().FullName, "_", UniqueID); }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            Traductor.Suscribir(this);
            ActualizarIdioma();
        }

        public void ActualizarIdioma()
        {
            lblTituloDisponibles.Text = Traductor.T("selectorPermisos.disponibles");
            lblAyudaDisponibles.Text = Traductor.T("selectorPermisos.ayuda");
            lblTituloSeleccionados.Text = Traductor.T("selectorPermisos.composicion");
            btnAgregar.Text = Traductor.T("selectorPermisos.agregar");
            btnQuitar.Text = Traductor.T("selectorPermisos.quitar");

            // Las listas ya cargadas guardan su texto en el ViewState: se
            // vuelven a armar las etiquetas a partir del catálogo
            Dictionary<int, BEComponente> catalogo = Catalogo;

            foreach (ListBox lista in new ListBox[] { lstDisponibles, lstSeleccionados })
            {
                foreach (ListItem item in lista.Items)
                {
                    int codigo = Convert.ToInt32(item.Value);

                    if (catalogo.ContainsKey(codigo))
                    {
                        item.Text = CrearEtiqueta(catalogo[codigo]);
                    }
                }
            }

            ActualizarResumen();
        }


        /// Cantidad de componentes elegidos.
        
        public int CantidadSeleccionados
        {
            get { return lstSeleccionados.Items.Count; }
        }

        
        /// Carga el componente. "disponibles" tiene que venir con el subárbol
        /// de cada elemento armado, para poder detectar los que ya están
        /// incluidos dentro de una familia.
        
        public void Cargar(List<BEComponente> disponibles, List<BEComponente> seleccionados)
        {
            Dictionary<int, BEComponente> catalogo = new Dictionary<int, BEComponente>();

            lstDisponibles.Items.Clear();
            lstSeleccionados.Items.Clear();

            foreach (BEComponente componente in disponibles)
            {
                catalogo[componente.CodPermiso] = componente;
            }

            List<int> elegidos = new List<int>();

            if (seleccionados != null)
            {
                foreach (BEComponente componente in seleccionados)
                {
                    elegidos.Add(componente.CodPermiso);

                    // Un componente ya asignado puede haber quedado inactivo:
                    // se muestra igual para no perderlo sin avisar
                    if (!catalogo.ContainsKey(componente.CodPermiso))
                    {
                        catalogo[componente.CodPermiso] = componente;
                    }

                    lstSeleccionados.Items.Add(CrearItem(catalogo[componente.CodPermiso]));
                }
            }

            foreach (BEComponente componente in disponibles)
            {
                if (!elegidos.Contains(componente.CodPermiso))
                {
                    lstDisponibles.Items.Add(CrearItem(componente));
                }
            }

            Catalogo = catalogo;

            ActualizarResumen();
        }

        
        /// Quita del listado de disponibles un componente puntual. Se usa al
        /// modificar una familia, que no puede contenerse a sí misma.
        
        public void ExcluirDeDisponibles(int codPermiso)
        {
            ListItem item = lstDisponibles.Items.FindByValue(codPermiso.ToString());

            if (item != null)
            {
                lstDisponibles.Items.Remove(item);
            }
        }

        
        /// Códigos de los componentes elegidos, en el orden en que se ven.
        
        public List<int> ObtenerSeleccionados()
        {
            List<int> seleccionados = new List<int>();

            foreach (ListItem item in lstSeleccionados.Items)
            {
                int codigo;

                if (int.TryParse(item.Value, out codigo))
                {
                    seleccionados.Add(codigo);
                }
            }

            return seleccionados;
        }

        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            ListItem elegido = lstDisponibles.SelectedItem;

            if (elegido == null)
            {
                lblMensaje.Text = Traductor.T("selectorPermisos.elegirDisponible");
                return;
            }

            string yaIncluido = BuscarQuienLoIncluye(Convert.ToInt32(elegido.Value));

            if (yaIncluido != null)
            {
                lblMensaje.Text = Server.HtmlEncode(Traductor.T("selectorPermisos.yaIncluido", elegido.Text, yaIncluido));
                return;
            }

            lstDisponibles.Items.Remove(elegido);
            lstSeleccionados.Items.Add(elegido);

            ActualizarResumen();
        }

        protected void btnQuitar_Click(object sender, EventArgs e)
        {
            ListItem elegido = lstSeleccionados.SelectedItem;

            if (elegido == null)
            {
                lblMensaje.Text = Traductor.T("selectorPermisos.elegirSeleccionado");
                return;
            }

            lstSeleccionados.Items.Remove(elegido);

            // Vuelve a la lista de disponibles solo si sigue estando vigente
            if (Catalogo.ContainsKey(Convert.ToInt32(elegido.Value)))
            {
                lstDisponibles.Items.Add(elegido);
            }

            ActualizarResumen();
        }

        
        /// Devuelve el nombre del componente ya elegido que contiene al que se
        /// está por agregar, o null si no hay ninguno. Es la consulta que
        /// resuelve el propio Composite recorriendo su árbol.
        
        private string BuscarQuienLoIncluye(int codPermiso)
        {
            Dictionary<int, BEComponente> catalogo = Catalogo;

            foreach (ListItem item in lstSeleccionados.Items)
            {
                int codElegido = Convert.ToInt32(item.Value);

                if (!catalogo.ContainsKey(codElegido))
                {
                    continue;
                }

                BEComponente elegido = catalogo[codElegido];

                if (elegido.EsFamilia && elegido.Contiene(codPermiso))
                {
                    return elegido.Nombre;
                }
            }

            return null;
        }

        private ListItem CrearItem(BEComponente componente)
        {
            return new ListItem(CrearEtiqueta(componente), componente.CodPermiso.ToString());
        }

        private string CrearEtiqueta(BEComponente componente)
        {
            string etiqueta = componente.EsFamilia
                ? Traductor.T("selectorPermisos.familia", componente.Nombre, componente.CantidadPermisosSimples)
                : componente.Nombre;

            if (!componente.Activo)
            {
                etiqueta = etiqueta + " - " + Traductor.T("selectorPermisos.dadoDeBaja");
            }

            return etiqueta;
        }

        private void ActualizarResumen()
        {
            if (lstSeleccionados.Items.Count == 0)
            {
                lblResumen.Text = Traductor.T("selectorPermisos.sinElementos");
                return;
            }

            // Se cuentan los permisos simples que quedan alcanzados, que es lo
            // que realmente va a habilitar el rol o la familia
            List<int> alcanzados = new List<int>();
            Dictionary<int, BEComponente> catalogo = Catalogo;

            foreach (ListItem item in lstSeleccionados.Items)
            {
                int codigo = Convert.ToInt32(item.Value);

                if (!catalogo.ContainsKey(codigo))
                {
                    continue;
                }

                foreach (BEPermiso permiso in catalogo[codigo].ObtenerPermisosSimples())
                {
                    if (!alcanzados.Contains(permiso.CodPermiso))
                    {
                        alcanzados.Add(permiso.CodPermiso);
                    }
                }
            }

            lblResumen.Text = Traductor.T("selectorPermisos.resumen",
                lstSeleccionados.Items.Count,
                lstSeleccionados.Items.Count == 1 ? string.Empty : "s",
                alcanzados.Count,
                alcanzados.Count == 1 ? string.Empty : "s");
        }
    }
}
