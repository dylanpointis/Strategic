using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class DALAutomatizacion
    {
        private readonly DALConexion dalCon = new DALConexion();

        public List<BEAutomatizacion> TraerListaAutomatizaciones()
        {
            DataTable tabla = dalCon.ConsultaProcAlmacenado("TraerListaAutomatizaciones", null);
            List<BEAutomatizacion> automatizaciones = new List<BEAutomatizacion>();

            foreach (DataRow row in tabla.Rows)
            {
                automatizaciones.Add(new BEAutomatizacion
                {
                    IdAutomatizacion = Convert.ToInt32(row["IdAutomatizacion"]),
                    IdCatalogoAutomatizacion = Convert.ToInt32(row["IdCatalogoAutomatizacion"]),
                    Nombre = row["Nombre"].ToString(),
                    Tipo = row["Tipo"].ToString(),
                    Descripcion = row["Descripcion"].ToString(),
                    Parametros = row["Parametros"].ToString(),
                    Estado = row["Estado"].ToString(),
                    FechaAlta = Convert.ToDateTime(row["FechaAlta"]),
                    FechaUltimaEjecucion = row["FechaUltimaEjecucion"] == DBNull.Value
                        ? (DateTime?)null
                        : Convert.ToDateTime(row["FechaUltimaEjecucion"])
                });
            }

            return automatizaciones;
        }

        public void ModificarEstado(int idAutomatizacion, string estado)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@IdAutomatizacion", idAutomatizacion),
                new SqlParameter("@Estado", estado)
            };

            dalCon.EjecutarProcAlmacenado("ModificarEstadoAutomatizacion", parametros);
        }

        public void ModificarParametros(int idAutomatizacion, string parametrosJson)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@IdAutomatizacion", idAutomatizacion),
                new SqlParameter("@Parametros", parametrosJson)
            };

            dalCon.EjecutarProcAlmacenado("ModificarParametrosAutomatizacion", parametros);
        }

        public void ActualizarFechaUltimaEjecucion(int idAutomatizacion)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@IdAutomatizacion", idAutomatizacion)
            };

            dalCon.EjecutarProcAlmacenado("ActualizarFechaEjecucionAutomatizacion", parametros);
        }

        public void RegistrarHistorial(BEHistorialAutomatizacion historial)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@IdProducto", historial.IdProducto),
                new SqlParameter("@IdAutomatizacion", historial.IdAutomatizacion),
                new SqlParameter("@AccionEjecutada", historial.AccionEjecutada),
                new SqlParameter("@ValorAnterior", historial.ValorAnterior),
                new SqlParameter("@ValorNuevo", historial.ValorNuevo),
                new SqlParameter("@Estado", historial.Estado)
            };

            dalCon.EjecutarProcAlmacenado("RegistrarHistorialAutomatizacion", parametros);
        }

        public List<BEHistorialAutomatizacion> TraerHistorial()
        {
            DataTable tabla = dalCon.ConsultaProcAlmacenado("TraerHistorialAutomatizaciones", null);
            List<BEHistorialAutomatizacion> historial = new List<BEHistorialAutomatizacion>();

            foreach (DataRow row in tabla.Rows)
            {
                historial.Add(new BEHistorialAutomatizacion
                {
                    IdHistorial = Convert.ToInt32(row["IdHistorial"]),
                    IdProducto = Convert.ToInt32(row["IdProducto"]),
                    NombreProducto = row["NombreProducto"].ToString(),
                    IdAutomatizacion = Convert.ToInt32(row["IdAutomatizacion"]),
                    NombreAutomatizacion = row["NombreAutomatizacion"].ToString(),
                    AccionEjecutada = row["AccionEjecutada"].ToString(),
                    ValorAnterior = row["ValorAnterior"].ToString(),
                    ValorNuevo = row["ValorNuevo"].ToString(),
                    Estado = row["Estado"].ToString(),
                    FechaEjecucion = Convert.ToDateTime(row["FechaEjecucion"])
                });
            }

            return historial;
        }
    }
}
