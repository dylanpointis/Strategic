using Services;
using System.Data.SqlClient;

namespace DAL
{
    public class DALEvento
    {
        private readonly DALConexion dalCon = new DALConexion();

        public void RegistrarEvento(Evento evento)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@NombreUsuario", evento.NombreUsuario),
                new SqlParameter("@Modulo", evento.Modulo),
                new SqlParameter("@Evento", evento.Descripcion),
                new SqlParameter("@Criticidad", evento.Criticidad),
                new SqlParameter("@Fecha", evento.Fecha),
                new SqlParameter("@Hora", evento.Hora)
            };

            dalCon.EjecutarProcAlmacenado("RegistrarEvento", parametros);
        }
    }
}
