using Services;
using System.Data;
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

        public DataTable TraerListaEventos()
        {
            return dalCon.ConsultaProcAlmacenado("TraerListaEventos", null);
        }

        public DataTable FiltrarEventos(string nombreUsuario, string modulo, string descripcion, string fechaInicio, string fechaFin)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@NombreUsuario", nombreUsuario),
                new SqlParameter("@Modulo", modulo),
                new SqlParameter("@Evento", descripcion),
                new SqlParameter("@FechaInicio", fechaInicio),
                new SqlParameter("@FechaFin", fechaFin)
            };

            return dalCon.ConsultaProcAlmacenado("FiltrarEventos", parametros);
        }
    }
}
