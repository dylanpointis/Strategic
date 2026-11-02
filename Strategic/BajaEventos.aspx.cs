using BLL;
using Services;
using Strategic.Seguridad;
using System;

namespace Strategic
{
    // CU-008-038 - Baja logica de los eventos anteriores a los ultimos 30 dias
    public partial class BajaEventos : PaginaSegura
    {
        private readonly BLLEvento bllEvento = new BLLEvento();

        public override void ActualizarIdioma()
        {
            Title = Traductor.T("bajaEventos.titulo");
            btnConfirmar.Text = Traductor.T("bajaEventos.confirmarBoton");
            btnCancelar.Text = Traductor.T("comun.cancelar");
            btnVolver.Text = Traductor.T("comun.volverAlListado");

            if (IsPostBack)
            {
                CargarConfirmacion();
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarConfirmacion();
            }
        }

        protected void btnConfirmar_Click(object sender, EventArgs e)
        {
            try
            {
                int cantidad = bllEvento.BajaEventos(SessionManager.UsuarioActual.NombreUsuario);

                Session["MensajeEventos"] = Traductor.T("bajaEventos.exito",
                    cantidad, bllEvento.FechaLimiteMantenimiento().ToString("dd/MM/yyyy"));

                Response.Redirect("~/ConsultarEventosSistema.aspx");
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(ex.Message);
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/ConsultarEventosSistema.aspx");
        }

        // Informa cuantos eventos se darian de baja; si no hay ninguno no hay nada que confirmar
        private void CargarConfirmacion()
        {
            try
            {
                int cantidad = bllEvento.ContarEventosParaBaja();
                string fechaLimite = bllEvento.FechaLimiteMantenimiento().ToString("dd/MM/yyyy");

                if (cantidad == 0)
                {
                    MostrarSinRegistro(Traductor.T("bajaEventos.sinEventos", BLLEvento.DiasDeMantenimiento));
                    return;
                }

                lblConfirmacion.Text = Server.HtmlEncode(Traductor.T("bajaEventos.confirmar", cantidad, fechaLimite));
            }
            catch (Exception ex)
            {
                MostrarSinRegistro(ex.Message);
            }
        }

        private void MostrarSinRegistro(string mensaje)
        {
            lblSinRegistro.Text = Server.HtmlEncode(mensaje);

            pnlConfirmacion.Visible = false;
            pnlSinRegistro.Visible = true;
        }
    }
}
