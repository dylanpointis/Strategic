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
        private const string MensajeSinProductos = "No existen datos sincronizados. Realice una sincronización";
        private const string MensajeSinResultados = "No se encontraron datos con los filtros ingresados";

        private readonly BLLProducto bllProducto = new BLLProducto();

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
                    lblError.Text = "No se encontró el producto seleccionado";
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

            grillaProductos.AgregarColumna("Codigo", "Código", "celda-codigo");
            grillaProductos.AgregarColumna("Nombre", "Nombre");
            grillaProductos.AgregarColumna("Marca", "Marca");
            grillaProductos.AgregarColumna("Categoria", "Categoría");
            grillaProductos.AgregarColumna("Estado", "Estado");
            grillaProductos.AgregarColumna("Precio", "Precio", "celda-numero", "${0:N2}");
            grillaProductos.AgregarColumna("Stock", "Stock", "celda-numero");
            grillaProductos.AgregarColumnaAccion("Detalle", "VerDetalle", "Ver detalle");
        }

        private void CargarGrilla(List<BEProducto> productos, string mensajeVacio)
        {
            grillaProductos.Cargar(productos, mensajeVacio);

            lblCantidad.Text = grillaProductos.CantidadRegistros == 1
                ? "1 producto"
                : string.Format("{0} productos", grillaProductos.CantidadRegistros);
        }

        private void CargarCombos(List<BEProducto> productos)
        {
            CargarOpciones(ddlCategoria, productos.Select(producto => producto.Categoria));
            CargarOpciones(ddlEstado, productos.Select(producto => producto.Estado));
        }

        private void CargarOpciones(DropDownList lista, IEnumerable<string> valores)
        {
            lista.Items.Clear();
            lista.Items.Add(new ListItem("Todos", string.Empty));

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
                : "Sin sincronizar");

            pnlDetalleProducto.Visible = true;
        }

        private void OcultarDetalle()
        {
            pnlDetalleProducto.Visible = false;
        }

        private string ObtenerTextoStock(BEProducto producto)
        {
            return producto.StockBajoMinimo
                ? string.Format("{0} (por debajo del mínimo)", producto.Stock)
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
