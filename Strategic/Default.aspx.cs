using Services;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web.UI;

namespace Strategic
{
    // Pantalla de entrada del sitio. El usuario logueado va directo al Dashboard
    // (CU-002-004) y el que no tiene sesion ve la bienvenida de Strategic
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (SessionManager.HayUsuarioLogueado)
            {
                Response.Redirect("~/Dashboard.aspx");
                return;
            }
        }

        // En PreRender para que tome el idioma elegido en el postback del selector
        protected void Page_PreRender(object sender, EventArgs e)
        {
            Title = Traductor.T("bienvenida.tituloPagina");

            lblNombre.Text = Traductor.T("usuario.nombre");
            lblEmpresa.Text = Traductor.T("bienvenida.contacto.empresa");
            lblEmail.Text = Traductor.T("usuario.email");
            lblTelefono.Text = Traductor.T("bienvenida.contacto.telefono");
            lblPlan.Text = Traductor.T("bienvenida.contacto.plan");
            lblMensaje.Text = Traductor.T("bienvenida.contacto.mensaje");
            btnEnviar.Text = Traductor.T("bienvenida.contacto.boton");

            // El valor queda fijo en español porque es lo que lee Strategic en el mail
            ddlPlan.Items[0].Text = Traductor.T("bienvenida.contacto.planADefinir");
            ddlPlan.Items[1].Text = Traductor.T("bienvenida.plan.basico");
            ddlPlan.Items[2].Text = Traductor.T("bienvenida.plan.pro");
            ddlPlan.Items[3].Text = Traductor.T("bienvenida.plan.max");
        }

        protected void btnEnviar_Click(object sender, EventArgs e)
        {
            // Despues del postback se vuelve al formulario para que se vea el resultado
            ClientScript.RegisterStartupScript(GetType(), "IrAContacto", "document.getElementById('contacto').scrollIntoView();", true);

            try
            {
                SolicitarCotizacion(txtNombre.Text, txtEmpresa.Text, txtEmail.Text, txtTelefono.Text, ddlPlan.SelectedValue, txtMensaje.Text);

                lblExito.Text = Server.HtmlEncode(Traductor.T("bienvenida.contacto.exito"));

                txtNombre.Text = string.Empty;
                txtEmpresa.Text = string.Empty;
                txtEmail.Text = string.Empty;
                txtTelefono.Text = string.Empty;
                txtMensaje.Text = string.Empty;
                ddlPlan.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(ex.Message);
            }
        }

        // La consulta no se guarda en la base: solo se manda por mail al equipo
        // de Strategic, con una confirmacion al interesado
        private void SolicitarCotizacion(string nombre, string empresa, string email, string telefono, string plan, string mensaje)
        {
            ExigirTexto(nombre, "campo.elNombre", 100);
            ExigirTexto(empresa, "campo.laEmpresa", 100);
            ExigirTexto(email, "campo.elEmail", 100);
            ExigirTexto(mensaje, "campo.elMensaje", 2000);

            if (!Regex.IsMatch(email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$"))
            {
                throw new Exception(Traductor.T("usuario.formatoEmail"));
            }

            // Al equipo de Strategic, siempre en español
            string cuerpo = string.Format(
                "Se recibió una solicitud de cotización desde el sitio de Strategic.\n\n" +
                "Nombre: {0}\n" +
                "Empresa: {1}\n" +
                "Email: {2}\n" +
                "Teléfono: {3}\n" +
                "Plan de interés: {4}\n\n" +
                "Mensaje:\n{5}",
                nombre.Trim(),
                empresa.Trim(),
                email.Trim(),
                string.IsNullOrWhiteSpace(telefono) ? "-" : telefono.Trim(),
                plan,
                mensaje.Trim());

            try
            {
                MailService.EnviarMail(MailService.MailStrategic, "Solicitud de cotización - " + empresa.Trim(), cuerpo, null, null);
            }
            catch (Exception)
            {
                throw new Exception(Traductor.T("error.cotizacionNoEnviada"));
            }

            // La confirmacion va en el idioma del interesado. Si falla, la
            // solicitud igual le llego a Strategic
            try
            {
                MailService.EnviarMail(email.Trim(),
                    Traductor.T("mail.cotizacion.asunto"),
                    Traductor.T("mail.cotizacion.cuerpo", nombre.Trim()),
                    null,
                    null);
            }
            catch (Exception)
            {
            }
        }

        // "campo" es la clave de traduccion del nombre del campo, por ejemplo "campo.elNombre"
        private void ExigirTexto(string valor, string campo, int largoMaximo)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                throw new Exception(Traductor.T("error.debeCompletar", Traductor.T(campo)));
            }

            if (valor.Trim().Length > largoMaximo)
            {
                throw new Exception(Traductor.T("error.valorLargo", Traductor.T(campo), largoMaximo));
            }
        }

        // Precios de referencia en dolares, con el separador de miles del idioma actual
        protected string Precio(decimal monto)
        {
            CultureInfo cultura = Traductor.IdiomaActual == Traductor.Ingles
                ? CultureInfo.GetCultureInfo("en-US")
                : CultureInfo.GetCultureInfo("es-AR");

            return "USD " + monto.ToString("N0", cultura);
        }
    }
}
