using System.IO;
using System.Net.Mail;
using System.Web;

namespace Services
{
    // Manda mails por SMTP. El servidor, la cuenta y la clave salen de
    // system.net/mailSettings en el Web.config
    public static class MailService
    {
        // Casilla del equipo de Strategic: recibe los avisos de renovacion y
        // baja de suscripciones y las solicitudes de cotizacion
        public const string MailStrategic = "strategic.services.contact@gmail.com";

        // Si el servidor no contesta, la pagina no queda colgada esperando
        private const int TiempoEsperaMs = 15000;

        public static void EnviarMail(string destinatario, string asunto, string cuerpo, byte[] adjunto, string nombreAdjunto)
        {
            // Sin parametros, MailMessage y SmtpClient toman el remitente y el servidor del Web.config
            using (MailMessage mail = new MailMessage())
            using (SmtpClient cliente = new SmtpClient())
            {
                mail.To.Add(destinatario);
                mail.Subject = asunto;
                mail.Body = cuerpo;

                if (adjunto != null)
                {
                    mail.Attachments.Add(new Attachment(new MemoryStream(adjunto), nombreAdjunto, MimeMapping.GetMimeMapping(nombreAdjunto)));
                }

                cliente.Timeout = TiempoEsperaMs;
                cliente.Send(mail);
            }
        }
    }
}
