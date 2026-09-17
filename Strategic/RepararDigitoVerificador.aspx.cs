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
        private const string MensajeInconsistente = "Se detectaron datos modificados por fuera del sistema. Hasta repararlo, el sistema queda fuera de servicio: podés tomar los datos actuales como correctos o volver a un respaldo anterior.";
        private const string MensajeConsistente = "El dígito verificador de todas las tablas coincide con los datos actuales.";

        private readonly BLLDigitoVerificador bllDigitoVerificador = new BLLDigitoVerificador();

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
            lblEstado.Text = "¿Recalcular el dígito verificador? Los datos actuales de todas las tablas, incluidos los modificados por fuera del sistema, se van a tomar como correctos.";
            pnlConfirmacion.Visible = true;
            pnlAcciones.Visible = false;
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            lblEstado.Text = MensajeInconsistente;
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
                lblExito.Text = "Dígito verificador recalculado. Los datos actuales quedaron como referencia y el sistema vuelve a estar disponible.";
            }
            catch (Exception ex)
            {
                pnlAcciones.Visible = true;
                lblError.Text = Server.HtmlEncode("Error al recalcular el dígito verificador: " + ex.Message);
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
                lblError.Text = Server.HtmlEncode("Error al verificar la integridad: " + ex.Message);
            }
        }

        private void Mostrar(List<BEInconsistenciaDV> inconsistencias)
        {
            bool hayInconsistencias = inconsistencias.Count > 0;

            lblEstado.Text = hayInconsistencias ? MensajeInconsistente : MensajeConsistente;

            pnlAcciones.Visible = hayInconsistencias;
            pnlInicio.Visible = !hayInconsistencias;
            pnlTabla.Visible = hayInconsistencias;

            grillaInconsistencias.Cargar(inconsistencias, "No hay inconsistencias");
            lblCantidad.Text = inconsistencias.Count == 1 ? "1 inconsistencia" : inconsistencias.Count + " inconsistencias";
        }

        private void ConfigurarGrilla()
        {
            grillaInconsistencias.ClavePrimaria = "Tabla";
            grillaInconsistencias.FilasPorPagina = 25;
            grillaInconsistencias.MostrarSelectorFilas = true;

            grillaInconsistencias.AgregarColumna("Tabla", "Tabla");
            grillaInconsistencias.AgregarColumna("Clave", "Registro (clave)");
            grillaInconsistencias.AgregarColumna("Columna", "Campo");
            grillaInconsistencias.AgregarColumna("Detalle", "Detalle");
        }
    }
}
