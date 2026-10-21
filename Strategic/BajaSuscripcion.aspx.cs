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
            lblClave.Text = Traductor.T("bajaSuscripcion.clave");
            valClave.ErrorMessage = Traductor.T("error.debeCompletar", Traductor.T("campo.laClave"));
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
            try
            {
                BEUsuario enSesion = SessionManager.UsuarioActual;

                DateTime finDeAcceso = bllSuscripcion.CancelarSuscripcion(txtClave.Text, enSesion.NombreUsuario);

                Session["MensajeSuscripcion"] = Traductor.T("bajaSuscripcion.exito", finDeAcceso.ToString("dd/MM/yyyy"));

                Response.Redirect("~/RenovarSuscripcion.aspx");
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(ex.Message);
            }
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
