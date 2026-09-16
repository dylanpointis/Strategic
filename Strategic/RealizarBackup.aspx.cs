using BE;
using BLL;
using Strategic.Componentes;
using Strategic.Seguridad;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.UI;

namespace Strategic
{
    // CU-008-039 - Realizar Respaldo (Backup)
    public partial class RealizarBackup : PaginaSegura
    {
        private readonly BLLRespaldo bllRespaldo = new BLLRespaldo();

        // Carpeta del servidor donde se guardan los .bak, configurada en el Web.config
        private string CarpetaRespaldos
        {
            get { return ConfigurationManager.AppSettings["CarpetaRespaldos"]; }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            // Las columnas se arman en el Init para que la grilla pueda reconstruirse
            // en cada postback a partir de su ViewState
            ConfigurarGrilla();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                lblCarpeta.Text = Server.HtmlEncode(CarpetaRespaldos);
                lblMaximo.Text = BLLRespaldo.MaximoRespaldos.ToString();
                CargarGrilla();
            }
        }

        protected void btnGenerar_Click(object sender, EventArgs e)
        {
            pnlConfirmacion.Visible = true;
            btnGenerar.Enabled = false;
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            OcultarConfirmacion();
        }

        protected void btnConfirmar_Click(object sender, EventArgs e)
        {
            OcultarConfirmacion();

            try
            {
                BERespaldo respaldo = bllRespaldo.RealizarBackup(CarpetaRespaldos, UsuarioEnSesion.NombreUsuario);

                lblExito.Text = Server.HtmlEncode(string.Format(
                    "Backup generado correctamente el {0:dd/MM/yyyy} a las {0:HH:mm} ({1}). Archivo: {2}",
                    respaldo.Fecha,
                    respaldo.TamanioTexto,
                    respaldo.NombreArchivo));
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode("Error al realizar el respaldo: " + ex.Message);
            }

            CargarGrilla();
        }

        protected void grillaRespaldos_AccionSeleccionada(object sender, AccionGrillaEventArgs e)
        {
            if (e.Comando != "Descargar")
            {
                return;
            }

            try
            {
                BERespaldo respaldo = bllRespaldo.TraerRespaldo(CarpetaRespaldos, e.ObtenerClave("NombreArchivo"));

                if (respaldo == null)
                {
                    lblError.Text = "No se encontró el archivo de respaldo";
                    return;
                }

                Descargar(respaldo);
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode("Error al descargar el respaldo: " + ex.Message);
            }
        }

        private void Descargar(BERespaldo respaldo)
        {
            Response.Clear();
            Response.ContentType = "application/octet-stream";
            Response.AppendHeader("Content-Disposition", "attachment; filename=" + respaldo.NombreArchivo);
            Response.AppendHeader("Content-Length", respaldo.TamanioBytes.ToString());
            Response.TransmitFile(respaldo.Ruta);
            Response.Flush();
            Response.SuppressContent = true;
            Context.ApplicationInstance.CompleteRequest();
        }

        private void ConfigurarGrilla()
        {
            grillaRespaldos.ClavePrimaria = "NombreArchivo";
            grillaRespaldos.FilasPorPagina = 10;
            grillaRespaldos.MostrarSelectorFilas = false;

            grillaRespaldos.AgregarColumna("NombreArchivo", "Archivo");
            grillaRespaldos.AgregarColumna("Fecha", "Fecha", "celda-fecha", "{0:dd/MM/yyyy HH:mm}");
            grillaRespaldos.AgregarColumna("TamanioTexto", "Tamaño", "celda-numero");
            grillaRespaldos.AgregarColumnaAccion("Descarga", "Descargar", "Descargar");
        }

        private void CargarGrilla()
        {
            try
            {
                List<BERespaldo> respaldos = bllRespaldo.TraerRespaldos(CarpetaRespaldos);

                grillaRespaldos.Cargar(respaldos, "Todavía no se generaron respaldos");
                lblCantidad.Text = respaldos.Count == 1 ? "1 respaldo" : respaldos.Count + " respaldos";
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(ex.Message);
            }
        }

        private void OcultarConfirmacion()
        {
            pnlConfirmacion.Visible = false;
            btnGenerar.Enabled = true;
        }
    }
}
