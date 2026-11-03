using BE;
using BLL;
using Services;
using Strategic.Seguridad;
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;

namespace Strategic
{
    // CU-002-008 - Generar Reporte. Se elige el tipo, el periodo y la
    // categoria, se ve el reporte en pantalla y se exporta a XML
    public partial class Reportes : PaginaSegura
    {
        private readonly BLLReporte bllReporte = new BLLReporte();
        private readonly BLLDashboard bllDashboard = new BLLDashboard();

        // Criterios del ultimo reporte generado: la exportacion y el cambio de
        // idioma lo vuelven a armar con estos y no con lo que haya en los filtros
        private string TipoGenerado
        {
            get { return Convert.ToString(ViewState["TipoGenerado"]); }
            set { ViewState["TipoGenerado"] = value; }
        }

        private DateTime? DesdeGenerado
        {
            get { return ViewState["DesdeGenerado"] as DateTime?; }
            set { ViewState["DesdeGenerado"] = value; }
        }

        private DateTime? HastaGenerado
        {
            get { return ViewState["HastaGenerado"] as DateTime?; }
            set { ViewState["HastaGenerado"] = value; }
        }

        private string CategoriaGenerada
        {
            get { return Convert.ToString(ViewState["CategoriaGenerada"]); }
            set { ViewState["CategoriaGenerada"] = value; }
        }

        public override void ActualizarIdioma()
        {
            Title = Traductor.T("reportes.titulo");
            lblTipo.Text = Traductor.T("reportes.tipo");
            lblCategoria.Text = Traductor.T("productos.categoria");
            btnGenerar.Text = Traductor.T("reportes.generar");
            btnLimpiar.Text = Traductor.T("comun.limpiarFiltros");
            btnExportar.Text = Traductor.T("reportes.exportarXml");

            // Las opciones armadas desde codigo existen recien con el ViewState recuperado
            foreach (ListItem tipo in ddlTipo.Items)
            {
                tipo.Text = Traductor.T("reportes.tipo." + tipo.Value);
            }

            if (ddlCategoria.Items.Count > 0)
            {
                ddlCategoria.Items[0].Text = Traductor.T("dashboard.todasLasCategorias");
            }

            // Con el idioma cambiado se vuelve a armar para traducir metricas y columnas
            if (IsPostBack && !string.IsNullOrEmpty(TipoGenerado))
            {
                try
                {
                    MostrarReporte(GenerarConCriteriosGuardados());
                }
                catch (Exception ex)
                {
                    OcultarReporte();
                    lblError.Text = Server.HtmlEncode(ex.Message);
                }
            }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            // Las columnas se arman en el Init para que las grillas puedan
            // reconstruirse en cada postback a partir de su ViewState
            ConfigurarGrillas();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarFiltros();
            }
        }

        protected void btnGenerar_Click(object sender, EventArgs e)
        {
            if (!filtroFechas.Validar())
            {
                return;
            }

            try
            {
                BEReporte reporte = bllReporte.GenerarReporte(
                    ddlTipo.SelectedValue,
                    filtroFechas.FechaDesde,
                    filtroFechas.FechaHasta,
                    ddlCategoria.SelectedValue);

                TipoGenerado = reporte.Tipo;
                DesdeGenerado = reporte.PeriodoDesde;
                HastaGenerado = reporte.PeriodoHasta;
                CategoriaGenerada = reporte.Categoria;

                MostrarReporte(reporte);
            }
            catch (Exception ex)
            {
                OcultarReporte();
                lblError.Text = Server.HtmlEncode(ex.Message);
            }
        }

        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            ddlTipo.SelectedIndex = 0;
            ddlCategoria.SelectedIndex = 0;
            EstablecerPeriodoPorDefecto();
            OcultarReporte();
        }

        protected void btnExportar_Click(object sender, EventArgs e)
        {
            byte[] archivo;
            string nombre;

            try
            {
                BEReporte reporte = GenerarConCriteriosGuardados();

                archivo = bllReporte.ExportarXml(reporte);
                nombre = bllReporte.NombreArchivo(reporte);
            }
            catch (Exception ex)
            {
                lblError.Text = Server.HtmlEncode(ex.Message);
                return;
            }

            Response.Clear();
            Response.ContentType = "application/xml";
            Response.AppendHeader("Content-Disposition", "attachment; filename=" + nombre);
            Response.AppendHeader("Content-Length", archivo.Length.ToString());
            Response.BinaryWrite(archivo);
            Response.Flush();
            Response.SuppressContent = true;
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void rptMetricas_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
            {
                return;
            }

            KeyValuePair<string, string> metrica = (KeyValuePair<string, string>)e.Item.DataItem;

            ((Label)e.Item.FindControl("lblNombre")).Text = Traductor.T(metrica.Key);
            ((Label)e.Item.FindControl("lblValor")).Text = Server.HtmlEncode(metrica.Value);
        }

        private void ConfigurarGrillas()
        {
            grillaCategorias.ClavePrimaria = "Categoria";
            grillaCategorias.FilasPorPagina = 10;
            grillaCategorias.AgregarColumna("Categoria", "productos.categoria");
            grillaCategorias.AgregarColumna("Unidades", "reportes.unidadesVendidas", "celda-numero");
            grillaCategorias.AgregarColumna("Monto", "reportes.montoTotal", "celda-numero", "${0:N2}");

            grillaTopProductos.ClavePrimaria = "IdProducto";
            grillaTopProductos.FilasPorPagina = 10;
            grillaTopProductos.AgregarColumna("Codigo", "productos.codigo", "celda-codigo");
            grillaTopProductos.AgregarColumna("Nombre", "comun.nombre");
            grillaTopProductos.AgregarColumna("Categoria", "productos.categoria");
            grillaTopProductos.AgregarColumna("Unidades", "reportes.unidadesVendidas", "celda-numero");
            grillaTopProductos.AgregarColumna("Monto", "reportes.montoTotal", "celda-numero", "${0:N2}");

            grillaStock.ClavePrimaria = "IdProducto";
            grillaStock.FilasPorPagina = 10;
            grillaStock.MostrarSelectorFilas = true;
            grillaStock.AgregarColumna("Codigo", "productos.codigo", "celda-codigo");
            grillaStock.AgregarColumna("Nombre", "comun.nombre");
            grillaStock.AgregarColumna("Categoria", "productos.categoria");
            grillaStock.AgregarColumna("Stock", "reportes.stockActual", "celda-numero");
            grillaStock.AgregarColumna("StockMinimo", "reportes.stockMinimo", "celda-numero");
            grillaStock.AgregarColumna("StockMaximo", "reportes.stockMaximo", "celda-numero");
            grillaStock.AgregarColumna("UnidadesVendidas", "reportes.unidadesVendidas", "celda-numero");
            grillaStock.AgregarColumnaTraducida("SituacionTexto", "reportes.situacion");

            grillaCompetencia.ClavePrimaria = "IdProductoCompetencia";
            grillaCompetencia.FilasPorPagina = 10;
            grillaCompetencia.MostrarSelectorFilas = true;
            grillaCompetencia.AgregarColumna("NombreProducto", "monitoreo.producto");
            grillaCompetencia.AgregarColumna("NombreCompetidor", "mapearProductos.competidor");
            grillaCompetencia.AgregarColumna("PrecioPropio", "reportes.precioPropio", "celda-numero", "${0:N2}");
            grillaCompetencia.AgregarColumna("PrecioCompetencia", "reportes.precioCompetencia", "celda-numero", "${0:N2}");
            grillaCompetencia.AgregarColumna("DiferenciaPorcentaje", "reportes.diferencia", "celda-numero", "{0:N2}%");
            grillaCompetencia.AgregarColumnaTraducida("Posicion", "reportes.posicion");
        }

        private void CargarFiltros()
        {
            ddlTipo.Items.Clear();

            foreach (string tipo in BLLReporte.Tipos)
            {
                ddlTipo.Items.Add(new ListItem(Traductor.T("reportes.tipo." + tipo), tipo));
            }

            ddlCategoria.Items.Clear();
            ddlCategoria.Items.Add(new ListItem(Traductor.T("dashboard.todasLasCategorias"), string.Empty));

            List<string> categorias = bllReporte.TraerCategorias();

            foreach (string categoria in categorias)
            {
                ddlCategoria.Items.Add(new ListItem(categoria, categoria));
            }

            EstablecerPeriodoPorDefecto();

            // Alternativa 4.2 [#ERR017]: sin productos sincronizados no hay categorias ni datos
            pnlSinDatos.Visible = new BLLProducto().TraerListaProductos().Count == 0;
        }

        private void EstablecerPeriodoPorDefecto()
        {
            DateTime desde;
            DateTime hasta;

            bllDashboard.ObtenerPeriodoPorDefecto(out desde, out hasta);
            filtroFechas.EstablecerRango(desde, hasta);
        }

        private BEReporte GenerarConCriteriosGuardados()
        {
            return bllReporte.GenerarReporte(TipoGenerado, DesdeGenerado, HastaGenerado, CategoriaGenerada);
        }

        private void MostrarReporte(BEReporte reporte)
        {
            lblTituloReporte.Text = Traductor.T("reportes.reporteDe", Traductor.T("reportes.tipo." + reporte.Tipo));
            lblPeriodo.Text = Server.HtmlEncode(TextoPeriodo(reporte.PeriodoDesde, reporte.PeriodoHasta)
                + " · " + (reporte.Categoria ?? Traductor.T("dashboard.todasLasCategorias")));

            rptMetricas.DataSource = ArmarMetricas(reporte);
            rptMetricas.DataBind();

            pnlVentas.Visible = reporte.Tipo == BEReporte.TipoVentas;
            pnlStock.Visible = reporte.Tipo == BEReporte.TipoStock;
            pnlCompetencia.Visible = reporte.Tipo == BEReporte.TipoCompetencia;

            switch (reporte.Tipo)
            {
                case BEReporte.TipoVentas:
                    grillaCategorias.Cargar(reporte.VentasPorCategoria, "comun.sinResultadosFiltros");
                    grillaTopProductos.Cargar(reporte.TopProductos, "comun.sinResultadosFiltros");
                    break;
                case BEReporte.TipoStock:
                    grillaStock.Cargar(reporte.Stock, "comun.sinResultadosFiltros");
                    break;
                case BEReporte.TipoCompetencia:
                    grillaCompetencia.Cargar(reporte.Comparaciones, "comun.sinResultadosFiltros");
                    break;
            }

            pnlReporte.Visible = true;
        }

        // Clave de traduccion del nombre de la metrica y su valor ya formateado
        private List<KeyValuePair<string, string>> ArmarMetricas(BEReporte reporte)
        {
            List<KeyValuePair<string, string>> metricas = new List<KeyValuePair<string, string>>();

            switch (reporte.Tipo)
            {
                case BEReporte.TipoVentas:
                    metricas.Add(Metrica("reportes.metrica.cantidadVentas", reporte.ResumenVentas.CantidadVentas.ToString("N0")));
                    metricas.Add(Metrica("reportes.metrica.unidadesVendidas", reporte.ResumenVentas.UnidadesVendidas.ToString("N0")));
                    metricas.Add(Metrica("reportes.metrica.facturacionTotal", string.Format("${0:N2}", reporte.ResumenVentas.FacturacionTotal)));
                    metricas.Add(Metrica("reportes.metrica.promedioPorVenta", string.Format("${0:N2}", reporte.PromedioPorVenta)));
                    break;

                case BEReporte.TipoStock:
                    metricas.Add(Metrica("reportes.metrica.cantidadProductos", reporte.Stock.Count.ToString("N0")));
                    metricas.Add(Metrica("reportes.metrica.stockTotal", reporte.StockTotal.ToString("N0")));
                    metricas.Add(Metrica("reportes.metrica.bajoMinimo", reporte.ProductosBajoMinimo.ToString("N0")));
                    metricas.Add(Metrica("reportes.metrica.sinStock", reporte.ProductosSinStock.ToString("N0")));
                    metricas.Add(Metrica("reportes.metrica.unidadesVendidas", reporte.UnidadesVendidasStock.ToString("N0")));
                    break;

                case BEReporte.TipoCompetencia:
                    metricas.Add(Metrica("reportes.metrica.publicaciones", reporte.ResumenCompetencia.PublicacionesMonitoreadas.ToString("N0")));
                    metricas.Add(Metrica("reportes.metrica.masCaros", reporte.ResumenCompetencia.ProductosMasCaros.ToString("N0")));
                    metricas.Add(Metrica("reportes.metrica.masBaratos", reporte.ResumenCompetencia.ProductosMasBaratos.ToString("N0")));
                    metricas.Add(Metrica("reportes.metrica.diferenciaPromedio", string.Format("{0:N2}%", reporte.ResumenCompetencia.DiferenciaPromedio)));
                    break;
            }

            return metricas;
        }

        private KeyValuePair<string, string> Metrica(string clave, string valor)
        {
            return new KeyValuePair<string, string>(clave, valor);
        }

        // Las dos fechas son opcionales: sin ninguna el reporte toma todo lo registrado
        private string TextoPeriodo(DateTime? desde, DateTime? hasta)
        {
            if (desde.HasValue && hasta.HasValue)
            {
                return Traductor.T("reportes.periodo", desde.Value.ToString("dd/MM/yyyy"), hasta.Value.ToString("dd/MM/yyyy"));
            }

            if (desde.HasValue)
            {
                return Traductor.T("reportes.periodoDesde", desde.Value.ToString("dd/MM/yyyy"));
            }

            return hasta.HasValue
                ? Traductor.T("reportes.periodoHasta", hasta.Value.ToString("dd/MM/yyyy"))
                : Traductor.T("reportes.periodoCompleto");
        }

        private void OcultarReporte()
        {
            pnlReporte.Visible = false;
            TipoGenerado = null;
            DesdeGenerado = null;
            HastaGenerado = null;
            CategoriaGenerada = null;
        }
    }
}
