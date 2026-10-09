using BE;
using BLL;
using Services;
using Strategic.Seguridad;
using System;
using System.Collections.Generic;
using System.Web.UI;

namespace Strategic
{
    // CU-008-041 - Reparar Digito Verificador
    public partial class RepararDigitoVerificador : PaginaSegura
    {
        private const string MensajeInconsistente = "repararDV.inconsistente";
        private const string MensajeConsistente = "repararDV.consistente";
        private const string MensajeConfirmar = "repararDV.confirmar";

        private readonly BLLDigitoVerificador bllDigitoVerificador = new BLLDigitoVerificador();

        // Clave de traduccion del mensaje de estado que se esta mostrando
        private string EstadoActual
        {
            get { return Convert.ToString(ViewState["EstadoActual"]); }
            set { ViewState["EstadoActual"] = value; }
        }

        public override void ActualizarIdioma()
        {
            Title = Traductor.T("repararDV.titulo");
            btnRecalcular.Text = Traductor.T("repararDV.recalcular");
            btnRestaurar.Text = Traductor.T("repararDV.restaurar");
            btnVerificar.Text = Traductor.T("repararDV.verificar");
            btnConfirmar.Text = Traductor.T("repararDV.siRecalcular");
            btnCancelar.Text = Traductor.T("comun.cancelar");
            btnInicio.Text = Traductor.T("accesoDenegado.irAlInicio");
            MostrarCantidad();

            // El estado se conoce recien al recuperar el ViewState
            if (!string.IsNullOrEmpty(EstadoActual))
            {
                MostrarEstado(EstadoActual);
            }
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
                // Lo que encontro el login. Si se entro directo a la pantalla, se verifica ahora
                List<BEInconsistenciaDV> inconsistencias = SessionManager.InconsistenciasDV;

                if (inconsistencias == null)
                {
                    Verificar();
                }
                else
                {
                    Mostrar(inconsistencias);
                }
            }
        }

        protected void btnVerificar_Click(object sender, EventArgs e)
        {
            Verificar();
        }

        protected void btnRecalcular_Click(object sender, EventArgs e)
        {
            MostrarEstado(MensajeConfirmar);
            pnlConfirmacion.Visible = true;
            pnlAcciones.Visible = false;
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            MostrarEstado(MensajeInconsistente);
            pnlConfirmacion.Visible = false;
            pnlAcciones.Visible = true;
        }

        protected void btnConfirmar_Click(object sender, EventArgs e)
        {
            pnlConfirmacion.Visible = false;

            try
            {
                bllDigitoVerificador.RecalcularDV(UsuarioEnSesion.NombreUsuario);

                SessionManager.InconsistenciasDV = null;
                Mostrar(new List<BEInconsistenciaDV>());
                lblExito.Text = Traductor.T("repararDV.exito");
            }
            catch (Exception ex)
            {
                pnlAcciones.Visible = true;
                lblError.Text = Server.HtmlEncode(Traductor.T("repararDV.errorRecalcular", ex.Message));
            }
        }

        protected void btnRestaurar_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/RealizarRestore.aspx");
        }

        protected void btnInicio_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Default.aspx");
        }

        private void Verificar()
        {
            try
            {
                List<BEInconsistenciaDV> inconsistencias = bllDigitoVerificador.CompararDV();

                SessionManager.InconsistenciasDV = inconsistencias;
                Mostrar(inconsistencias);
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(Traductor.T("repararDV.errorVerificar", ex.Message));
            }
        }

        private void Mostrar(List<BEInconsistenciaDV> inconsistencias)
        {
            bool hayInconsistencias = inconsistencias.Count > 0;

            MostrarEstado(hayInconsistencias ? MensajeInconsistente : MensajeConsistente);

            pnlAcciones.Visible = hayInconsistencias;
            pnlInicio.Visible = !hayInconsistencias;
            pnlTabla.Visible = hayInconsistencias;

            grillaInconsistencias.Cargar(inconsistencias, "repararDV.sinInconsistencias");
            MostrarCantidad();
        }

        private void MostrarEstado(string clave)
        {
            EstadoActual = clave;
            lblEstado.Text = Traductor.T(clave);
        }

        private void MostrarCantidad()
        {
            int cantidad = grillaInconsistencias.CantidadRegistros;

            lblCantidad.Text = cantidad == 1
                ? Traductor.T("repararDV.cantidadUno")
                : Traductor.T("repararDV.cantidad", cantidad);
        }

        private void ConfigurarGrilla()
        {
            grillaInconsistencias.ClavePrimaria = "Tabla";
            grillaInconsistencias.FilasPorPagina = 25;
            grillaInconsistencias.MostrarSelectorFilas = true;

            grillaInconsistencias.AgregarColumna("Tabla", "repararDV.tabla");
            grillaInconsistencias.AgregarColumna("Clave", "repararDV.registro");
            grillaInconsistencias.AgregarColumna("Columna", "repararDV.campo");
            grillaInconsistencias.AgregarColumnaTraducida("Detalle", "comun.detalle");
        }
    }
}
