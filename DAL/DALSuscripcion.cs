using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class DALSuscripcion
    {
        private readonly DALConexion dalCon = new DALConexion();

        public List<BESuscripcion> TraerListaSuscripciones()
        {
            return MapearSuscripciones(dalCon.ConsultaProcAlmacenado("TraerListaSuscripciones", null));
        }

        public List<BESuscripcion> FiltrarSuscripcionesPorFechaPago(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@FechaDesde", fechaDesde),
                new SqlParameter("@FechaHasta", fechaHasta)
            };

            return MapearSuscripciones(dalCon.ConsultaProcAlmacenado("FiltrarSuscripcionesPorFechaPago", parametros));
        }

        // Pasa a Vencida los periodos activos que ya terminaron. Devuelve cuantos cambio
        public int ActualizarSuscripcionesVencidas()
        {
            DataTable tabla = dalCon.ConsultaProcAlmacenado("ActualizarSuscripcionesVencidas", null);

            foreach (DataRow row in tabla.Rows)
            {
                return Convert.ToInt32(row["Cantidad"]);
            }

            return 0;
        }

        // El numero de operacion tiene que llegar ya encriptado
        public int RegistrarSuscripcion(BESuscripcion suscripcion)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@FechaPago", suscripcion.FechaPago),
                new SqlParameter("@FechaInicio", suscripcion.FechaInicio),
                new SqlParameter("@FechaVencimiento", suscripcion.FechaVencimiento),
                new SqlParameter("@Plan", suscripcion.Plan),
                new SqlParameter("@Monto", suscripcion.Monto),
                new SqlParameter("@NumeroOperacion", suscripcion.NumeroOperacion)
            };

            DataTable tabla = dalCon.ConsultaProcAlmacenado("RegistrarSuscripcion", parametros);

            foreach (DataRow row in tabla.Rows)
            {
                return Convert.ToInt32(row["IdSuscripcion"]);
            }

            return 0;
        }

        public void CancelarSuscripcion()
        {
            dalCon.EjecutarProcAlmacenado("CancelarSuscripcion", null);
        }

        private List<BESuscripcion> MapearSuscripciones(DataTable tabla)
        {
            List<BESuscripcion> suscripciones = new List<BESuscripcion>();

            foreach (DataRow row in tabla.Rows)
            {
                suscripciones.Add(new BESuscripcion
                {
                    IdSuscripcion = Convert.ToInt32(row["IdSuscripcion"]),
                    FechaPago = Convert.ToDateTime(row["FechaPago"]),
                    FechaInicio = Convert.ToDateTime(row["FechaInicio"]),
                    FechaVencimiento = Convert.ToDateTime(row["FechaVencimiento"]),
                    Estado = row["Estado"].ToString(),
                    Plan = row["Plan"].ToString(),
                    Monto = Convert.ToDecimal(row["Monto"]),
                    NumeroOperacion = row["NumeroOperacion"] == DBNull.Value
                        ? null
                        : row["NumeroOperacion"].ToString()
                });
            }

            return suscripciones;
        }
    }
}
