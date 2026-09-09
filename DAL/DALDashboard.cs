using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class DALDashboard
    {
        private readonly DALConexion dalCon = new DALConexion();

        public BEResumenDashboard TraerResumen(DateTime? fechaInicio, DateTime? fechaFin, string categoria)
        {
            DataTable tabla = dalCon.ConsultaProcAlmacenado("TraerResumenDashboard", ArmarParametros(fechaInicio, fechaFin, categoria));

            BEResumenDashboard resumen = new BEResumenDashboard();

            foreach (DataRow row in tabla.Rows)
            {
                resumen.FacturacionTotal = Convert.ToDecimal(row["FacturacionTotal"]);
                resumen.CantidadVentas = Convert.ToInt32(row["CantidadVentas"]);
                resumen.UnidadesVendidas = Convert.ToInt32(row["UnidadesVendidas"]);
            }

            return resumen;
        }

        public List<BEVentaPorCategoria> TraerVentasPorCategoria(DateTime? fechaInicio, DateTime? fechaFin, string categoria)
        {
            DataTable tabla = dalCon.ConsultaProcAlmacenado("TraerVentasPorCategoria", ArmarParametros(fechaInicio, fechaFin, categoria));
            List<BEVentaPorCategoria> lista = new List<BEVentaPorCategoria>();

            foreach (DataRow row in tabla.Rows)
            {
                lista.Add(new BEVentaPorCategoria
                {
                    Categoria = row["Categoria"].ToString(),
                    Unidades = Convert.ToInt32(row["Unidades"]),
                    Monto = Convert.ToDecimal(row["Monto"])
                });
            }

            return lista;
        }

        public List<BETopProducto> TraerTopProductos(DateTime? fechaInicio, DateTime? fechaFin, string categoria, int cantidad)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@FechaInicio", fechaInicio),
                new SqlParameter("@FechaFin", fechaFin),
                new SqlParameter("@Categoria", categoria),
                new SqlParameter("@Cantidad", cantidad)
            };

            DataTable tabla = dalCon.ConsultaProcAlmacenado("TraerTopProductos", parametros);
            List<BETopProducto> lista = new List<BETopProducto>();

            foreach (DataRow row in tabla.Rows)
            {
                lista.Add(new BETopProducto
                {
                    IdProducto = Convert.ToInt32(row["IdProducto"]),
                    Codigo = row["Codigo"].ToString(),
                    Nombre = row["Nombre"].ToString(),
                    Categoria = row["Categoria"].ToString(),
                    Unidades = Convert.ToInt32(row["Unidades"]),
                    Monto = Convert.ToDecimal(row["Monto"])
                });
            }

            return lista;
        }

        private SqlParameter[] ArmarParametros(DateTime? fechaInicio, DateTime? fechaFin, string categoria)
        {
            return new SqlParameter[]
            {
                new SqlParameter("@FechaInicio", fechaInicio),
                new SqlParameter("@FechaFin", fechaFin),
                new SqlParameter("@Categoria", categoria)
            };
        }
    }
}
