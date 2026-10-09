using BE;
using BE.Composite;
using BLL;
using Services;
using Strategic.Seguridad;
using System;
using System.Web.UI;

namespace Strategic
{
    // CU-005-028 - Modificar Familia
    public partial class ModificarFamilia : PaginaSegura
    {
        private readonly BLLPermiso bllPermiso = new BLLPermiso();

        private int FamiliaEditada
        {
            get { return ViewState["FamiliaEditada"] == null ? 0 : Convert.ToInt32(ViewState["FamiliaEditada"]); }
            set { ViewState["FamiliaEditada"] = value; }
        }

        // Clave de traduccion del estado de la familia editada
        private string EstadoEditado
        {
            get { return Convert.ToString(ViewState["EstadoEditado"]); }
            set { ViewState["EstadoEditado"] = value; }
        }

        public override void ActualizarIdioma()
        {
            Title = Traductor.T("modificarFamilia.titulo");
            lblNombre.Text = Traductor.T("familia.nombre");
            valNombre.ErrorMessage = Traductor.T("familia.nombreRequerido");
            lblDescripcion.Text = Traductor.T("familia.descripcion");
            btnGuardar.Text = Traductor.T("comun.guardarCambios");
            btnCancelar.Text = Traductor.T("comun.cancelar");
            btnVolver.Text = Traductor.T("comun.volverAlListado");

            // El estado se conoce recien al recuperar el ViewState
            if (!string.IsNullOrEmpty(EstadoEditado))
            {
                lblEstadoActual.Text = Server.HtmlEncode(Traductor.T(EstadoEditado));
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarFamilia(Request.QueryString["familia"]);
            }
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!IsValid)
            {
                return;
            }

            try
            {
                BEUsuario enSesion = SessionManager.UsuarioActual;

                bllPermiso.ModificarFamilia(
                    FamiliaEditada,
                    txtNombre.Text,
                    txtDescripcion.Text,
                    selectorPermisos.ObtenerSeleccionados(),
                    enSesion.NombreUsuario);

                Session["MensajeFamilias"] = Traductor.T("modificarFamilia.exito", txtNombre.Text.Trim());
                Response.Redirect("~/ConsultarFamilias.aspx");
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(ex.Message);
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/ConsultarFamilias.aspx");
        }

        private void CargarFamilia(string valor)
        {
            int codPermiso;

            if (!int.TryParse(valor, out codPermiso))
            {
                MostrarSinRegistro(Traductor.T("modificarFamilia.sinFamilia"));
                return;
            }

            try
            {
                BEFamilia familia = bllPermiso.TraerFamiliaConHijos(codPermiso);

                if (familia == null)
                {
                    MostrarSinRegistro(Traductor.T("familia.noEncontrada"));
                    return;
                }

                FamiliaEditada = familia.CodPermiso;
                EstadoEditado = familia.EstadoTexto;

                txtNombre.Text = familia.Nombre;
                txtDescripcion.Text = familia.Descripcion;
                lblEstadoActual.Text = Server.HtmlEncode(Traductor.T(familia.EstadoTexto));

                selectorPermisos.Cargar(
                    bllPermiso.TraerComponentesDisponiblesConArbol(),
                    familia.ObtenerHijos());

                // Una familia no puede contenerse a si misma: se saca de la
                // lista de disponibles. Los ciclos indirectos los rechaza la BLL
                selectorPermisos.ExcluirDeDisponibles(familia.CodPermiso);
            }
            catch (Exception ex)
            {
                MostrarSinRegistro(ex.Message);
            }
        }

        private void MostrarSinRegistro(string mensaje)
        {
            lblSinRegistro.Text = Server.HtmlEncode(mensaje);

            pnlFormulario.Visible = false;
            pnlSinRegistro.Visible = true;
        }
    }
}
