using BE;
using BLL;
using Services;
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
        protected string CarpetaRespaldos
        {
            get { return ConfigurationManager.AppSettings["CarpetaRespaldos"]; }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            // Las columnas se arman en el Init para que la grilla pueda reconstruirse
            // en cada postback a partir de su ViewState
            ConfigurarGrilla();
        }

        public override void ActualizarIdioma()
        {
            Title = Traductor.T("backup.titulo");
            btnGenerar.Text = Traductor.T("backup.generar");
            btnConfirmar.Text = Traductor.T("comun.confirmar");
            btnCancelar.Text = Traductor.T("comun.cancelar");
            MostrarCantidad();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
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

                lblExito.Text = Server.HtmlEncode(Traductor.T(
                    "backup.exito",
                    respaldo.Fecha,
                    respaldo.TamanioTexto,
                    respaldo.NombreArchivo));
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(Traductor.T("backup.error", ex.Message));
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
                    lblError.Text = Traductor.T("backup.archivoNoEncontrado");
                    return;
                }

                Descargar(respaldo);
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(Traductor.T("backup.errorDescarga", ex.Message));
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

            grillaRespaldos.AgregarColumna("NombreArchivo", "backup.archivo");
            grillaRespaldos.AgregarColumna("Fecha", "comun.fecha", "celda-fecha", "{0:dd/MM/yyyy HH:mm}");
            grillaRespaldos.AgregarColumna("TamanioTexto", "backup.tamanio", "celda-numero");
            grillaRespaldos.AgregarColumnaAccion("backup.descarga", "Descargar", "backup.descargar");
        }

        private void CargarGrilla()
        {
            try
            {
                List<BERespaldo> respaldos = bllRespaldo.TraerRespaldos(CarpetaRespaldos);

                grillaRespaldos.Cargar(respaldos, "backup.sinRespaldos");
                MostrarCantidad();
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(ex.Message);
            }
        }

        private void MostrarCantidad()
        {
            int cantidad = grillaRespaldos.CantidadRegistros;

            lblCantidad.Text = cantidad == 1
                ? Traductor.T("backup.cantidadUno")
                : Traductor.T("backup.cantidad", cantidad);
        }

        private void OcultarConfirmacion()
        {
            pnlConfirmacion.Visible = false;
            btnGenerar.Enabled = true;
        }
    }
}
