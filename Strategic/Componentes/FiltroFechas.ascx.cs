using Services;
using System;
using System.Globalization;
using System.Web.UI;

namespace Strategic.Componentes
{
    
    /// Componente reutilizable que representa un selector de rango de fechas
    /// compuesto por dos campos de fecha (Desde y Hasta) y un botón de aplicar filtro.
    /// Expone las fechas seleccionadas como propiedades públicas y notifica a la
    /// página contenedora mediante el evento FiltroAplicado cuando el usuario confirma
    /// el filtrado.
    ///
    /// Las etiquetas se reciben como claves de traducción y se traducen en el
    /// PreRender, que corre después de un posible cambio de idioma.

    public partial class FiltroFechas : UserControl
    {
        // Formato que envía el navegador en los campos con TextMode="Date"
        private const string FormatoFecha = "yyyy-MM-dd";

        
        /// Se dispara cuando el usuario confirma el filtrado con el botón del componente.
        /// Solo se dispara si el rango seleccionado es válido.
        
        public event EventHandler<RangoFechasEventArgs> FiltroAplicado;

        #region Propiedades de configuracion

        
        /// Clave de la etiqueta del campo Desde.
        
        public string EtiquetaDesde
        {
            get
            {
                string etiqueta = Convert.ToString(ViewState["EtiquetaDesde"]);
                return string.IsNullOrEmpty(etiqueta) ? "filtroFechas.desde" : etiqueta;
            }
            set { ViewState["EtiquetaDesde"] = value; }
        }

        
        /// Clave de la etiqueta del campo Hasta.
        
        public string EtiquetaHasta
        {
            get
            {
                string etiqueta = Convert.ToString(ViewState["EtiquetaHasta"]);
                return string.IsNullOrEmpty(etiqueta) ? "filtroFechas.hasta" : etiqueta;
            }
            set { ViewState["EtiquetaHasta"] = value; }
        }

        
        /// Clave del texto del botón de aplicar filtro.
        
        public string TextoBoton
        {
            get
            {
                string texto = Convert.ToString(ViewState["TextoBoton"]);
                return string.IsNullOrEmpty(texto) ? "filtroFechas.aplicar" : texto;
            }
            set { ViewState["TextoBoton"] = value; }
        }

        
        /// Indica si el componente muestra su propio botón de aplicar filtro.
        /// Se pone en false cuando la página contenedora ya tiene un botón que
        /// dispara el filtrado junto con otros campos.
        
        public bool MostrarBoton
        {
            get { return ViewState["MostrarBoton"] == null || Convert.ToBoolean(ViewState["MostrarBoton"]); }
            set { ViewState["MostrarBoton"] = value; }
        }

        #endregion

        #region Fechas seleccionadas

        
        /// Fecha desde seleccionada, o null si el campo está vacío.
        
        public DateTime? FechaDesde
        {
            get { return ConvertirFecha(txtDesde.Text); }
            set { txtDesde.Text = FormatearFecha(value); }
        }

        
        /// Fecha hasta seleccionada, o null si el campo está vacío.
        
        public DateTime? FechaHasta
        {
            get { return ConvertirFecha(txtHasta.Text); }
            set { txtHasta.Text = FormatearFecha(value); }
        }

        
        /// Indica si el rango seleccionado es coherente (la fecha hasta no es
        /// anterior a la fecha desde).
        
        public bool EsRangoValido
        {
            get
            {
                DateTime? desde = FechaDesde;
                DateTime? hasta = FechaHasta;

                if (!desde.HasValue || !hasta.HasValue)
                {
                    return true;
                }

                return hasta.Value >= desde.Value;
            }
        }

        #endregion

        #region Metodos publicos

        
        /// Valida el rango y muestra el mensaje de error dentro del componente
        /// si la fecha hasta es anterior a la fecha desde.
        
        public bool Validar()
        {
            if (EsRangoValido)
            {
                OcultarError();
                return true;
            }

            MostrarError(Traductor.T("filtroFechas.errorRango"));
            return false;
        }

        
        /// Establece el rango de fechas del componente.
        
        public void EstablecerRango(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            FechaDesde = fechaDesde;
            FechaHasta = fechaHasta;
            OcultarError();
        }

        
        /// Vacía los dos campos de fecha.
        
        public void Limpiar()
        {
            EstablecerRango(null, null);
        }

        #endregion

        #region Eventos del componente

        protected override void OnPreRender(EventArgs e)
        {
            lblDesde.Text = Traductor.T(EtiquetaDesde);
            lblHasta.Text = Traductor.T(EtiquetaHasta);
            btnAplicar.Text = Traductor.T(TextoBoton);
            pnlAccion.Visible = MostrarBoton;

            base.OnPreRender(e);
        }

        protected void btnAplicar_Click(object sender, EventArgs e)
        {
            if (!Validar())
            {
                return;
            }

            if (FiltroAplicado != null)
            {
                FiltroAplicado(this, new RangoFechasEventArgs(FechaDesde, FechaHasta));
            }
        }

        #endregion

        #region Metodos privados

        private void MostrarError(string mensaje)
        {
            lblErrorRango.Text = Server.HtmlEncode(mensaje);
            pnlErrorRango.Visible = true;
        }

        private void OcultarError()
        {
            lblErrorRango.Text = string.Empty;
            pnlErrorRango.Visible = false;
        }

        private DateTime? ConvertirFecha(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return null;
            }

            DateTime fecha;

            // El navegador envía siempre yyyy-MM-dd; el TryParse queda como respaldo
            // para los navegadores que no soportan TextMode="Date"
            if (DateTime.TryParseExact(valor.Trim(), FormatoFecha, CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha))
            {
                return fecha;
            }

            if (DateTime.TryParse(valor.Trim(), out fecha))
            {
                return fecha;
            }

            return null;
        }

        private string FormatearFecha(DateTime? fecha)
        {
            return fecha.HasValue ? fecha.Value.ToString(FormatoFecha) : string.Empty;
        }

        #endregion
    }

    
    /// Datos que el filtro le envía a la página contenedora cuando el usuario
    /// confirma el filtrado.
    
    public class RangoFechasEventArgs : EventArgs
    {
        public RangoFechasEventArgs(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            FechaDesde = fechaDesde;
            FechaHasta = fechaHasta;
        }

        
        /// Fecha desde seleccionada, o null si el campo está vacío.
        
        public DateTime? FechaDesde { get; private set; }

        
        /// Fecha hasta seleccionada, o null si el campo está vacío.
        
        public DateTime? FechaHasta { get; private set; }
    }
}
