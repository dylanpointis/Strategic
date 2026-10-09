using BE;
using DAL;
using Services;
using System;
using System.Collections.Generic;

namespace BLL
{
    public class BLLProducto
    {
        private readonly DALProducto dalProducto = new DALProducto();

        public List<BEProducto> TraerListaProductos()
        {
            return dalProducto.TraerListaProductos();
        }

        public List<BEProducto> FiltrarProductos(string nombre, string categoria, string estado, decimal? precioMinimo, decimal? precioMaximo)
        {
            if (precioMinimo.HasValue && precioMinimo.Value < 0)
            {
                throw new Exception(Traductor.T("error.precioMinimoNegativo"));
            }

            if (precioMaximo.HasValue && precioMaximo.Value < 0)
            {
                throw new Exception(Traductor.T("error.precioMaximoNegativo"));
            }

            if (precioMinimo.HasValue && precioMaximo.HasValue && precioMaximo.Value < precioMinimo.Value)
            {
                throw new Exception(Traductor.T("error.precioMaximoMenor"));
            }

            return dalProducto.FiltrarProductos(
                Normalizar(nombre),
                Normalizar(categoria),
                Normalizar(estado),
                precioMinimo,
                precioMaximo);
        }

        public BEProducto TraerProductoPorId(int idProducto)
        {
            if (idProducto <= 0)
            {
                throw new Exception(Traductor.T("error.productoInvalido"));
            }

            return dalProducto.TraerProductoPorId(idProducto);
        }

        public List<BEHistorialPrecio> TraerHistorialPrecios(int idProducto, DateTime? fechaInicio, DateTime? fechaFin)
        {
            ValidarConsultaHistorial(idProducto, fechaInicio, fechaFin);

            return dalProducto.TraerHistorialPrecios(idProducto, fechaInicio, fechaFin);
        }

        public List<BEHistorialStock> TraerHistorialStock(int idProducto, DateTime? fechaInicio, DateTime? fechaFin)
        {
            ValidarConsultaHistorial(idProducto, fechaInicio, fechaFin);

            return dalProducto.TraerHistorialStock(idProducto, fechaInicio, fechaFin);
        }

        private void ValidarConsultaHistorial(int idProducto, DateTime? fechaInicio, DateTime? fechaFin)
        {
            if (idProducto <= 0)
            {
                throw new Exception(Traductor.T("error.seleccioneProductoHistorial"));
            }

            if (fechaInicio.HasValue && fechaFin.HasValue && fechaFin.Value < fechaInicio.Value)
            {
                throw new Exception(Traductor.T("filtroFechas.errorRango"));
            }
        }

        private string Normalizar(string valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }
    }
}
