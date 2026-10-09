using BE;
using BLL;
using Services;
using Strategic.Componentes;
using Strategic.Seguridad;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Strategic
{
    // CU-004-013 - Ver Productos Sincronizados
    public partial class ProductosSincronizados : PaginaSegura
    {
        private const string MensajeSinProductos = "productos.sinProductos";
        private const string MensajeSinResultados = "comun.sinResultadosFiltros";

        private readonly BLLProducto bllProducto = new BLLProducto();

        private int ProductoSeleccionado
        {
            get { return ViewState["ProductoSeleccionado"] == null ? 0 : Convert.ToInt32(ViewState["ProductoSeleccionado"]); }
            set { ViewState["ProductoSeleccionado"] = value; }
        }

        public override void ActualizarIdioma()
        {
            Title = Traductor.T("productos.titulo");
            lblNombre.Text = Traductor.T("productos.nombreOCodigo");
            txtNombre.Attributes["placeholder"] = Traductor.T("comun.todos");
            lblCategoria.Text = Traductor.T("productos.categoria");
            lblEstado.Text = Traductor.T("comun.estado");
            lblPrecioDesde.Text = Traductor.T("productos.precioDesde");
            lblPrecioHasta.Text = Traductor.T("productos.precioHasta");
            txtPrecioHasta.Attributes["placeholder"] = Traductor.T("productos.sinTope");
            btnFiltrar.Text = Traductor.T("comun.filtrar");
            btnLimpiar.Text = Traductor.T("comun.limpiarFiltros");
            btnCerrarDetalle.Text = Traductor.T("eventos.cerrarDetalle");
            MostrarCantidad();

            // Lo que se arma desde codigo solo existe una vez recuperado el
            // ViewState, por eso se pregunta antes de traducirlo
            if (ddlCategoria.Items.Count > 0)
            {
                ddlCategoria.Items[0].Text = Traductor.T("comun.todos");
                ddlEstado.Items[0].Text = Traductor.T("comun.todos");
            }

            if (ProductoSeleccionado > 0)
            {
                MostrarDetalle(bllProducto.TraerProductoPorId(ProductoSeleccionado));
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
                List<BEProducto> productos = bllProducto.TraerListaProductos();

                CargarCombos(productos);
                CargarGrilla(productos, MensajeSinProductos);
            }
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            try
            {
                List<BEProducto> productos = bllProducto.FiltrarProductos(
                    txtNombre.Text.Trim(),
                    ddlCategoria.SelectedValue,
                    ddlEstado.SelectedValue,
                    ObtenerPrecio(txtPrecioDesde.Text),
                    ObtenerPrecio(txtPrecioHasta.Text));

                OcultarDetalle();
                CargarGrilla(productos, MensajeSinResultados);
            }
            catch (Exception ex)
            {
                lblError.Text = ex.Message;
            }
        }

        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            txtNombre.Text = string.Empty;
            txtPrecioDesde.Text = string.Empty;
            txtPrecioHasta.Text = string.Empty;
            ddlCategoria.SelectedIndex = 0;
            ddlEstado.SelectedIndex = 0;

            List<BEProducto> productos = bllProducto.TraerListaProductos();

            OcultarDetalle();
            CargarCombos(productos);
            CargarGrilla(productos, MensajeSinProductos);
        }

        protected void grillaProductos_AccionSeleccionada(object sender, AccionGrillaEventArgs e)
        {
            if (e.Comando != "VerDetalle")
            {
                return;
            }

            try
            {
                int idProducto = Convert.ToInt32(e.ObtenerClave("IdProducto"));
                BEProducto producto = bllProducto.TraerProductoPorId(idProducto);

                if (producto == null)
                {
                    OcultarDetalle();
                    lblError.Text = Traductor.T("productos.noEncontrado");
                    return;
                }

                MostrarDetalle(producto);
            }
            catch (Exception ex)
            {
                lblError.Text = ex.Message;
            }
        }

        protected void btnCerrarDetalle_Click(object sender, EventArgs e)
        {
            OcultarDetalle();
        }

        private void ConfigurarGrilla()
        {
            grillaProductos.ClavePrimaria = "IdProducto";
            grillaProductos.FilasPorPagina = 10;
            grillaProductos.MostrarSelectorFilas = true;

            grillaProductos.AgregarColumna("Codigo", "productos.codigo", "celda-codigo");
            grillaProductos.AgregarColumna("Nombre", "comun.nombre");
            grillaProductos.AgregarColumna("Marca", "productos.marca");
            grillaProductos.AgregarColumna("Categoria", "productos.categoria");
            grillaProductos.AgregarColumna("Estado", "comun.estado");
            grillaProductos.AgregarColumna("Precio", "productos.precio", "celda-numero", "${0:N2}");
            grillaProductos.AgregarColumna("Stock", "productos.stock", "celda-numero");
            grillaProductos.AgregarColumnaAccion("comun.detalle", "VerDetalle", "productos.verDetalle");
        }

        private void CargarGrilla(List<BEProducto> productos, string mensajeVacio)
        {
            grillaProductos.Cargar(productos, mensajeVacio);
            MostrarCantidad();
        }

        private void MostrarCantidad()
        {
            int cantidad = grillaProductos.CantidadRegistros;

            lblCantidad.Text = cantidad == 1
                ? Traductor.T("productos.cantidadUno")
                : Traductor.T("productos.cantidad", cantidad);
        }

        private void CargarCombos(List<BEProducto> productos)
        {
            CargarOpciones(ddlCategoria, productos.Select(producto => producto.Categoria));
            CargarOpciones(ddlEstado, productos.Select(producto => producto.Estado));
        }

        private void CargarOpciones(DropDownList lista, IEnumerable<string> valores)
        {
            lista.Items.Clear();
            lista.Items.Add(new ListItem(Traductor.T("comun.todos"), string.Empty));

            List<string> opciones = valores
                .Where(valor => !string.IsNullOrWhiteSpace(valor))
                .Distinct()
                .OrderBy(valor => valor)
                .ToList();

            foreach (string opcion in opciones)
            {
                lista.Items.Add(new ListItem(opcion, opcion));
            }
        }

        private void MostrarDetalle(BEProducto producto)
        {
            ProductoSeleccionado = producto.IdProducto;

            lblDetalleCodigo.Text = Server.HtmlEncode(producto.Codigo);
            lblDetalleNombre.Text = Server.HtmlEncode(producto.Nombre);
            lblDetalleMarca.Text = Server.HtmlEncode(TextoOGuion(producto.Marca));
            lblDetalleCategoria.Text = Server.HtmlEncode(TextoOGuion(producto.Categoria));
            lblDetalleEstado.Text = Server.HtmlEncode(producto.Estado);
            lblDetallePrecio.Text = Server.HtmlEncode(string.Format("${0:N2}", producto.Precio));
            lblDetalleStock.Text = Server.HtmlEncode(ObtenerTextoStock(producto));
            lblDetalleStockMinimoMaximo.Text = Server.HtmlEncode(string.Format("{0} / {1}",
                producto.StockMinimo.HasValue ? producto.StockMinimo.Value.ToString() : "-",
                producto.StockMaximo.HasValue ? producto.StockMaximo.Value.ToString() : "-"));
            lblDetalleFechaSincronizacion.Text = Server.HtmlEncode(producto.FechaSincronizacion.HasValue
                ? producto.FechaSincronizacion.Value.ToString("dd/MM/yyyy HH:mm")
                : Traductor.T("productos.sinSincronizar"));

            pnlDetalleProducto.Visible = true;
        }

        private void OcultarDetalle()
        {
            pnlDetalleProducto.Visible = false;
            ProductoSeleccionado = 0;
        }

        private string ObtenerTextoStock(BEProducto producto)
        {
            return producto.StockBajoMinimo
                ? Traductor.T("productos.stockBajoMinimo", producto.Stock)
                : producto.Stock.ToString();
        }

        private string TextoOGuion(string valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? "-" : valor;
        }

        private decimal? ObtenerPrecio(string valor)
        {
            decimal precio;

            // El input type="number" envia siempre el punto como separador decimal
            if (decimal.TryParse(valor, NumberStyles.Number, CultureInfo.InvariantCulture, out precio))
            {
                return precio;
            }

            if (decimal.TryParse(valor, out precio))
            {
                return precio;
            }

            return null;
        }
    }
}
