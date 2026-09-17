using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class DALDigitoVerificador
    {
        private readonly DALConexion dalConexion = new DALConexion();

        // Tabla DigitoVerificador completa: una fila por tabla controlada
        public List<BEDigitoVerificador> TraerTablaDV()
        {
            DataTable tabla = dalConexion.ConsultaProcAlmacenado("TraerTablaDigitoVerificador", null);
            List<BEDigitoVerificador> lista = new List<BEDigitoVerificador>();

            foreach (DataRow row in tabla.Rows)
            {
                lista.Add(new BEDigitoVerificador(
                    row["Tabla"].ToString(),
                    row["DVH"] == DBNull.Value ? null : row["DVH"].ToString(),
                    row["DVV"] == DBNull.Value ? null : row["DVV"].ToString()));
            }

            return lista;
        }

        // Datos actuales de una tabla controlada, ordenados por su clave primaria
        public DataTable TraerTabla(string tabla)
        {
            DataTable datos = dalConexion.ConsultaProcAlmacenado("TraerTablaParaDV", new SqlParameter[]
            {
                new SqlParameter("@Tabla", tabla)
            });

            datos.TableName = tabla;
            return datos;
        }

        // Nombres de las columnas que forman la clave primaria de la tabla
        public List<string> TraerColumnasClave(string tabla)
        {
            DataTable datos = dalConexion.ConsultaProcAlmacenado("TraerColumnasClaveDV", new SqlParameter[]
            {
                new SqlParameter("@Tabla", tabla)
            });

            List<string> columnas = new List<string>();

            foreach (DataRow row in datos.Rows)
            {
                columnas.Add(row["Columna"].ToString());
            }

            return columnas;
        }

        public List<BEDigitoVerificadorFila> TraerDVHFilas(string tabla)
        {
            DataTable datos = dalConexion.ConsultaProcAlmacenado("TraerDVHFilas", new SqlParameter[]
            {
                new SqlParameter("@Tabla", tabla)
            });

            List<BEDigitoVerificadorFila> lista = new List<BEDigitoVerificadorFila>();

            foreach (DataRow row in datos.Rows)
            {
                lista.Add(new BEDigitoVerificadorFila(tabla, row["Clave"].ToString(), row["DVH"].ToString()));
            }

            return lista;
        }

        public List<BEDigitoVerificadorColumna> TraerDVVColumnas(string tabla)
        {
            DataTable datos = dalConexion.ConsultaProcAlmacenado("TraerDVVColumnas", new SqlParameter[]
            {
                new SqlParameter("@Tabla", tabla)
            });

            List<BEDigitoVerificadorColumna> lista = new List<BEDigitoVerificadorColumna>();

            foreach (DataRow row in datos.Rows)
            {
                lista.Add(new BEDigitoVerificadorColumna(tabla, row["Columna"].ToString(), row["DVV"].ToString()));
            }

            return lista;
        }

        // Guarda el DV global de la tabla (que ademas borra el detalle viejo) y
        // vuelve a cargar el DVH de cada fila y el DVV de cada columna
        public void PersistirDV(BEDigitoVerificador dv)
        {
            dalConexion.EjecutarProcAlmacenado("PersistirDV", new SqlParameter[]
            {
                new SqlParameter("@Tabla", dv.Tabla),
                new SqlParameter("@DVH", dv.DVH),
                new SqlParameter("@DVV", dv.DVV)
            });

            foreach (BEDigitoVerificadorFila fila in dv.Filas)
            {
                dalConexion.EjecutarProcAlmacenado("PersistirDVHFila", new SqlParameter[]
                {
                    new SqlParameter("@Tabla", dv.Tabla),
                    new SqlParameter("@Clave", fila.Clave),
                    new SqlParameter("@DVH", fila.DVH)
                });
            }

            foreach (BEDigitoVerificadorColumna columna in dv.Columnas)
            {
                dalConexion.EjecutarProcAlmacenado("PersistirDVVColumna", new SqlParameter[]
                {
                    new SqlParameter("@Tabla", dv.Tabla),
                    new SqlParameter("@Columna", columna.Columna),
                    new SqlParameter("@DVV", columna.DVV)
                });
            }
        }
    }
}
