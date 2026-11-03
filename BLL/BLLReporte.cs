using BE;
using DAL;
using Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace BLL
{
    // CU-002-008 - Generar Reporte. Arma el reporte de ventas, stock o
    // competencia y lo exporta a XML. Es una consulta: no registra eventos
    public class BLLReporte
    {
        // El XML usa formatos fijos, sin depender del idioma de la pantalla
        private static readonly CultureInfo Invariante = CultureInfo.InvariantCulture;

        private readonly BLLDashboard bllDashboard = new BLLDashboard();
        private readonly BLLCompetencia bllCompetencia = new BLLCompetencia();
        private readonly BLLProducto bllProducto = new BLLProducto();
        private readonly DALReporte dalReporte = new DALReporte();

        public static readonly string[] Tipos = { BEReporte.TipoVentas, BEReporte.TipoStock, BEReporte.TipoCompetencia };

        #region Generacion

        public BEReporte GenerarReporte(string tipo, DateTime? fechaInicio, DateTime? fechaFin, string categoria)
        {
            if (!Tipos.Contains(tipo))
            {
                throw new Exception(Traductor.T("error.tipoReporteInvalido"));
            }

            if (fechaInicio.HasValue && fechaFin.HasValue && fechaFin.Value < fechaInicio.Value)
            {
                throw new Exception(Traductor.T("filtroFechas.errorRango"));
            }

            // Alternativa 4.2 [#ERR017]
            if (bllProducto.TraerListaProductos().Count == 0)
            {
                throw new Exception(Traductor.T("productos.sinProductos"));
            }

            categoria = string.IsNullOrWhiteSpace(categoria) ? null : categoria.Trim();

            BEReporte reporte = new BEReporte
            {
                Tipo = tipo,
                FechaGeneracion = DateTime.Now,
                PeriodoDesde = fechaInicio,
                PeriodoHasta = fechaFin,
                Categoria = categoria
            };

            // Alternativa 4.3 [#ERR018]
            try
            {
                CargarDatos(reporte);
            }
            catch (Exception)
            {
                throw new Exception(Traductor.T("error.procesarReporte"));
            }

            // Alternativa 4.1 [#ERR013]
            if (reporte.CantidadRegistros == 0)
            {
                throw new Exception(Traductor.T("comun.sinResultadosFiltros"));
            }

            return reporte;
        }

        // Categorias de los productos sincronizados, para el filtro de la pantalla
        public List<string> TraerCategorias()
        {
            return bllProducto.TraerListaProductos()
                .Select(producto => producto.Categoria)
                .Where(categoria => !string.IsNullOrWhiteSpace(categoria))
                .Distinct()
                .OrderBy(categoria => categoria)
                .ToList();
        }

        private void CargarDatos(BEReporte reporte)
        {
            switch (reporte.Tipo)
            {
                case BEReporte.TipoVentas:
                    reporte.ResumenVentas = bllDashboard.TraerResumen(reporte.PeriodoDesde, reporte.PeriodoHasta, reporte.Categoria);
                    reporte.VentasPorCategoria = bllDashboard.TraerVentasPorCategoria(reporte.PeriodoDesde, reporte.PeriodoHasta, reporte.Categoria);
                    reporte.TopProductos = bllDashboard.TraerTopProductos(reporte.PeriodoDesde, reporte.PeriodoHasta, reporte.Categoria);
                    break;

                case BEReporte.TipoStock:
                    reporte.Stock = dalReporte.TraerReporteStock(reporte.PeriodoDesde, reporte.PeriodoHasta, reporte.Categoria);
                    break;

                case BEReporte.TipoCompetencia:
                    // El periodo filtra por la fecha del ultimo precio relevado: un
                    // mapeo sin relevamientos solo entra si no se pidieron fechas
                    reporte.Comparaciones = bllCompetencia.FiltrarComparacionPrecios(null, reporte.Categoria, null)
                        .Where(c => DentroDelPeriodo(c.FechaConsulta, reporte.PeriodoDesde, reporte.PeriodoHasta))
                        .ToList();
                    reporte.ResumenCompetencia = bllCompetencia.CalcularResumen(reporte.Comparaciones);
                    break;
            }
        }

        private bool DentroDelPeriodo(DateTime? fecha, DateTime? desde, DateTime? hasta)
        {
            if (!desde.HasValue && !hasta.HasValue)
            {
                return true;
            }

            if (!fecha.HasValue)
            {
                return false;
            }

            return (!desde.HasValue || fecha.Value.Date >= desde.Value.Date)
                && (!hasta.HasValue || fecha.Value.Date <= hasta.Value.Date);
        }

        #endregion

        #region Exportacion

        public byte[] ExportarXml(BEReporte reporte)
        {
            // Alternativa 6.1 [#ERR020]
            try
            {
                return GenerarXml(reporte);
            }
            catch (Exception)
            {
                throw new Exception(Traductor.T("error.exportarReporte"));
            }
        }

        public string NombreArchivo(BEReporte reporte)
        {
            return string.Format("Reporte-{0}-{1:yyyyMMdd-HHmm}.xml", reporte.Tipo, reporte.FechaGeneracion);
        }

        // <Reporte><Encabezado/><MetricasGenerales/> y el detalle del tipo
        private byte[] GenerarXml(BEReporte reporte)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                // Se escribe en forma secuencial: encabezado, metricas y detalle
                XmlTextWriter escritor = new XmlTextWriter(stream, new UTF8Encoding(false));
                escritor.Formatting = Formatting.Indented;

                escritor.WriteStartDocument();
                escritor.WriteStartElement("Reporte");

                escritor.WriteStartElement("Encabezado");
                escritor.WriteElementString("FechaGeneracion", reporte.FechaGeneracion.ToString("yyyy-MM-ddTHH:mm:ss"));
                escritor.WriteElementString("PeriodoDesde", Fecha(reporte.PeriodoDesde));
                escritor.WriteElementString("PeriodoHasta", Fecha(reporte.PeriodoHasta));
                escritor.WriteElementString("TipoReporte", reporte.Tipo);
                escritor.WriteElementString("Categoria", reporte.Categoria ?? string.Empty);
                escritor.WriteEndElement();

                escritor.WriteStartElement("MetricasGenerales");

                foreach (KeyValuePair<string, string> metrica in MetricasXml(reporte))
                {
                    escritor.WriteElementString(metrica.Key, metrica.Value);
                }

                escritor.WriteEndElement();

                switch (reporte.Tipo)
                {
                    case BEReporte.TipoVentas:
                        EscribirDetalleVentas(escritor, reporte);
                        break;
                    case BEReporte.TipoStock:
                        EscribirDetalleStock(escritor, reporte);
                        break;
                    case BEReporte.TipoCompetencia:
                        EscribirDetalleCompetencia(escritor, reporte);
                        break;
                }

                escritor.WriteEndElement();
                escritor.WriteEndDocument();
                escritor.Flush();

                return stream.ToArray();
            }
        }

        private List<KeyValuePair<string, string>> MetricasXml(BEReporte reporte)
        {
            List<KeyValuePair<string, string>> metricas = new List<KeyValuePair<string, string>>();

            switch (reporte.Tipo)
            {
                case BEReporte.TipoVentas:
                    metricas.Add(Par("CantidadVentas", reporte.ResumenVentas.CantidadVentas));
                    metricas.Add(Par("UnidadesVendidas", reporte.ResumenVentas.UnidadesVendidas));
                    metricas.Add(Par("FacturacionTotal", Numero(reporte.ResumenVentas.FacturacionTotal)));
                    metricas.Add(Par("PromedioPorVenta", Numero(reporte.PromedioPorVenta)));
                    break;

                case BEReporte.TipoStock:
                    metricas.Add(Par("CantidadProductos", reporte.Stock.Count));
                    metricas.Add(Par("StockTotal", reporte.StockTotal));
                    metricas.Add(Par("ProductosBajoMinimo", reporte.ProductosBajoMinimo));
                    metricas.Add(Par("ProductosSinStock", reporte.ProductosSinStock));
                    metricas.Add(Par("UnidadesVendidas", reporte.UnidadesVendidasStock));
                    break;

                case BEReporte.TipoCompetencia:
                    metricas.Add(Par("PublicacionesMonitoreadas", reporte.ResumenCompetencia.PublicacionesMonitoreadas));
                    metricas.Add(Par("ProductosMasCaros", reporte.ResumenCompetencia.ProductosMasCaros));
                    metricas.Add(Par("ProductosMasBaratos", reporte.ResumenCompetencia.ProductosMasBaratos));
                    metricas.Add(Par("SinPrecioRegistrado", reporte.ResumenCompetencia.SinPrecioRegistrado));
                    metricas.Add(Par("DiferenciaPromedio", Numero(reporte.ResumenCompetencia.DiferenciaPromedio)));
                    break;
            }

            return metricas;
        }

        private void EscribirDetalleVentas(XmlTextWriter escritor, BEReporte reporte)
        {
            escritor.WriteStartElement("VentasPorCategoria");

            foreach (BEVentaPorCategoria categoria in reporte.VentasPorCategoria)
            {
                escritor.WriteStartElement("Categoria");
                escritor.WriteElementString("Nombre", categoria.Categoria);
                escritor.WriteElementString("UnidadesVendidas", categoria.Unidades.ToString(Invariante));
                escritor.WriteElementString("MontoTotal", Numero(categoria.Monto));
                escritor.WriteEndElement();
            }

            escritor.WriteEndElement();

            escritor.WriteStartElement("TopProductos");

            foreach (BETopProducto producto in reporte.TopProductos)
            {
                escritor.WriteStartElement("Producto");
                escritor.WriteElementString("Codigo", producto.Codigo);
                escritor.WriteElementString("Nombre", producto.Nombre);
                escritor.WriteElementString("Categoria", producto.Categoria);
                escritor.WriteElementString("UnidadesVendidas", producto.Unidades.ToString(Invariante));
                escritor.WriteElementString("MontoTotal", Numero(producto.Monto));
                escritor.WriteEndElement();
            }

            escritor.WriteEndElement();
        }

        private void EscribirDetalleStock(XmlTextWriter escritor, BEReporte reporte)
        {
            escritor.WriteStartElement("Productos");

            foreach (BEReporteStock producto in reporte.Stock)
            {
                escritor.WriteStartElement("Producto");
                escritor.WriteElementString("Codigo", producto.Codigo);
                escritor.WriteElementString("Nombre", producto.Nombre);
                escritor.WriteElementString("Categoria", producto.Categoria);
                escritor.WriteElementString("StockActual", producto.Stock.ToString(Invariante));
                escritor.WriteElementString("StockMinimo", Entero(producto.StockMinimo));
                escritor.WriteElementString("StockMaximo", Entero(producto.StockMaximo));
                escritor.WriteElementString("UnidadesVendidas", producto.UnidadesVendidas.ToString(Invariante));
                escritor.WriteElementString("Situacion", producto.Situacion);
                escritor.WriteEndElement();
            }

            escritor.WriteEndElement();
        }

        private void EscribirDetalleCompetencia(XmlTextWriter escritor, BEReporte reporte)
        {
            escritor.WriteStartElement("Comparaciones");

            foreach (BEComparacionPrecio comparacion in reporte.Comparaciones)
            {
                escritor.WriteStartElement("Comparacion");
                escritor.WriteElementString("CodigoProducto", comparacion.CodigoProducto);
                escritor.WriteElementString("Producto", comparacion.NombreProducto);
                escritor.WriteElementString("Categoria", comparacion.Categoria);
                escritor.WriteElementString("Competidor", comparacion.NombreCompetidor);
                escritor.WriteElementString("Marketplace", comparacion.Marketplace);
                escritor.WriteElementString("PrecioPropio", Numero(comparacion.PrecioPropio));
                escritor.WriteElementString("PrecioCompetencia", comparacion.PrecioCompetencia.HasValue ? Numero(comparacion.PrecioCompetencia.Value) : string.Empty);
                escritor.WriteElementString("DiferenciaPorcentaje", comparacion.DiferenciaPorcentaje.HasValue ? Numero(comparacion.DiferenciaPorcentaje.Value) : string.Empty);
                escritor.WriteElementString("FechaConsulta", Fecha(comparacion.FechaConsulta));
                escritor.WriteEndElement();
            }

            escritor.WriteEndElement();
        }

        private KeyValuePair<string, string> Par(string nombre, int valor)
        {
            return new KeyValuePair<string, string>(nombre, valor.ToString(Invariante));
        }

        private KeyValuePair<string, string> Par(string nombre, string valor)
        {
            return new KeyValuePair<string, string>(nombre, valor);
        }

        private string Numero(decimal valor)
        {
            return valor.ToString("0.00", Invariante);
        }

        private string Entero(int? valor)
        {
            return valor.HasValue ? valor.Value.ToString(Invariante) : string.Empty;
        }

        private string Fecha(DateTime? fecha)
        {
            return fecha.HasValue ? fecha.Value.ToString("yyyy-MM-dd") : string.Empty;
        }

        #endregion
    }
}
