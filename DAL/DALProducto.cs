using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class DALProducto
    {
        private readonly DALConexion dalCon = new DALConexion();

        public List<BEProducto> TraerListaProductos()
        {
            return MapearProductos(dalCon.ConsultaProcAlmacenado("TraerListaProductos", null));
        }

        public List<BEProducto> FiltrarProductos(string nombre, string categoria, string estado, decimal? precioMinimo, decimal? precioMaximo)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@Nombre", nombre),
                new SqlParameter("@Categoria", categoria),
                new SqlParameter("@Estado", estado),
                new SqlParameter("@PrecioMinimo", precioMinimo),
                new SqlParameter("@PrecioMaximo", precioMaximo)
            };

            return MapearProductos(dalCon.ConsultaProcAlmacenado("FiltrarProductos", parametros));
        }

        /// <summary>
        /// Productos cuyo stock esta en el minimo definido o por debajo.
        /// </summary>
        public List<BEProducto> TraerProductosBajoStock(string categoria)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@Categoria", categoria)
            };

            return MapearProductos(dalCon.ConsultaProcAlmacenado("TraerProductosBajoStock", parametros));
        }

        public BEProducto TraerProductoPorId(int idProducto)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@IdProducto", idProducto)
            };

            List<BEProducto> productos = MapearProductos(dalCon.ConsultaProcAlmacenado("TraerProductoPorId", parametros));

            return productos.Count > 0 ? productos[0] : null;
        }

        public List<BEHistorialPrecio> TraerHistorialPrecios(int idProducto, DateTime? fechaInicio, DateTime? fechaFin)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@IdProducto", idProducto),
                new SqlParameter("@FechaInicio", fechaInicio),
                new SqlParameter("@FechaFin", fechaFin)
            };

            DataTable tabla = dalCon.ConsultaProcAlmacenado("TraerHistorialPrecios", parametros);
            List<BEHistorialPrecio> historial = new List<BEHistorialPrecio>();

            foreach (DataRow row in tabla.Rows)
            {
                historial.Add(new BEHistorialPrecio
                {
                    IdHistorialPrecio = Convert.ToInt32(row["IdHistorialPrecio"]),
                    IdProducto = Convert.ToInt32(row["IdProducto"]),
                    NombreProducto = row["NombreProducto"].ToString(),
                    Precio = Convert.ToDecimal(row["Precio"]),
                    Fecha = Convert.ToDateTime(row["Fecha"])
                });
            }

            return historial;
        }

        public List<BEHistorialStock> TraerHistorialStock(int idProducto, DateTime? fechaInicio, DateTime? fechaFin)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@IdProducto", idProducto),
                new SqlParameter("@FechaInicio", fechaInicio),
                new SqlParameter("@FechaFin", fechaFin)
            };

            DataTable tabla = dalCon.ConsultaProcAlmacenado("TraerHistorialStock", parametros);
            List<BEHistorialStock> historial = new List<BEHistorialStock>();

            foreach (DataRow row in tabla.Rows)
            {
                historial.Add(new BEHistorialStock
                {
                    IdHistorialStock = Convert.ToInt32(row["IdHistorialStock"]),
                    IdProducto = Convert.ToInt32(row["IdProducto"]),
                    NombreProducto = row["NombreProducto"].ToString(),
                    Stock = Convert.ToInt32(row["Stock"]),
                    Fecha = Convert.ToDateTime(row["Fecha"])
                });
            }

            return historial;
        }

        private List<BEProducto> MapearProductos(DataTable tabla)
        {
            List<BEProducto> productos = new List<BEProducto>();

            foreach (DataRow row in tabla.Rows)
            {
                productos.Add(new BEProducto
                {
                    IdProducto = Convert.ToInt32(row["IdProducto"]),
                    Codigo = row["Codigo"].ToString(),
                    Nombre = row["Nombre"].ToString(),
                    Estado = row["Estado"].ToString(),
                    Precio = Convert.ToDecimal(row["Precio"]),
                    Stock = Convert.ToInt32(row["Stock"]),
                    StockMaximo = row["StockMaximo"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["StockMaximo"]),
                    StockMinimo = row["StockMinimo"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["StockMinimo"]),
                    Categoria = row["Categoria"] == DBNull.Value ? string.Empty : row["Categoria"].ToString(),
                    BorradoLogico = Convert.ToBoolean(row["BorradoLogico"]),
                    FechaSincronizacion = row["FechaSincronizacion"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["FechaSincronizacion"]),
                    Marca = row["Marca"] == DBNull.Value ? string.Empty : row["Marca"].ToString()
                });
            }

            return productos;
        }
    }
}
