using BE;
using BLL;
using Services;
using Strategic.Seguridad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

namespace Strategic
{
    // CU-006-029 - Configurar Integracion
    public partial class ConfigurarIntegracion : PaginaSegura
    {
        private readonly BLLIntegracion bllIntegracion = new BLLIntegracion();

        // Configuracion guardada del tipo elegido, para proponer sus columnas al armar el mapeo
        private BEConfiguracionIntegracion configuracionExistente;

        // Columnas leidas del archivo de muestra; alimentan los combos del mapeo
        private List<string> ColumnasArchivo
        {
            get { return ViewState["ColumnasArchivo"] as List<string> ?? new List<string>(); }
            set { ViewState["ColumnasArchivo"] = value; }
        }

        private string TipoSeleccionado
        {
            get { return ddlTipoArchivo.SelectedValue; }
        }

        public override void ActualizarIdioma()
        {
            Title = Traductor.T("configurarIntegracion.titulo");
            lblTipoArchivo.Text = Traductor.T("integracion.tipoArchivo");
            lblArchivo.Text = Traductor.T("configurarIntegracion.archivo");
            btnLeerArchivo.Text = Traductor.T("configurarIntegracion.leerArchivo");
            btnCancelar.Text = Traductor.T("comun.cancelar");

            // Las opciones se agregan desde codigo: en el Init de un postback
            // todavia no se recuperaron del ViewState
            foreach (ListItem item in ddlTipoArchivo.Items)
            {
                item.Text = Traductor.T("integracion.tipo." + item.Value);
            }

            if (ddlTipoArchivo.Items.Count > 0)
            {
                MostrarConfiguracionExistente();
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                foreach (string tipo in BLLIntegracion.TiposArchivo)
                {
                    ddlTipoArchivo.Items.Add(new ListItem(Traductor.T("integracion.tipo." + tipo), tipo));
                }

                MostrarConfiguracionExistente();
            }
        }

        protected void ddlTipoArchivo_SelectedIndexChanged(object sender, EventArgs e)
        {
            // El mapeo armado era para el tipo anterior
            pnlMapeo.Visible = false;
            MostrarConfiguracionExistente();
        }

        // Pasos 3 y 4: se lee el archivo de muestra y se arma el formulario de mapeo
        protected void btnLeerArchivo_Click(object sender, EventArgs e)
        {
            if (!fuMuestra.HasFile)
            {
                lblError.Text = Server.HtmlEncode(Traductor.T("integracion.seleccioneArchivo"));
                return;
            }

            try
            {
                List<string> columnas = bllIntegracion.LeerColumnasArchivo(fuMuestra.PostedFile.InputStream, fuMuestra.FileName);

                ColumnasArchivo = columnas;
                lblColumnasLeidas.Text = Server.HtmlEncode(Traductor.T("configurarIntegracion.columnasLeidas", fuMuestra.FileName, columnas.Count));

                configuracionExistente = bllIntegracion.ConsultarConfiguracionIntegracion(TipoSeleccionado);
                rptMapeo.DataSource = bllIntegracion.TraerCampos(TipoSeleccionado);
                rptMapeo.DataBind();

                pnlMapeo.Visible = true;
            }
            catch (Exception ex)
            {
                pnlMapeo.Visible = false;
                lblError.Text = Server.HtmlEncode(ex.Message);
            }
        }

        protected void rptMapeo_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
            {
                return;
            }

            BECampoIntegracion campo = (BECampoIntegracion)e.Item.DataItem;
            Label lblCampo = (Label)e.Item.FindControl("lblCampo");
            DropDownList ddlColumna = (DropDownList)e.Item.FindControl("ddlColumna");

            lblCampo.Text = Server.HtmlEncode(Traductor.T(campo.CampoTexto) + (campo.Obligatorio ? " *" : string.Empty));

            ddlColumna.Items.Add(new ListItem(Traductor.T("configurarIntegracion.sinAsignar"), string.Empty));

            foreach (string columna in ColumnasArchivo)
            {
                ddlColumna.Items.Add(new ListItem(columna, columna));
            }

            ddlColumna.SelectedValue = SugerirColumna(campo.Nombre);
        }

        // Paso 6: se valida y se guarda el mapeo
        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            BEConfiguracionIntegracion configuracion = new BEConfiguracionIntegracion { TipoArchivo = TipoSeleccionado };

            foreach (RepeaterItem item in rptMapeo.Items)
            {
                configuracion.Mapeos.Add(new BEMapeoColumna
                {
                    ColumnaStrategic = ((HiddenField)item.FindControl("hfCampo")).Value,
                    ColumnaCliente = ((DropDownList)item.FindControl("ddlColumna")).SelectedValue
                });
            }

            try
            {
                bllIntegracion.RegistrarConfiguracionIntegracion(configuracion, SessionManager.UsuarioActual.NombreUsuario);

                pnlMapeo.Visible = false;
                lblExito.Text = Server.HtmlEncode(Traductor.T("configurarIntegracion.exito", Traductor.T("integracion.tipo." + TipoSeleccionado)));
                MostrarConfiguracionExistente();
            }
            catch (Exception ex)
            {
                lblErrorMapeo.Text = Server.HtmlEncode(ex.Message);
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            pnlMapeo.Visible = false;
        }

        // Alternativa 6.2 [#ERR026]: si el tipo ya tiene configuracion se avisa
        // y guardar pasa a reemplazarla
        private void MostrarConfiguracionExistente()
        {
            BEConfiguracionIntegracion existente = bllIntegracion.ConsultarConfiguracionIntegracion(TipoSeleccionado);

            if (existente == null)
            {
                lblConfiguracionExistente.Text = string.Empty;
                btnGuardar.Text = Traductor.T("configurarIntegracion.guardar");
                return;
            }

            lblConfiguracionExistente.Text = Server.HtmlEncode(Traductor.T(
                "configurarIntegracion.existente",
                existente.FechaConfiguracion.ToString("dd/MM/yyyy HH:mm")));
            btnGuardar.Text = Traductor.T("configurarIntegracion.reemplazar");
        }

        // Propone la columna ya configurada para el campo o, si no hay, una con el mismo nombre
        private string SugerirColumna(string campo)
        {
            string columna = configuracionExistente != null ? configuracionExistente.ColumnaDe(campo) : null;

            if (columna != null && ColumnasArchivo.Contains(columna))
            {
                return columna;
            }

            return ColumnasArchivo.FirstOrDefault(c => string.Equals(c, campo, StringComparison.OrdinalIgnoreCase)) ?? string.Empty;
        }
    }
}
