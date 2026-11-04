using BE;
using DAL;
using System;
using System.Collections.Generic;

namespace BLL
{
    // Si el precio supera en mas de "umbral"% a la publicacion mas barata de la
    // competencia, lo baja un "ajuste"%
    public class AjustarPrecioStrategy : IAutomatizacionStrategy
    {
        private readonly BLLCompetencia bllCompetencia = new BLLCompetencia();
        private readonly DALProducto dalProducto = new DALProducto();

        public BEHistorialAutomatizacion Ejecutar(BEAutomatizacion automatizacion, BEProducto producto)
        {
            if (producto.Estado != "Activo")
            {
                return null;
            }

            decimal? precioCompetencia = TraerPrecioMasBajo(producto.IdProducto);

            if (!precioCompetencia.HasValue)
            {
                return null;
            }

            int umbral = BLLAutomatizacion.LeerParametro(automatizacion, "umbral");
            int ajuste = BLLAutomatizacion.LeerParametro(automatizacion, "ajuste");

            if (producto.Precio <= precioCompetencia.Value * (1 + umbral / 100m))
            {
                return null;
            }

            decimal precioNuevo = Math.Round(producto.Precio * (1 - ajuste / 100m), 2);

            dalProducto.AjustarPrecio(producto.IdProducto, precioNuevo);

            return new BEHistorialAutomatizacion
            {
                AccionEjecutada = string.Format("Precio ajustado un {0}% por estar más de un {1}% sobre la competencia", ajuste, umbral),
                ValorAnterior = producto.Precio.ToString("0.00"),
                ValorNuevo = precioNuevo.ToString("0.00"),
                Estado = BEHistorialAutomatizacion.EstadoExitosa
            };
        }

        // Solo cuentan las publicaciones monitoreadas que ya tienen un precio relevado
        private decimal? TraerPrecioMasBajo(int idProducto)
        {
            decimal? precioMasBajo = null;

            List<BEProductoCompetencia> publicaciones = bllCompetencia.TraerProductoCompetencia(idProducto);

            foreach (BEProductoCompetencia publicacion in publicaciones)
            {
                if (publicacion.Estado != "Activa" || !publicacion.UltimoPrecio.HasValue || publicacion.UltimoPrecio.Value <= 0)
                {
                    continue;
                }

                if (!precioMasBajo.HasValue || publicacion.UltimoPrecio.Value < precioMasBajo.Value)
                {
                    precioMasBajo = publicacion.UltimoPrecio.Value;
                }
            }

            return precioMasBajo;
        }
    }
}
