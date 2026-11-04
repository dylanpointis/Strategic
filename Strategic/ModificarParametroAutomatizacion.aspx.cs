using BE;
using BLL;
using Services;
using Strategic.Seguridad;
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;

namespace Strategic
{
    // CU-003-011 - Modificar parametro de Automatizacion
    // Solo cambian los valores: el tipo y la logica son fijos
    public partial class ModificarParametroAutomatizacion : PaginaSegura
    {
        private readonly BLLAutomatizacion bllAutomatizacion = new BLLAutomatizacion();

        // Automatizacion que se esta editando
        private int AutomatizacionEditada
        {
            get { return ViewState["AutomatizacionEditada"] == null ? 0 : Convert.ToInt32(ViewState["AutomatizacionEditada"]); }
            set { ViewState["AutomatizacionEditada"] = value; }
        }

        public override void ActualizarIdioma()
        {
            Title = Traductor.T("modificarParametro.titulo");
            btnGuardar.Text = Traductor.T("comun.guardarCambios");
            btnCancelar.Text = Traductor.T("comun.cancelar");
            btnConfirmar.Text = Traductor.T("comun.confirmar");
            btnNoConfirmar.Text = Traductor.T("comun.cancelar");
            btnVolver.Text = Traductor.T("comun.volverAlListado");

            // Solo con el idioma cambiado en un postback ya estan los parametros:
            // tocar el repeater en el Init le impediria recuperar sus filas
            if (AutomatizacionEditada > 0)
            {
                foreach (RepeaterItem item in rptParametros.Items)
                {
                    TraducirParametro(item);
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarAutomatizacion(Request.QueryString["id"]);
            }
        }

        protected void rptParametros_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            BEParametroAutomatizacion parametro = e.Item.DataItem as BEParametroAutomatizacion;

            if (parametro == null)
            {
                return;
            }

            RangeValidator valRango = (RangeValidator)e.Item.FindControl("valRango");
            valRango.MinimumValue = parametro.Minimo.ToString();
            valRango.MaximumValue = parametro.Maximo.ToString();

            TraducirParametro(e.Item);
        }

        // Pasos 3 y 4: valida los valores y pide confirmacion
        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!IsValid)
            {
                return;
            }

            try
            {
                BEAutomatizacion automatizacion = bllAutomatizacion.TraerAutomatizacionPorId(AutomatizacionEditada);

                bllAutomatizacion.ValidarParametros(automatizacion, LeerValores());

                MostrarConfirmacion(true);
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(ex.Message);
            }
        }

        // Pasos 5 y 6
        protected void btnConfirmar_Click(object sender, EventArgs e)
        {
            try
            {
                bllAutomatizacion.ModificarParametros(AutomatizacionEditada, LeerValores(), SessionManager.UsuarioActual.NombreUsuario);

                Session["MensajeAutomatizaciones"] = Traductor.T("modificarParametro.exito");

                Response.Redirect("~/ConsultarAutomatizaciones.aspx");
            }
            catch (Exception ex)
            {
                MostrarConfirmacion(false);
                lblError.Text = Server.HtmlEncode(ex.Message);
            }
        }

        protected void btnNoConfirmar_Click(object sender, EventArgs e)
        {
            MostrarConfirmacion(false);
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/ConsultarAutomatizaciones.aspx");
        }

        private void CargarAutomatizacion(string idTexto)
        {
            int idAutomatizacion;

            if (!int.TryParse(idTexto, out idAutomatizacion) || idAutomatizacion <= 0)
            {
                MostrarSinAutomatizacion(Traductor.T("automatizacion.noEncontrada"));
                return;
            }

            try
            {
                BEAutomatizacion automatizacion = bllAutomatizacion.TraerAutomatizacionPorId(idAutomatizacion);

                if (automatizacion == null)
                {
                    MostrarSinAutomatizacion(Traductor.T("automatizacion.noEncontrada"));
                    return;
                }

                AutomatizacionEditada = automatizacion.IdAutomatizacion;
                lblNombre.Text = Server.HtmlEncode(automatizacion.Nombre);
                lblDescripcion.Text = Server.HtmlEncode(automatizacion.Descripcion);

                rptParametros.DataSource = bllAutomatizacion.TraerParametros(automatizacion);
                rptParametros.DataBind();
            }
            catch (Exception ex)
            {
                MostrarSinAutomatizacion(ex.Message);
            }
        }

        // Los textos del parametro salen de su clave y su rango
        private void TraducirParametro(RepeaterItem item)
        {
            string clave = ((HiddenField)item.FindControl("hfClave")).Value;
            string etiqueta = Traductor.T("automatizacion.parametro." + clave);
            RangeValidator valRango = (RangeValidator)item.FindControl("valRango");

            ((Label)item.FindControl("lblParametro")).Text = etiqueta;
            ((RequiredFieldValidator)item.FindControl("valRequerido")).ErrorMessage = Traductor.T("automatizacion.parametroRequerido");
            valRango.ErrorMessage = Traductor.T("automatizacion.parametroFueraDeRango", etiqueta, valRango.MinimumValue, valRango.MaximumValue);
        }

        private Dictionary<string, int> LeerValores()
        {
            Dictionary<string, int> valores = new Dictionary<string, int>();

            foreach (RepeaterItem item in rptParametros.Items)
            {
                string clave = ((HiddenField)item.FindControl("hfClave")).Value;
                TextBox txtValor = (TextBox)item.FindControl("txtValor");

                valores[clave] = Convert.ToInt32(txtValor.Text);
            }

            return valores;
        }

        private void MostrarConfirmacion(bool mostrar)
        {
            pnlConfirmacion.Visible = mostrar;
            pnlAcciones.Visible = !mostrar;
            rptParametros.Visible = !mostrar;
        }

        private void MostrarSinAutomatizacion(string mensaje)
        {
            lblSinAutomatizacion.Text = Server.HtmlEncode(mensaje);

            pnlFormulario.Visible = false;
            pnlSinAutomatizacion.Visible = true;
        }
    }
}
