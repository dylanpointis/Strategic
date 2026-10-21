using BE;
using BLL;
using Newtonsoft.Json;
using Services;
using Strategic.Seguridad;
using System;
using System.Collections.Generic;

namespace Strategic
{
    // CU-009-042 - Renovar Suscripcion
    public partial class RenovarSuscripcion : PaginaSegura
    {
        private const string MensajeSinHistorial = "renovarSuscripcion.sinHistorial";

        private readonly BLLSuscripcion bllSuscripcion = new BLLSuscripcion();

        // Texto que lleva el QR de pago. Es de prueba: no apunta a una cuenta real
        protected string TextoQrJson
        {
            get
            {
                return SerializarTexto(Traductor.T(
                    "renovarSuscripcion.textoQr",
                    BLLSuscripcion.TitularStrategic,
                    BLLSuscripcion.AliasStrategic,
                    lblRenovacionMonto.Text));
            }
        }

        protected string ErrorQrJson
        {
            get { return SerializarTexto(Traductor.T("error.qrNoGenerado")); }
        }

        public override void ActualizarIdioma()
        {
            Title = Traductor.T("renovarSuscripcion.titulo");
            btnRenovar.Text = Traductor.T("renovarSuscripcion.renovar");
            btnDarDeBaja.Text = Traductor.T("renovarSuscripcion.darDeBaja");
            lblNumeroOperacion.Text = Traductor.T("suscripcion.numeroOperacion");
            valNumeroOperacion.ErrorMessage = Traductor.T("error.debeCompletar", Traductor.T("campo.elNumeroOperacion"));
            btnConfirmar.Text = Traductor.T("renovarSuscripcion.confirmarPago");
            btnCancelar.Text = Traductor.T("comun.cancelar");
            lblSinSuscripcion.Text = Traductor.T("error.sinSuscripcion");

            // La grilla guarda la cantidad en su ViewState
            MostrarCantidad();

            // El estado se conoce recien al recuperar el ViewState: solo se
            // vuelve a traducir si el idioma cambia despues de eso
            if (EstadoActual != null)
            {
                lblEstadoActual.Text = Traductor.T(EstadoActual);
            }
        }

        // Clave de traduccion del estado de la suscripcion que se esta mostrando
        private string EstadoActual
        {
            get { return ViewState["EstadoActual"] as string; }
            set { ViewState["EstadoActual"] = value; }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            ConfigurarGrilla();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarSuscripcion();

                if (SessionManager.SuscripcionVencida)
                {
                    lblAvisoVencida.Text = Server.HtmlEncode(Traductor.T("renovarSuscripcion.avisoVencida"));
                }

                // Mensaje que deja la baja (o la renovacion de una suscripcion vencida) al volver
                if (Session["MensajeSuscripcion"] != null)
                {
                    lblExito.Text = Server.HtmlEncode(Session["MensajeSuscripcion"].ToString());
                    Session.Remove("MensajeSuscripcion");
                }
            }
        }

        protected void btnRenovar_Click(object sender, EventArgs e)
        {
            try
            {
                List<BESuscripcion> suscripciones = bllSuscripcion.TraerListaSuscripciones();
                BESuscripcion renovacion = bllSuscripcion.ArmarRenovacion(suscripciones);

                lblRenovacionPlan.Text = Server.HtmlEncode(renovacion.Plan);
                lblRenovacionMonto.Text = string.Format("${0:N2}", renovacion.Monto);
                lblRenovacionVencimiento.Text = renovacion.FechaVencimiento.ToString("dd/MM/yyyy");
                lblCbu.Text = BLLSuscripcion.CbuStrategic;
                lblAlias.Text = BLLSuscripcion.AliasStrategic;
                lblTitular.Text = Server.HtmlEncode(BLLSuscripcion.TitularStrategic);

                txtNumeroOperacion.Text = string.Empty;
                pnlRenovacion.Visible = true;
                btnRenovar.Visible = false;
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(ex.Message);
            }
        }

        protected void btnConfirmar_Click(object sender, EventArgs e)
        {
            try
            {
                BEUsuario enSesion = SessionManager.UsuarioActual;

                BESuscripcion renovacion = bllSuscripcion.RenovarSuscripcion(txtNumeroOperacion.Text, enSesion.NombreUsuario);

                // Con la renovacion vuelve el acceso al resto del sistema
                if (SessionManager.SuscripcionVencida)
                {
                    SessionManager.SuscripcionVencida = false;
                    Session["MensajeSuscripcion"] = Traductor.T(
                        "renovarSuscripcion.exito",
                        renovacion.FechaVencimiento.ToString("dd/MM/yyyy"));

                    // Se recarga para que el menu vuelva a mostrar todas las opciones
                    Response.Redirect("~/RenovarSuscripcion.aspx");
                    return;
                }

                pnlRenovacion.Visible = false;
                btnRenovar.Visible = true;
                CargarSuscripcion();

                lblExito.Text = Server.HtmlEncode(Traductor.T(
                    "renovarSuscripcion.exito",
                    renovacion.FechaVencimiento.ToString("dd/MM/yyyy")));
            }
            catch (Exception ex)
            {
                lblErrorRenovacion.Text = Server.HtmlEncode(ex.Message);
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            pnlRenovacion.Visible = false;
            btnRenovar.Visible = true;
        }

        protected void btnDarDeBaja_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/BajaSuscripcion.aspx");
        }

        private void ConfigurarGrilla()
        {
            grillaSuscripciones.ClavePrimaria = "IdSuscripcion";
            grillaSuscripciones.FilasPorPagina = 10;

            grillaSuscripciones.AgregarColumna("FechaPago", "suscripcion.fechaPago", "celda-fecha", "{0:dd/MM/yyyy HH:mm}");
            grillaSuscripciones.AgregarColumna("Periodo", "suscripcion.periodo", "celda-fecha");
            grillaSuscripciones.AgregarColumna("Plan", "suscripcion.plan");
            grillaSuscripciones.AgregarColumna("Monto", "suscripcion.monto", "celda-numero", "${0:N2}");
            grillaSuscripciones.AgregarColumnaTraducida("EstadoTexto", "comun.estado");
            grillaSuscripciones.AgregarColumna("NumeroOperacionEnmascarado", "suscripcion.numeroOperacion", "celda-codigo");
        }

        private void CargarSuscripcion()
        {
            try
            {
                List<BESuscripcion> suscripciones = bllSuscripcion.TraerListaSuscripciones();
                BESuscripcion actual = bllSuscripcion.TraerSuscripcionActual(suscripciones);

                // Alternativa 2.1 [#ERR045]
                if (actual == null)
                {
                    pnlSuscripcion.Visible = false;
                    pnlSinSuscripcion.Visible = true;
                    return;
                }

                EstadoActual = actual.EstadoTexto;
                lblEstadoActual.Text = Traductor.T(actual.EstadoTexto);
                lblPlan.Text = Server.HtmlEncode(actual.Plan);
                lblFechaInicio.Text = actual.FechaInicio.ToString("dd/MM/yyyy");
                lblFechaVencimiento.Text = actual.FechaVencimiento.ToString("dd/MM/yyyy");
                lblMonto.Text = string.Format("${0:N2}", actual.Monto);

                // La baja es solo del WebMaster y solo tiene sentido con un periodo activo
                btnDarDeBaja.Visible = TienePermiso("BajaSuscripcion") && actual.Activa;

                grillaSuscripciones.Cargar(suscripciones, MensajeSinHistorial);
                MostrarCantidad();
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(ex.Message);
            }
        }

        private void MostrarCantidad()
        {
            int cantidad = grillaSuscripciones.CantidadRegistros;

            lblCantidad.Text = cantidad == 1
                ? Traductor.T("renovarSuscripcion.cantidadUno")
                : Traductor.T("renovarSuscripcion.cantidad", cantidad);
        }

        // Escapa < > & para poder incrustar el texto dentro de un <script>
        private string SerializarTexto(string texto)
        {
            JsonSerializerSettings ajustes = new JsonSerializerSettings();
            ajustes.StringEscapeHandling = StringEscapeHandling.EscapeHtml;

            return JsonConvert.SerializeObject(texto, ajustes);
        }
    }
}
