using BE;
using BLL;
using Services;
using Strategic.Seguridad;
using System;
using System.Collections.Generic;

namespace Strategic
{
    // CU-009-043 - Baja Suscripcion
    public partial class BajaSuscripcion : PaginaSegura
    {
        private readonly BLLSuscripcion bllSuscripcion = new BLLSuscripcion();

        // Fecha hasta la que se mantiene el acceso, para armar la advertencia
        private DateTime? FinDeAcceso
        {
            get { return ViewState["FinDeAcceso"] as DateTime?; }
            set { ViewState["FinDeAcceso"] = value; }
        }

        public override void ActualizarIdioma()
        {
            Title = Traductor.T("bajaSuscripcion.titulo");
            btnConfirmar.Text = Traductor.T("bajaSuscripcion.confirmar");
            btnCancelar.Text = Traductor.T("comun.cancelar");
            btnVolver.Text = Traductor.T("bajaSuscripcion.volver");

            MostrarAdvertencia();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarSuscripcion();
            }
        }

        protected void btnConfirmar_Click(object sender, EventArgs e)
        {
            BEUsuario enSesion = SessionManager.UsuarioActual;
            DateTime finDeAcceso;

            try
            {
                finDeAcceso = bllSuscripcion.CancelarSuscripcion(campoClave.Clave, enSesion.NombreUsuario);
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(ex.Message);
                return;
            }

            string mensaje = Traductor.T("bajaSuscripcion.exito", finDeAcceso.ToString("dd/MM/yyyy"));

            // La baja ya quedo guardada: si un mail falla solo se muestra el error y se sigue
            string errores = string.Empty;

            try
            {
                EnviarConfirmacion(finDeAcceso);
                mensaje += " " + Traductor.T("bajaSuscripcion.mailEnviado", enSesion.Email);
            }
            catch (Exception)
            {
                errores += Traductor.T("error.mailBajaNoEnviado") + " ";
            }

            try
            {
                AvisarStrategic(finDeAcceso);
            }
            catch (Exception)
            {
                errores += Traductor.T("error.avisoStrategicNoEnviado");
            }

            if (errores != string.Empty)
            {
                Session["ErrorSuscripcion"] = errores.Trim();
            }

            Session["MensajeSuscripcion"] = mensaje;
            Response.Redirect("~/RenovarSuscripcion.aspx");
        }

        // CU-009-043, paso 4: confirmacion de la baja al usuario
        private void EnviarConfirmacion(DateTime finDeAcceso)
        {
            BEUsuario enSesion = SessionManager.UsuarioActual;

            MailService.EnviarMail(enSesion.Email,
                Traductor.T("mail.baja.asunto"),
                Traductor.T("mail.baja.cuerpo", enSesion.Nombre, finDeAcceso.ToString("dd/MM/yyyy")),
                null,
                null);
        }

        // CU-009-043, paso 4: aviso al equipo de Strategic. Va siempre en español
        // porque no lo lee el cliente
        private void AvisarStrategic(DateTime finDeAcceso)
        {
            BEUsuario enSesion = SessionManager.UsuarioActual;

            string asunto = string.Format("Baja de suscripción - acceso hasta el {0:dd/MM/yyyy}", finDeAcceso);

            string cuerpo = string.Format(
                "Se registró la baja de una suscripción. El cliente conserva el acceso al sistema hasta que finaliza el período abonado.\n\n" +
                "Instancia: {0}\n" +
                "Cliente: {1} {2} ({3}) - {4}\n" +
                "Fecha de la baja: {5:dd/MM/yyyy HH:mm}\n" +
                "Acceso hasta: {6:dd/MM/yyyy}",
                Request.Url.GetLeftPart(UriPartial.Authority),
                enSesion.Nombre,
                enSesion.Apellido,
                enSesion.NombreUsuario,
                enSesion.Email,
                DateTime.Now,
                finDeAcceso);

            MailService.EnviarMail(MailService.MailStrategic, asunto, cuerpo, null, null);
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/RenovarSuscripcion.aspx");
        }

        private void CargarSuscripcion()
        {
            try
            {
                List<BESuscripcion> suscripciones = bllSuscripcion.TraerListaSuscripciones();

                // Alternativa 2.1 [#ERR047] y suscripcion sin periodo activo
                bllSuscripcion.ValidarBaja(suscripciones);

                FinDeAcceso = bllSuscripcion.CalcularFinDeAcceso(suscripciones);
                MostrarAdvertencia();
            }
            catch (Exception ex)
            {
                lblNoDisponible.Text = Server.HtmlEncode(ex.Message);

                pnlConfirmacion.Visible = false;
                pnlNoDisponible.Visible = true;
            }
        }

        // Paso 2: el acceso sigue hasta el vencimiento ya pagado
        private void MostrarAdvertencia()
        {
            if (FinDeAcceso.HasValue)
            {
                lblAdvertencia.Text = Server.HtmlEncode(Traductor.T(
                    "bajaSuscripcion.advertencia",
                    FinDeAcceso.Value.ToString("dd/MM/yyyy")));
            }
        }
    }
}
