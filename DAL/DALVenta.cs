using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class DALVenta
    {
        private readonly DALConexion dalCon = new DALConexion();

        public List<BEVenta> TraerListaVentas()
        {
            return MapearVentas(dalCon.ConsultaProcAlmacenado("TraerListaVentas", null));
        }

        public List<BEVenta> FiltrarVentas(DateTime? fechaInicio, DateTime? fechaFin, int? idProducto, string categoria, string estado)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@FechaInicio", fechaInicio),
                new SqlParameter("@FechaFin", fechaFin),
                new SqlParameter("@IdProducto", idProducto),
                new SqlParameter("@Categoria", categoria),
                new SqlParameter("@Estado", estado)
            };

            return MapearVentas(dalCon.ConsultaProcAlmacenado("FiltrarVentas", parametros));
        }

        
        /// Unidades vendidas por día de un producto, sin las ventas canceladas.
        /// Es la serie sobre la que se proyecta la demanda.
        
        public List<KeyValuePair<DateTime, int>> TraerUnidadesPorDia(int idProducto)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@IdProducto", idProducto)
            };

            DataTable tabla = dalCon.ConsultaProcAlmacenado("TraerVentasDiariasProducto", parametros);
            List<KeyValuePair<DateTime, int>> unidades = new List<KeyValuePair<DateTime, int>>();

            foreach (DataRow row in tabla.Rows)
            {
                unidades.Add(new KeyValuePair<DateTime, int>(
                    Convert.ToDateTime(row["Fecha"]),
                    Convert.ToInt32(row["Unidades"])));
            }

            return unidades;
        }

        private List<BEVenta> MapearVentas(DataTable tabla)
        {
            List<BEVenta> ventas = new List<BEVenta>();

            foreach (DataRow row in tabla.Rows)
            {
                ventas.Add(new BEVenta
                {
                    IdVenta = Convert.ToInt32(row["IdVenta"]),
                    NroVenta = row["NroVenta"].ToString(),
                    Fecha = Convert.ToDateTime(row["Fecha"]),
                    MontoTotal = Convert.ToDecimal(row["MontoTotal"]),
                    Estado = row["Estado"].ToString(),
                    CantidadItems = Convert.ToInt32(row["CantidadItems"]),
                    UnidadesVendidas = Convert.ToInt32(row["UnidadesVendidas"])
                });
            }

            return ventas;
        }
    }
}
