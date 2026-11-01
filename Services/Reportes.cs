using BE;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using System.IO;
using System.Web;

namespace Services
{
    // Arma los PDF del sistema a partir de una plantilla HTML con marcadores @
    public static class Reportes
    {
        // CU-009-042: factura de una renovacion de la suscripcion
        public static byte[] GenerarFacturaSuscripcion(BESuscripcion suscripcion, BEUsuario cliente, string paginahtml, string rutaLogo)
        {
            // Primero los textos: asi un dato que contenga "@texto..." no se reemplaza
            paginahtml = paginahtml.Replace("@textoFactura", Texto("factura.numero"));
            paginahtml = paginahtml.Replace("@textoFecha", Texto("comun.fecha"));
            paginahtml = paginahtml.Replace("@textoDetalleCliente", Texto("factura.detalleCliente"));
            paginahtml = paginahtml.Replace("@textoUsuario", Texto("usuario.nombreUsuario"));
            paginahtml = paginahtml.Replace("@textoNombre", Texto("usuario.nombre"));
            paginahtml = paginahtml.Replace("@textoApellido", Texto("usuario.apellido"));
            paginahtml = paginahtml.Replace("@textoEmail", Texto("usuario.email"));
            paginahtml = paginahtml.Replace("@textoDetalleServicio", Texto("factura.detalleServicio"));
            paginahtml = paginahtml.Replace("@textoConcepto", Texto("factura.concepto"));
            paginahtml = paginahtml.Replace("@textoPeriodo", Texto("suscripcion.periodo"));
            paginahtml = paginahtml.Replace("@textoImporte", Texto("suscripcion.monto"));
            paginahtml = paginahtml.Replace("@textoDetallePago", Texto("factura.detallePago"));
            paginahtml = paginahtml.Replace("@textoNeto", Texto("factura.neto"));
            paginahtml = paginahtml.Replace("@textoIVA", Codificar(Traductor.T("factura.iva", BESuscripcion.PorcentajeIva)));
            paginahtml = paginahtml.Replace("@textoMontoTotal", Texto("factura.total"));
            paginahtml = paginahtml.Replace("@textoMetodo", Texto("factura.metodo"));
            paginahtml = paginahtml.Replace("@textoNumeroOperacion", Texto("suscripcion.numeroOperacion"));
            paginahtml = paginahtml.Replace("@textoPie", Texto("factura.pie"));

            // Cada renovacion paga un solo periodo: la factura tiene una linea
            string filas = "";
            filas += "<tr>";
            filas += "<td>" + Codificar(Traductor.T("factura.conceptoSuscripcion", suscripcion.Plan)) + "</td>";
            filas += "<td>" + Codificar(suscripcion.Periodo) + "</td>";
            filas += "<td>" + Importe(suscripcion.Monto) + "</td>";
            filas += "</tr>";

            paginahtml = paginahtml.Replace("@FILAS", filas);

            paginahtml = paginahtml.Replace("@NroFactura", suscripcion.IdSuscripcion.ToString("D8"));
            paginahtml = paginahtml.Replace("@Fecha", suscripcion.FechaPago.ToString("dd/MM/yyyy HH:mm"));

            paginahtml = paginahtml.Replace("@Neto", Importe(suscripcion.MontoNeto));
            paginahtml = paginahtml.Replace("@Iva", Importe(suscripcion.MontoIva));
            paginahtml = paginahtml.Replace("@Total", Importe(suscripcion.Monto));
            paginahtml = paginahtml.Replace("@Metodo", Texto("factura.metodoTransferencia"));

            // Igual que en el historial, el cliente ve solo el final del comprobante
            paginahtml = paginahtml.Replace("@NumeroOperacion", Codificar(suscripcion.NumeroOperacionEnmascarado));

            // El email va ultimo: tiene una @ y no tiene que coincidir con otro marcador
            paginahtml = paginahtml.Replace("@Usuario", Codificar(cliente.NombreUsuario));
            paginahtml = paginahtml.Replace("@Nombre", Codificar(cliente.Nombre));
            paginahtml = paginahtml.Replace("@Apellido", Codificar(cliente.Apellido));
            paginahtml = paginahtml.Replace("@Email", Codificar(cliente.Email));

            return GenerarPDF(paginahtml, rutaLogo);
        }

        private static byte[] GenerarPDF(string paginahtml, string rutaLogo)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                Document pdf = new Document(PageSize.A4, 25, 25, 25, 25);
                PdfWriter escritor = PdfWriter.GetInstance(pdf, stream);

                pdf.Open();
                Image img = Image.GetInstance(rutaLogo);
                img.ScaleToFit(100, 40);
                img.Alignment = Image.UNDERLYING;
                img.SetAbsolutePosition(pdf.Right - 100, pdf.Top - 40);
                pdf.Add(img);

                using (StringReader lector = new StringReader(paginahtml))
                {
                    XMLWorkerHelper.GetInstance().ParseXHtml(escritor, pdf, lector);
                }

                pdf.Close();

                // Close tambien cierra el stream, pero ToArray sigue andando
                return stream.ToArray();
            }
        }

        private static string Texto(string clave)
        {
            return Codificar(Traductor.T(clave));
        }

        private static string Importe(decimal monto)
        {
            return string.Format("${0:N2}", monto);
        }

        // La plantilla se lee como XHTML: un & o un < en los datos la romperia
        private static string Codificar(string texto)
        {
            return HttpUtility.HtmlEncode(texto ?? string.Empty);
        }
    }
}
