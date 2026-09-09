using BE;
using DAL;
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
                throw new Exception("El precio minimo no puede ser negativo");
            }

            if (precioMaximo.HasValue && precioMaximo.Value < 0)
            {
                throw new Exception("El precio maximo no puede ser negativo");
            }

            if (precioMinimo.HasValue && precioMaximo.HasValue && precioMaximo.Value < precioMinimo.Value)
            {
                throw new Exception("El precio maximo no puede ser menor al precio minimo");
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
                throw new Exception("Debe seleccionar un producto valido");
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
                throw new Exception("Debe seleccionar un producto para consultar el historial");
            }

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
