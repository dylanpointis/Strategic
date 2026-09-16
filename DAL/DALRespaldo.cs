using System;
using System.Data.SqlClient;

namespace DAL
{
    // Backup y restore de la base. Van como comandos escritos contra master
    // porque un stored procedure de la propia base no puede restaurarla
    public class DALRespaldo
    {
        private readonly DALConexion dalConexion = new DALConexion();

        public void RealizarBackup(string ruta)
        {
            string comando = string.Format("BACKUP DATABASE [{0}] TO DISK = @Ruta WITH INIT", dalConexion.NombreBaseDeDatos);

            dalConexion.EjecutarComandoEnMaster(comando, new SqlParameter[]
            {
                new SqlParameter("@Ruta", ruta)
            });
        }

        public void RealizarRestore(string ruta)
        {
            string baseDeDatos = dalConexion.NombreBaseDeDatos;

            // Las conexiones abiertas del pool apuntan a la base que se va a cerrar
            SqlConnection.ClearAllPools();

            try
            {
                // Se cierran las conexiones activas para que el restore no quede bloqueado
                dalConexion.EjecutarComandoEnMaster(
                    string.Format("ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE", baseDeDatos), null);

                dalConexion.EjecutarComandoEnMaster(
                    string.Format("RESTORE DATABASE [{0}] FROM DISK = @Ruta WITH REPLACE", baseDeDatos),
                    new SqlParameter[] { new SqlParameter("@Ruta", ruta) });
            }
            finally
            {
                // Pase lo que pase la base vuelve a multiusuario. Si el restore
                // fallo a mitad de camino este comando tambien puede fallar, y en
                // ese caso interesa el error original, no este
                try
                {
                    dalConexion.EjecutarComandoEnMaster(
                        string.Format("ALTER DATABASE [{0}] SET MULTI_USER", baseDeDatos), null);
                }
                catch (Exception)
                {
                }

                SqlConnection.ClearAllPools();
            }
        }
    }
}
