using BE;
using BLL;
using Services;
using Strategic.Seguridad;
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;

namespace Strategic
{
    // CU-006-030 - Importar y sincronizar Datos
    public partial class ImportarDatos : PaginaSegura
    {
        // Errores que se listan en el resumen; el resto queda en el historial
        private const int ErroresMostrados = 50;

        private readonly BLLIntegracion bllIntegracion = new BLLIntegracion();

        public override void ActualizarIdioma()
        {
            Title = Traductor.T("importarDatos.titulo");
            lblTipoArchivo.Text = Traductor.T("integracion.tipoArchivo");
            lblArchivo.Text = Traductor.T("importarDatos.archivoXml");
            btnImportar.Text = Traductor.T("importarDatos.importar");
            lblSinConfiguracion.Text = Server.HtmlEncode(Traductor.T("error.sinConfiguracionIntegracion"));
            lnkConfigurar.Text = Traductor.T("importarDatos.irAConfigurar");
            lnkHistorial.Text = Traductor.T("importarDatos.verHistorial");

            // Las opciones se agregan desde codigo: en el Init de un postback
            // todavia no se recuperaron del ViewState
            if (ddlTipoArchivo.Items.Count > 0)
            {
                ddlTipoArchivo.Items[0].Text = Traductor.T("integracion.seleccioneTipo");

                for (int i = 1; i < ddlTipoArchivo.Items.Count; i++)
                {
                    ddlTipoArchivo.Items[i].Text = Traductor.T("integracion.tipo." + ddlTipoArchivo.Items[i].Value);
                }
            }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            grillaMapeo.ClavePrimaria = "ColumnaStrategic";
            grillaMapeo.FilasPorPagina = 20;
            grillaMapeo.AgregarColumnaTraducida("CampoTexto", "configurarIntegracion.campoStrategic");
            grillaMapeo.AgregarColumna("ColumnaCliente", "configurarIntegracion.columnaCliente", "celda-codigo");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ddlTipoArchivo.Items.Add(new ListItem(Traductor.T("integracion.seleccioneTipo"), string.Empty));

                foreach (string tipo in BLLIntegracion.TiposArchivo)
                {
                    ddlTipoArchivo.Items.Add(new ListItem(Traductor.T("integracion.tipo." + tipo), tipo));
                }

                lnkConfigurar.Visible = TienePermiso("ConfigurarIntegracion");
                lnkHistorial.Visible = TienePermiso("HistorialSincronizaciones");
            }
        }

        // Paso 4: se muestra el mapeo guardado para el tipo elegido
        protected void ddlTipoArchivo_SelectedIndexChanged(object sender, EventArgs e)
        {
            pnlResultado.Visible = false;
            MostrarConfiguracion();
        }

        // Paso 6: se valida el archivo, se importa y se muestra el resumen
        protected void btnImportar_Click(object sender, EventArgs e)
        {
            BEConfiguracionIntegracion configuracion = MostrarConfiguracion();

            if (configuracion == null)
            {
                return;
            }

            if (!fuArchivo.HasFile)
            {
                lblError.Text = Server.HtmlEncode(Traductor.T("integracion.seleccioneArchivo"));
                return;
            }

            try
            {
                BEResultadoIntegracion resultado = bllIntegracion.ImportarDatos(
                    configuracion,
                    fuArchivo.PostedFile.InputStream,
                    fuArchivo.FileName,
                    SessionManager.UsuarioActual.NombreUsuario);

                MostrarResultado(resultado);
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(ex.Message);
            }
        }

        // Devuelve la configuracion del tipo elegido, o null si no hay
        private BEConfiguracionIntegracion MostrarConfiguracion()
        {
            pnlImportacion.Visible = false;
            pnlSinConfiguracion.Visible = false;

            if (string.IsNullOrEmpty(ddlTipoArchivo.SelectedValue))
            {
                return null;
            }

            try
            {
                BEConfiguracionIntegracion configuracion = bllIntegracion.ConsultarConfiguracionIntegracion(ddlTipoArchivo.SelectedValue);

                // Alternativa 4.1 [#ERR044]
                if (configuracion == null)
                {
                    pnlSinConfiguracion.Visible = true;
                    return null;
                }

                grillaMapeo.Cargar(configuracion.Mapeos);
                lblFechaConfiguracion.Text = Server.HtmlEncode(Traductor.T(
                    "importarDatos.configuradoEl",
                    configuracion.FechaConfiguracion.ToString("dd/MM/yyyy HH:mm")));

                pnlImportacion.Visible = true;
                return configuracion;
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(ex.Message);
                return null;
            }
        }

        private void MostrarResultado(BEResultadoIntegracion resultado)
        {
            lblEstadoResultado.Text = Server.HtmlEncode(Traductor.T(resultado.EstadoTexto));
            lblProcesados.Text = resultado.RegistrosProcesados.ToString("N0");
            lblImportados.Text = resultado.RegistrosImportados.ToString("N0");
            lblActualizados.Text = resultado.RegistrosActualizados.ToString("N0");
            lblConError.Text = resultado.RegistrosError.ToString("N0");

            lblMensajeResultado.Text = Server.HtmlEncode(Traductor.T(resultado.Mensaje));
            lblMensajeResultado.CssClass = resultado.Estado == BEResultadoIntegracion.EstadoExitosa
                ? "mensaje-exito"
                : resultado.Estado == BEResultadoIntegracion.EstadoConErrores ? "mensaje-aviso" : "mensaje-error";

            // El motivo de una falla ya se muestra como mensaje: no se repite en la lista
            List<string> errores = resultado.Errores.FindAll(e => e != Traductor.T(resultado.Mensaje));

            blErrores.Items.Clear();

            for (int i = 0; i < errores.Count && i < ErroresMostrados; i++)
            {
                blErrores.Items.Add(new ListItem(errores[i]));
            }

            if (errores.Count > ErroresMostrados)
            {
                blErrores.Items.Add(new ListItem(Traductor.T("integracion.masErrores", errores.Count - ErroresMostrados)));
            }

            pnlErrores.Visible = errores.Count > 0;
            pnlResultado.Visible = true;
        }
    }
}
