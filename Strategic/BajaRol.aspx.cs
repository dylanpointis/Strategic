using BE;
using BLL;
using Services;
using Strategic.Seguridad;
using System;
using System.Web.UI;

namespace Strategic
{
    // CU-005-023 - Baja (y reactivacion) de Rol
    public partial class BajaRol : PaginaSegura
    {
        private readonly BLLRol bllRol = new BLLRol();

        /// <summary>
        /// Rol sobre el que se está pidiendo confirmación. Viaja en el
        /// ViewState entre el clic y la confirmación.
        /// </summary>
        private int RolAConfirmar
        {
            get { return ViewState["RolAConfirmar"] == null ? 0 : Convert.ToInt32(ViewState["RolAConfirmar"]); }
            set { ViewState["RolAConfirmar"] = value; }
        }

        private bool EstadoAConfirmar
        {
            get { return ViewState["EstadoAConfirmar"] != null && Convert.ToBoolean(ViewState["EstadoAConfirmar"]); }
            set { ViewState["EstadoAConfirmar"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarRol(Request.QueryString["rol"]);
            }
        }

        protected void btnConfirmar_Click(object sender, EventArgs e)
        {
            int codRol = RolAConfirmar;
            bool activo = EstadoAConfirmar;

            if (codRol <= 0)
            {
                return;
            }

            try
            {
                BEUsuario enSesion = SessionManager.UsuarioActual;
                BERol rol = bllRol.TraerRolPorId(codRol);

                bllRol.CambiarEstadoRol(codRol, activo, enSesion.NombreUsuario);

                Session["MensajeRoles"] = string.Format(
                    activo ? "Se reactivó el rol {0}" : "Se dio de baja el rol {0}",
                    rol != null ? rol.Nombre : codRol.ToString());

                Response.Redirect("~/ConsultarRoles.aspx");
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(ex.Message);
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/ConsultarRoles.aspx");
        }

        private void CargarRol(string valor)
        {
            int codRol;

            if (!int.TryParse(valor, out codRol))
            {
                MostrarSinRegistro("No se indicó qué rol dar de baja");
                return;
            }

            try
            {
                BERol rol = bllRol.TraerRolPorId(codRol);

                if (rol == null)
                {
                    MostrarSinRegistro("No se encontró el rol indicado");
                    return;
                }

                RolAConfirmar = rol.CodRol;
                EstadoAConfirmar = !rol.Activo;

                lblConfirmacion.Text = Server.HtmlEncode(string.Format(
                    rol.Activo
                        ? "¿Deseás dar de baja el rol {0}? No va a poder asignarse a nuevos usuarios."
                        : "¿Deseás reactivar el rol {0}?",
                    rol.Nombre));

                btnConfirmar.Text = rol.Activo ? "Dar de baja" : "Reactivar";
                btnConfirmar.CssClass = rol.Activo ? "btn btn-strategic-danger" : "btn btn-strategic";
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
