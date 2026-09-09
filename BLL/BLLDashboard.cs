using BE;
using DAL;
using System;
using System.Collections.Generic;

namespace BLL
{
    public class BLLDashboard
    {
        // Cantidad de productos que entran en el ranking de mas vendidos
        private const int TopProductosMostrados = 10;

        // Porciones que se muestran en la torta antes de agrupar el resto en "Otras"
        private const int CategoriasEnGrafico = 5;

        private readonly DALDashboard dalDashboard = new DALDashboard();
        private readonly DALProducto dalProducto = new DALProducto();

        public BEResumenDashboard TraerResumen(DateTime? fechaInicio, DateTime? fechaFin, string categoria)
        {
            ValidarFiltros(fechaInicio, fechaFin);

            return dalDashboard.TraerResumen(fechaInicio, fechaFin, Normalizar(categoria));
        }

        public List<BEVentaPorCategoria> TraerVentasPorCategoria(DateTime? fechaInicio, DateTime? fechaFin, string categoria)
        {
            ValidarFiltros(fechaInicio, fechaFin);

            return dalDashboard.TraerVentasPorCategoria(fechaInicio, fechaFin, Normalizar(categoria));
        }

        public List<BETopProducto> TraerTopProductos(DateTime? fechaInicio, DateTime? fechaFin, string categoria)
        {
            ValidarFiltros(fechaInicio, fechaFin);

            return dalDashboard.TraerTopProductos(fechaInicio, fechaFin, Normalizar(categoria), TopProductosMostrados);
        }

        /// <summary>
        /// Deja la torta de categorías en una cantidad legible de porciones: conserva
        /// las más vendidas y suma el resto en una porción "Otras".
        /// Espera la lista ya ordenada por unidades de mayor a menor, que es como la
        /// devuelve TraerVentasPorCategoria.
        /// </summary>
        public List<BEVentaPorCategoria> AgruparCategoriasMenores(List<BEVentaPorCategoria> categorias)
        {
            // Si sobra una sola categoria no vale la pena agruparla: se muestran todas
            if (categorias.Count <= CategoriasEnGrafico + 1)
            {
                return new List<BEVentaPorCategoria>(categorias);
            }

            List<BEVentaPorCategoria> agrupadas = new List<BEVentaPorCategoria>();
            BEVentaPorCategoria otras = new BEVentaPorCategoria();

            otras.Categoria = "Otras";

            for (int i = 0; i < categorias.Count; i++)
            {
                if (i < CategoriasEnGrafico)
                {
                    agrupadas.Add(categorias[i]);
                    continue;
                }

                otras.Unidades = otras.Unidades + categorias[i].Unidades;
                otras.Monto = otras.Monto + categorias[i].Monto;
            }

            agrupadas.Add(otras);

            return agrupadas;
        }

        /// <summary>
        /// Productos cuyo stock quedó en el mínimo definido por el cliente o por debajo.
        /// Es la foto actual del stock, no depende del rango de fechas.
        /// </summary>
        public List<BEProducto> TraerProductosBajoStock(string categoria)
        {
            return dalProducto.TraerProductosBajoStock(Normalizar(categoria));
        }

        /// <summary>
        /// Período que el dashboard muestra por defecto: el mes en curso.
        /// </summary>
        public void ObtenerPeriodoPorDefecto(out DateTime fechaInicio, out DateTime fechaFin)
        {
            DateTime hoy = DateTime.Today;

            fechaInicio = new DateTime(hoy.Year, hoy.Month, 1);
            fechaFin = hoy;
        }

        private void ValidarFiltros(DateTime? fechaInicio, DateTime? fechaFin)
        {
            if (fechaInicio.HasValue && fechaFin.HasValue && fechaFin.Value < fechaInicio.Value)
            {
                throw new Exception("La fecha hasta no puede ser anterior a la fecha desde");
            }
        }

        private string Normalizar(string valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }
    }
}
