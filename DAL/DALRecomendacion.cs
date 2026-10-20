using BE;
using System;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class DALRecomendacion
    {
        private readonly DALConexion dalCon = new DALConexion();

        public int RegistrarRecomendacion(BERecomendacion recomendacion)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@IdProducto", recomendacion.IdProducto),
                new SqlParameter("@Regla", recomendacion.Regla),
                new SqlParameter("@Tipo", recomendacion.Tipo),
                new SqlParameter("@Titulo", recomendacion.Titulo),
                new SqlParameter("@Descripcion", recomendacion.Descripcion),
                new SqlParameter("@Justificacion", recomendacion.Justificacion),
                new SqlParameter("@AccionSugerida", recomendacion.AccionSugerida),
                new SqlParameter("@Prioridad", recomendacion.Prioridad),
                new SqlParameter("@FechaGeneracion", recomendacion.FechaGeneracion),
                new SqlParameter("@NombreUsuario", recomendacion.NombreUsuario)
            };

            DataTable tabla = dalCon.ConsultaProcAlmacenado("RegistrarRecomendacion", parametros);

            foreach (DataRow row in tabla.Rows)
            {
                return Convert.ToInt32(row["IdRecomendacion"]);
            }

            return 0;
        }

        // Recomendaciones de la misma regla para el producto ya guardadas ese dia
        public int ContarRecomendacionesGuardadas(int idProducto, string regla, DateTime fecha)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@IdProducto", idProducto),
                new SqlParameter("@Regla", regla),
                new SqlParameter("@Fecha", fecha.Date)
            };

            DataTable tabla = dalCon.ConsultaProcAlmacenado("ExisteRecomendacionGuardada", parametros);

            foreach (DataRow row in tabla.Rows)
            {
                return Convert.ToInt32(row["Cantidad"]);
            }

            return 0;
        }
    }
}
