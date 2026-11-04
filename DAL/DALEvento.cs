using Services;
using System;
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
                new SqlParameter("@NombreUsuario", (object)evento.NombreUsuario ?? DBNull.Value),
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

        // Da de baja logica los eventos anteriores a la fecha y devuelve cuantos fueron
        public DataTable BajaEventos(string fechaLimite)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@FechaLimite", fechaLimite)
            };

            return dalCon.ConsultaProcAlmacenado("BajaEventos", parametros);
        }
    }
}
