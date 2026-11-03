using System;
using System.Collections.Generic;
using System.Linq;

namespace BE
{
    // Reporte del CU-002-008. Segun el tipo se completan los datos de ventas,
    // de stock o de competencia; el encabezado es comun a los tres
    public class BEReporte
    {
        public const string TipoVentas = "Ventas";
        public const string TipoStock = "Stock";
        public const string TipoCompetencia = "Competencia";

        public string Tipo { get; set; }
        public DateTime FechaGeneracion { get; set; }
        public DateTime? PeriodoDesde { get; set; }
        public DateTime? PeriodoHasta { get; set; }

        // Null significa todas las categorias
        public string Categoria { get; set; }

        #region Ventas

        public BEResumenDashboard ResumenVentas { get; set; }
        public List<BEVentaPorCategoria> VentasPorCategoria { get; set; }
        public List<BETopProducto> TopProductos { get; set; }

        public decimal PromedioPorVenta
        {
            get
            {
                return ResumenVentas != null && ResumenVentas.CantidadVentas > 0
                    ? Math.Round(ResumenVentas.FacturacionTotal / ResumenVentas.CantidadVentas, 2)
                    : 0;
            }
        }

        #endregion

        #region Stock

        public List<BEReporteStock> Stock { get; set; }

        public int StockTotal
        {
            get { return Stock == null ? 0 : Stock.Sum(p => p.Stock); }
        }

        public int ProductosBajoMinimo
        {
            get { return Stock == null ? 0 : Stock.Count(p => p.BajoMinimo); }
        }

        public int ProductosSinStock
        {
            get { return Stock == null ? 0 : Stock.Count(p => p.Stock <= 0); }
        }

        public int UnidadesVendidasStock
        {
            get { return Stock == null ? 0 : Stock.Sum(p => p.UnidadesVendidas); }
        }

        #endregion

        #region Competencia

        public List<BEComparacionPrecio> Comparaciones { get; set; }
        public BEResumenComparacion ResumenCompetencia { get; set; }

        #endregion

        // Registros del detalle: si no hay ninguno no hay reporte que mostrar
        public int CantidadRegistros
        {
            get
            {
                switch (Tipo)
                {
                    case TipoVentas:
                        return ResumenVentas == null ? 0 : ResumenVentas.CantidadVentas;
                    case TipoStock:
                        return Stock == null ? 0 : Stock.Count;
                    case TipoCompetencia:
                        return Comparaciones == null ? 0 : Comparaciones.Count;
                    default:
                        return 0;
                }
            }
        }
    }
}
