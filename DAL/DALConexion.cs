using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class DALConexion
    {
        private SqlConnection CrearConexion()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["StrategicConnection"].ConnectionString;
            return new SqlConnection(connectionString);
        }

        public DataTable ConsultaProcAlmacenado(string nombreProc, SqlParameter[] parametros)
        {
            DataTable tabla = new DataTable();

            using (SqlConnection con = CrearConexion())
            using (SqlCommand command = new SqlCommand(nombreProc, con))
            {
                command.CommandType = CommandType.StoredProcedure;
                AgregarParametros(command, parametros);

                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    adapter.Fill(tabla);
                }
            }

            return tabla;
        }

        public void EjecutarProcAlmacenado(string nombreProc, SqlParameter[] parametros)
        {
            using (SqlConnection con = CrearConexion())
            {
                con.Open();
                using (SqlTransaction tran = con.BeginTransaction())
                using (SqlCommand command = new SqlCommand(nombreProc, con, tran))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    AgregarParametros(command, parametros);

                    try
                    {
                        command.ExecuteNonQuery();
                        tran.Commit();
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        private void AgregarParametros(SqlCommand command, SqlParameter[] parametros)
        {
            if (parametros == null)
            {
                return;
            }

            foreach (SqlParameter parametro in parametros)
            {
                if (parametro.Value == null)
                {
                    parametro.Value = DBNull.Value;
                }

                command.Parameters.Add(parametro);
            }
        }
    }
}
