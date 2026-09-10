using BE;
using DAL;
using System;
using System.Collections.Generic;

namespace BLL
{
    public class BLLCompetencia
    {
        private readonly DALCompetencia dalCompetencia = new DALCompetencia();

        public List<BECompetencia> TraerListaCompetidores()
        {
            return dalCompetencia.TraerListaCompetidores();
        }

        public List<BEComparacionPrecio> TraerComparacionPrecios()
        {
            return dalCompetencia.TraerComparacionPrecios();
        }

        public List<BEComparacionPrecio> FiltrarComparacionPrecios(int? idProducto, string categoria, int? idCompetencia)
        {
            if (idProducto.HasValue && idProducto.Value <= 0)
            {
                throw new Exception("Debe seleccionar un producto valido");
            }

            if (idCompetencia.HasValue && idCompetencia.Value <= 0)
            {
                throw new Exception("Debe seleccionar un competidor valido");
            }

            return dalCompetencia.FiltrarComparacionPrecios(idProducto, Normalizar(categoria), idCompetencia);
        }

        public BEComparacionPrecio TraerComparacionPrecioPorId(int idProductoCompetencia)
        {
            ValidarMapeo(idProductoCompetencia);

            return dalCompetencia.TraerComparacionPrecioPorId(idProductoCompetencia);
        }

        public List<BEHistorialPrecioCompetencia> TraerHistorialPrecioCompetencia(int idProductoCompetencia)
        {
            ValidarMapeo(idProductoCompetencia);

            return dalCompetencia.TraerHistorialPrecioCompetencia(idProductoCompetencia);
        }

        /// <summary>
        /// Arma los indicadores de la comparacion que se esta viendo. Se calcula
        /// sobre la lista ya consultada para no repetir el filtro en la base.
        /// </summary>
        public BEResumenComparacion CalcularResumen(List<BEComparacionPrecio> comparaciones)
        {
            BEResumenComparacion resumen = new BEResumenComparacion();

            if (comparaciones == null)
            {
                return resumen;
            }

            resumen.PublicacionesMonitoreadas = comparaciones.Count;

            decimal sumaDiferencias = 0;
            int conPrecio = 0;

            foreach (BEComparacionPrecio comparacion in comparaciones)
            {
                if (!comparacion.DiferenciaPorcentaje.HasValue)
                {
                    resumen.SinPrecioRegistrado = resumen.SinPrecioRegistrado + 1;
                    continue;
                }

                decimal diferencia = comparacion.DiferenciaPorcentaje.Value;

                sumaDiferencias = sumaDiferencias + diferencia;
                conPrecio = conPrecio + 1;

                if (diferencia > 0)
                {
                    resumen.ProductosMasCaros = resumen.ProductosMasCaros + 1;
                }
                else if (diferencia < 0)
                {
                    resumen.ProductosMasBaratos = resumen.ProductosMasBaratos + 1;
                }
            }

            // Las publicaciones sin precio relevado no entran en el promedio:
            // no aportan una diferencia y bajarian el valor sin motivo
            resumen.DiferenciaPromedio = conPrecio > 0
                ? Math.Round(sumaDiferencias / conPrecio, 2)
                : 0;

            return resumen;
        }

        private void ValidarMapeo(int idProductoCompetencia)
        {
            if (idProductoCompetencia <= 0)
            {
                throw new Exception("Debe seleccionar una publicacion de la competencia");
            }
        }

        private string Normalizar(string valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }
    }
}
