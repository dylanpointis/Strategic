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

        // Nombre de la base configurada en la cadena de conexion
        public string NombreBaseDeDatos
        {
            get
            {
                string connectionString = ConfigurationManager.ConnectionStrings["StrategicConnection"].ConnectionString;
                return new SqlConnectionStringBuilder(connectionString).InitialCatalog;
            }
        }

        // Conexion a master: hace falta para el restore, porque no se puede
        // restaurar la base a la que se esta conectado
        private SqlConnection CrearConexionConMaster()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["StrategicConnection"].ConnectionString;
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);
            builder.InitialCatalog = "master";
            return new SqlConnection(builder.ConnectionString);
        }

        // Ejecuta un comando escrito contra master. Solo lo usan el backup y el
        // restore, que no pueden ir en un stored procedure de la propia base
        public void EjecutarComandoEnMaster(string comando, SqlParameter[] parametros)
        {
            using (SqlConnection con = CrearConexionConMaster())
            using (SqlCommand command = new SqlCommand(comando, con))
            {
                command.CommandType = CommandType.Text;
                command.CommandTimeout = 600;
                AgregarParametros(command, parametros);

                con.Open();
                command.ExecuteNonQuery();
            }
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
