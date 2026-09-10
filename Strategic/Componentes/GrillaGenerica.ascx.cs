using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Strategic.Componentes
{
    /// <summary>
    /// Componente reutilizable que representa una grilla de datos con paginado.
    /// La página contenedora define las columnas en su evento Page_Init, le pasa el
    /// origen de datos con Cargar() y se entera de las acciones sobre una fila
    /// mediante el evento AccionSeleccionada.
    /// </summary>
    public partial class GrillaGenerica : UserControl
    {
        // Cantidad de números de página que se muestran a la vez en el paginador
        private const int VentanaPaginas = 5;

        /// <summary>
        /// Se dispara cuando el usuario ejecuta una acción sobre una fila de la grilla.
        /// </summary>
        public event EventHandler<AccionGrillaEventArgs> AccionSeleccionada;

        #region Propiedades de configuracion

        /// <summary>
        /// Campos clave de la grilla, separados por coma (equivale a DataKeyNames).
        /// </summary>
        public string ClavePrimaria
        {
            get { return Convert.ToString(ViewState["ClavePrimaria"]); }
            set
            {
                ViewState["ClavePrimaria"] = value;
                gvDatos.DataKeyNames = SepararClaves(value);
            }
        }

        /// <summary>
        /// Cantidad de filas que se muestran por página.
        /// </summary>
        public int FilasPorPagina
        {
            get { return ViewState["FilasPorPagina"] == null ? 10 : Convert.ToInt32(ViewState["FilasPorPagina"]); }
            set { ViewState["FilasPorPagina"] = value < 1 ? 1 : value; }
        }

        /// <summary>
        /// Mensaje que se muestra cuando la consulta no devuelve registros.
        /// </summary>
        public string MensajeVacio
        {
            get
            {
                string mensaje = Convert.ToString(ViewState["MensajeVacio"]);
                return string.IsNullOrEmpty(mensaje) ? "No se encontraron registros" : mensaje;
            }
            set { ViewState["MensajeVacio"] = value; }
        }

        /// <summary>
        /// Indica si se muestra el selector de filas por página.
        /// </summary>
        public bool MostrarSelectorFilas
        {
            get { return ViewState["MostrarSelectorFilas"] == null || Convert.ToBoolean(ViewState["MostrarSelectorFilas"]); }
            set { ViewState["MostrarSelectorFilas"] = value; }
        }

        #endregion

        #region Propiedades de solo lectura

        /// <summary>
        /// Cantidad total de registros del último origen de datos cargado.
        /// </summary>
        public int CantidadRegistros
        {
            get { return ViewState["CantidadRegistros"] == null ? 0 : Convert.ToInt32(ViewState["CantidadRegistros"]); }
            private set { ViewState["CantidadRegistros"] = value; }
        }

        /// <summary>
        /// Página que se está mostrando, empezando en cero.
        /// </summary>
        public int PaginaActual
        {
            get { return ViewState["PaginaActual"] == null ? 0 : Convert.ToInt32(ViewState["PaginaActual"]); }
            private set { ViewState["PaginaActual"] = value; }
        }

        /// <summary>
        /// Cantidad total de páginas del último origen de datos cargado.
        /// </summary>
        public int TotalPaginas
        {
            get { return ViewState["TotalPaginas"] == null ? 0 : Convert.ToInt32(ViewState["TotalPaginas"]); }
            private set { ViewState["TotalPaginas"] = value; }
        }

        #endregion

        #region Configuracion de columnas

        /// <summary>
        /// Agrega una columna de datos a la grilla.
        /// </summary>
        public void AgregarColumna(string campo, string titulo)
        {
            AgregarColumna(campo, titulo, null);
        }

        /// <summary>
        /// Agrega una columna de datos a la grilla con una clase css para la celda.
        /// </summary>
        public void AgregarColumna(string campo, string titulo, string claseCss)
        {
            BoundField columna = new BoundField();
            columna.DataField = campo;
            columna.HeaderText = titulo;
            columna.HtmlEncode = true;

            if (!string.IsNullOrEmpty(claseCss))
            {
                columna.ItemStyle.CssClass = claseCss;

                // El encabezado lleva la misma clase para que acompanie la alineacion de la celda
                columna.HeaderStyle.CssClass = claseCss;
            }

            gvDatos.Columns.Add(columna);
        }

        /// <summary>
        /// Agrega una columna de datos aplicando un formato de salida.
        /// El formato usa la sintaxis de DataFormatString, por ejemplo "{0:dd/MM/yyyy}".
        /// </summary>
        public void AgregarColumna(string campo, string titulo, string claseCss, string formato)
        {
            BoundField columna = new BoundField();
            columna.DataField = campo;
            columna.HeaderText = titulo;
            columna.HtmlEncode = true;
            columna.DataFormatString = formato;

            if (!string.IsNullOrEmpty(claseCss))
            {
                columna.ItemStyle.CssClass = claseCss;

                // El encabezado lleva la misma clase para que acompanie la alineacion de la celda
                columna.HeaderStyle.CssClass = claseCss;
            }

            gvDatos.Columns.Add(columna);
        }

        /// <summary>
        /// Agrega una columna con un botón de acción por fila. El comando viaja
        /// hacia la página contenedora en el evento AccionSeleccionada.
        /// </summary>
        public void AgregarColumnaAccion(string titulo, string comando, string texto)
        {
            gvDatos.Columns.Add(CrearColumnaAccion(titulo, comando, texto));
        }

        /// <summary>
        /// Agrega una columna de acción cuyo texto sale de un campo del origen
        /// de datos, para cuando la acción depende del estado de la fila
        /// (por ejemplo "Dar de baja" o "Reactivar" según el usuario).
        /// </summary>
        public void AgregarColumnaAccion(string titulo, string comando, string texto, string campoTexto)
        {
            ButtonField columna = CrearColumnaAccion(titulo, comando, texto);

            // DataTextField tiene prioridad sobre Text: este ultimo queda como
            // respaldo por si la fila no trae valor en el campo
            columna.DataTextField = campoTexto;

            gvDatos.Columns.Add(columna);
        }

        private ButtonField CrearColumnaAccion(string titulo, string comando, string texto)
        {
            ButtonField columna = new ButtonField();
            columna.ButtonType = ButtonType.Link;
            columna.HeaderText = titulo;
            columna.CommandName = comando;
            columna.Text = texto;
            columna.CausesValidation = false;
            columna.ControlStyle.CssClass = "btn-fila";
            columna.ItemStyle.CssClass = "celda-accion";

            return columna;
        }

        /// <summary>
        /// Quita todas las columnas definidas.
        /// </summary>
        public void LimpiarColumnas()
        {
            gvDatos.Columns.Clear();
        }

        #endregion

        #region Carga de datos

        /// <summary>
        /// Carga el origen de datos en la grilla y se posiciona en la primera página.
        /// </summary>
        public void Cargar(object origenDatos)
        {
            Session[ClaveDatos] = origenDatos;
            PaginaActual = 0;
            Enlazar();
        }

        /// <summary>
        /// Carga el origen de datos indicando el mensaje a mostrar si no hay registros.
        /// </summary>
        public void Cargar(object origenDatos, string mensajeVacio)
        {
            MensajeVacio = mensajeVacio;
            Cargar(origenDatos);
        }

        /// <summary>
        /// Libera el origen de datos guardado para esta grilla.
        /// </summary>
        public void Limpiar()
        {
            Session.Remove(ClaveDatos);
            Cargar(null);
        }

        #endregion

        #region Eventos del componente

        protected void gvDatos_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int indiceFila;

            if (!int.TryParse(Convert.ToString(e.CommandArgument), out indiceFila))
            {
                return;
            }

            if (indiceFila < 0 || indiceFila >= gvDatos.DataKeys.Count)
            {
                return;
            }

            int indiceAbsoluto = gvDatos.PageIndex * gvDatos.PageSize + indiceFila;

            AccionGrillaEventArgs argumentos = new AccionGrillaEventArgs(
                e.CommandName,
                gvDatos.DataKeys[indiceFila],
                indiceAbsoluto,
                ObtenerElemento(indiceAbsoluto));

            if (AccionSeleccionada != null)
            {
                AccionSeleccionada(this, argumentos);
            }
        }

        protected void gvDatos_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
            {
                return;
            }

            foreach (TableCell celda in e.Row.Cells)
            {
                foreach (Control control in celda.Controls)
                {
                    LinkButton boton = control as LinkButton;

                    if (boton != null && boton.Text == "Dar de baja")
                    {
                        // El ButtonField vuelve a aplicar su ControlStyle (con la
                        // clase "btn-fila" comun) sobre el control despues de este
                        // evento, y eso pisa cualquier CssClass agregada aca. Por
                        // eso el estilo de peligro se marca con un atributo propio
                        // en lugar de agregar una clase.
                        boton.Attributes["data-boton-peligro"] = "true";
                    }
                }
            }
        }

        protected void Paginado_Command(object sender, CommandEventArgs e)
        {
            switch (e.CommandName)
            {
                case "Primera":
                    PaginaActual = 0;
                    break;
                case "Anterior":
                    PaginaActual = PaginaActual - 1;
                    break;
                case "Siguiente":
                    PaginaActual = PaginaActual + 1;
                    break;
                case "Ultima":
                    PaginaActual = TotalPaginas - 1;
                    break;
                case "Ir":
                    PaginaActual = Convert.ToInt32(e.CommandArgument);
                    break;
                default:
                    return;
            }

            Enlazar();
        }

        protected void ddlFilasPorPagina_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilasPorPagina = Convert.ToInt32(ddlFilasPorPagina.SelectedValue);
            PaginaActual = 0;
            Enlazar();
        }

        #endregion

        #region Metodos usados por el markup

        protected string TextoPagina(object numeroPagina)
        {
            return Convert.ToString(Convert.ToInt32(numeroPagina) + 1);
        }

        protected string ClasePagina(object numeroPagina)
        {
            return Convert.ToInt32(numeroPagina) == PaginaActual
                ? "pagina-boton pagina-actual"
                : "pagina-boton";
        }

        #endregion

        #region Metodos privados

        // Clave con la que se guarda el origen de datos de esta grilla en la Session.
        // Incluye el UniqueID para permitir mas de una grilla en la misma pagina.
        private string ClaveDatos
        {
            get { return string.Concat("GrillaGenerica_", Page.GetType().FullName, "_", UniqueID); }
        }

        private void Enlazar()
        {
            object origenDatos = Session[ClaveDatos];

            CantidadRegistros = Contar(origenDatos);
            TotalPaginas = CalcularTotalPaginas(CantidadRegistros);
            PaginaActual = AcotarPagina(PaginaActual);

            gvDatos.PageSize = FilasPorPagina;
            gvDatos.PageIndex = PaginaActual;
            gvDatos.DataSource = origenDatos;
            gvDatos.DataBind();

            if (gvDatos.HeaderRow != null)
            {
                gvDatos.HeaderRow.TableSection = TableRowSection.TableHeader;
            }

            bool hayDatos = CantidadRegistros > 0;

            gvDatos.Visible = hayDatos;
            pnlSinResultados.Visible = !hayDatos;
            lblSinResultados.Text = Server.HtmlEncode(MensajeVacio);

            ActualizarPaginado(hayDatos);
        }

        private void ActualizarPaginado(bool hayDatos)
        {
            pnlPaginado.Visible = hayDatos && (TotalPaginas > 1 || MostrarSelectorFilas);
            pnlFilasPorPagina.Visible = MostrarSelectorFilas;

            if (!pnlPaginado.Visible)
            {
                return;
            }

            int desde = PaginaActual * FilasPorPagina + 1;
            int hasta = Math.Min((PaginaActual + 1) * FilasPorPagina, CantidadRegistros);

            lblRango.Text = string.Format("Mostrando {0} - {1} de {2} registros", desde, hasta, CantidadRegistros);

            lnkPrimera.Enabled = PaginaActual > 0;
            lnkAnterior.Enabled = PaginaActual > 0;
            lnkSiguiente.Enabled = PaginaActual < TotalPaginas - 1;
            lnkUltima.Enabled = PaginaActual < TotalPaginas - 1;

            rptPaginas.DataSource = CalcularVentanaPaginas();
            rptPaginas.DataBind();

            ListItem seleccionado = ddlFilasPorPagina.Items.FindByValue(Convert.ToString(FilasPorPagina));

            if (seleccionado != null)
            {
                ddlFilasPorPagina.ClearSelection();
                seleccionado.Selected = true;
            }
        }

        // Devuelve los numeros de pagina visibles alrededor de la pagina actual
        private List<int> CalcularVentanaPaginas()
        {
            List<int> paginas = new List<int>();

            int inicio = Math.Max(0, PaginaActual - VentanaPaginas / 2);
            int fin = Math.Min(TotalPaginas - 1, inicio + VentanaPaginas - 1);

            inicio = Math.Max(0, fin - VentanaPaginas + 1);

            for (int pagina = inicio; pagina <= fin; pagina++)
            {
                paginas.Add(pagina);
            }

            return paginas;
        }

        private int CalcularTotalPaginas(int cantidadRegistros)
        {
            if (cantidadRegistros <= 0)
            {
                return 0;
            }

            return (int)Math.Ceiling((double)cantidadRegistros / FilasPorPagina);
        }

        private int AcotarPagina(int pagina)
        {
            if (pagina < 0 || TotalPaginas == 0)
            {
                return 0;
            }

            return pagina > TotalPaginas - 1 ? TotalPaginas - 1 : pagina;
        }

        private int Contar(object origenDatos)
        {
            ICollection coleccion = origenDatos as ICollection;

            if (coleccion != null)
            {
                return coleccion.Count;
            }

            IEnumerable enumerable = origenDatos as IEnumerable;

            if (enumerable == null)
            {
                return 0;
            }

            int cantidad = 0;

            foreach (object elemento in enumerable)
            {
                cantidad++;
            }

            return cantidad;
        }

        private object ObtenerElemento(int indiceAbsoluto)
        {
            IList lista = Session[ClaveDatos] as IList;

            if (lista == null || indiceAbsoluto < 0 || indiceAbsoluto >= lista.Count)
            {
                return null;
            }

            return lista[indiceAbsoluto];
        }

        private string[] SepararClaves(string clavePrimaria)
        {
            if (string.IsNullOrWhiteSpace(clavePrimaria))
            {
                return new string[0];
            }

            return clavePrimaria.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        }

        #endregion
    }

    /// <summary>
    /// Datos que la grilla le envía a la página contenedora cuando el usuario
    /// ejecuta una acción sobre una fila.
    /// </summary>
    public class AccionGrillaEventArgs : EventArgs
    {
        public AccionGrillaEventArgs(string comando, DataKey claves, int indice, object elemento)
        {
            Comando = comando;
            Claves = claves;
            Indice = indice;
            Elemento = elemento;
        }

        /// <summary>
        /// Nombre del comando definido en AgregarColumnaAccion.
        /// </summary>
        public string Comando { get; private set; }

        /// <summary>
        /// Claves de la fila seleccionada (los campos indicados en ClavePrimaria).
        /// </summary>
        public DataKey Claves { get; private set; }

        /// <summary>
        /// Posición de la fila dentro del origen de datos completo.
        /// </summary>
        public int Indice { get; private set; }

        /// <summary>
        /// Objeto de negocio enlazado a la fila seleccionada.
        /// </summary>
        public object Elemento { get; private set; }

        /// <summary>
        /// Devuelve el valor de una de las claves de la fila como texto.
        /// </summary>
        public string ObtenerClave(string nombreCampo)
        {
            if (Claves == null || Claves[nombreCampo] == null)
            {
                return string.Empty;
            }

            return Convert.ToString(Claves[nombreCampo]);
        }
    }
}
