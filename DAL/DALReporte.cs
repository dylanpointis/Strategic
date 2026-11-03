using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    // CU-002-008. Los reportes de ventas y competencia usan DALDashboard y
    // DALCompetencia; aca queda solo el de stock, que tiene SP propio
    public class DALReporte
    {
        private readonly DALConexion dalCon = new DALConexion();

        public List<BEReporteStock> TraerReporteStock(DateTime? fechaInicio, DateTime? fechaFin, string categoria)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@FechaInicio", fechaInicio),
                new SqlParameter("@FechaFin", fechaFin),
                new SqlParameter("@Categoria", categoria)
            };

            DataTable tabla = dalCon.ConsultaProcAlmacenado("TraerReporteStock", parametros);
            List<BEReporteStock> lista = new List<BEReporteStock>();

            foreach (DataRow row in tabla.Rows)
            {
                lista.Add(new BEReporteStock
                {
                    IdProducto = Convert.ToInt32(row["IdProducto"]),
                    Codigo = row["Codigo"].ToString(),
                    Nombre = row["Nombre"].ToString(),
                    Categoria = row["Categoria"].ToString(),
                    Stock = Convert.ToInt32(row["Stock"]),
                    StockMinimo = row["StockMinimo"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["StockMinimo"]),
                    StockMaximo = row["StockMaximo"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["StockMaximo"]),
                    UnidadesVendidas = Convert.ToInt32(row["UnidadesVendidas"])
                });
            }

            return lista;
        }
    }
}
