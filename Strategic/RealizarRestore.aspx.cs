using BLL;
using Services;
using Strategic.Seguridad;
using System;
using System.Configuration;
using System.IO;
using System.Web.UI;

namespace Strategic
{
    // CU-008-040 - Realizar Restore Base de datos
    public partial class RealizarRestore : PaginaSegura
    {
        private readonly BLLRespaldo bllRespaldo = new BLLRespaldo();

        // Carpeta del servidor donde se deja el .bak subido antes de restaurar
        private string CarpetaSubidas
        {
            get { return ConfigurationManager.AppSettings["CarpetaSubidas"]; }
        }

        // Ruta del archivo ya subido, a la espera de la confirmacion
        private string RutaAConfirmar
        {
            get { return Convert.ToString(ViewState["RutaAConfirmar"]); }
            set { ViewState["RutaAConfirmar"] = value; }
        }

        public override void ActualizarIdioma()
        {
            Title = Traductor.T("restore.titulo");
            lblArchivo.Text = Traductor.T("restore.archivo");
            btnRestaurar.Text = Traductor.T("restore.restaurar");
            btnConfirmar.Text = Traductor.T("restore.confirmar");
            btnCancelar.Text = Traductor.T("comun.cancelar");

            // El archivo a confirmar se conoce recien al recuperar el ViewState
            string ruta = RutaAConfirmar;

            if (!string.IsNullOrEmpty(ruta) && File.Exists(ruta))
            {
                MostrarConfirmacion(Path.GetFileName(ruta), new FileInfo(ruta).Length);
            }
        }

        protected void btnRestaurar_Click(object sender, EventArgs e)
        {
            if (!fuArchivo.HasFile)
            {
                lblError.Text = Traductor.T("restore.archivoRequerido");
                return;
            }

            string nombreArchivo = Path.GetFileName(fuArchivo.FileName);

            if (!string.Equals(Path.GetExtension(nombreArchivo), ".bak", StringComparison.OrdinalIgnoreCase))
            {
                lblError.Text = Traductor.T("restore.extensionInvalida");
                return;
            }

            try
            {
                // El FileUpload no sobrevive al postback de la confirmacion, asi que
                // el archivo se guarda ahora y se restaura recien al confirmar
                if (!Directory.Exists(CarpetaSubidas))
                {
                    Directory.CreateDirectory(CarpetaSubidas);
                }

                string ruta = Path.Combine(CarpetaSubidas, nombreArchivo);
                fuArchivo.SaveAs(ruta);

                RutaAConfirmar = ruta;
                MostrarConfirmacion(nombreArchivo, fuArchivo.PostedFile.ContentLength);
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(Traductor.T("restore.errorSubida", ex.Message));
            }
        }

        protected void btnConfirmar_Click(object sender, EventArgs e)
        {
            string ruta = RutaAConfirmar;

            OcultarConfirmacion();

            if (string.IsNullOrEmpty(ruta))
            {
                return;
            }

            try
            {
                bllRespaldo.RealizarRestore(ruta, UsuarioEnSesion.NombreUsuario);

                lblExito.Text = Server.HtmlEncode(Traductor.T("restore.exito", Path.GetFileName(ruta)));
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(Traductor.T("restore.error", ex.Message));
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            string ruta = RutaAConfirmar;

            OcultarConfirmacion();

            // El archivo subido ya no hace falta
            if (!string.IsNullOrEmpty(ruta) && File.Exists(ruta))
            {
                File.Delete(ruta);
            }
        }

        private void MostrarConfirmacion(string nombreArchivo, long tamanioBytes)
        {
            lblConfirmacion.Text = Server.HtmlEncode(Traductor.T(
                "restore.confirmacion",
                nombreArchivo,
                tamanioBytes / (1024.0 * 1024.0)));

            pnlSeleccion.Visible = false;
            pnlConfirmacion.Visible = true;
        }

        private void OcultarConfirmacion()
        {
            RutaAConfirmar = null;
            pnlSeleccion.Visible = true;
            pnlConfirmacion.Visible = false;
        }
    }
}
