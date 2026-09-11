using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class DALCompetencia
    {
        private readonly DALConexion dalCon = new DALConexion();

        public List<BECompetencia> TraerListaCompetidores()
        {
            return MapearCompetidores(dalCon.ConsultaProcAlmacenado("TraerListaCompetidores", null));
        }

        public List<BECompetencia> FiltrarCompetidores(string texto, string marketplace, bool? activo)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@Texto", texto),
                new SqlParameter("@Marketplace", marketplace),
                new SqlParameter("@Activo", activo)
            };

            return MapearCompetidores(dalCon.ConsultaProcAlmacenado("FiltrarCompetidores", parametros));
        }

        public BECompetencia TraerCompetidorPorId(int idCompetencia)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@IdCompetencia", idCompetencia)
            };

            List<BECompetencia> competidores = MapearCompetidores(
                dalCon.ConsultaProcAlmacenado("TraerCompetidorPorId", parametros));

            return competidores.Count > 0 ? competidores[0] : null;
        }

        /// <summary>
        /// Cantidad de competidores que ya usan ese nombre.
        /// idCompetenciaExcluido deja fuera al competidor que se esta editando.
        /// </summary>
        public int ContarCompetidoresConNombre(string nombre, int? idCompetenciaExcluido)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@Nombre", nombre),
                new SqlParameter("@IdCompetenciaExcluido", idCompetenciaExcluido)
            };

            DataTable tabla = dalCon.ConsultaProcAlmacenado("ExisteCompetidorConNombre", parametros);

            foreach (DataRow row in tabla.Rows)
            {
                return Convert.ToInt32(row["Cantidad"]);
            }

            return 0;
        }

        public int AltaCompetidor(BECompetencia competidor)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@Nombre", competidor.Nombre),
                new SqlParameter("@Marketplace", competidor.Marketplace),
                new SqlParameter("@Descripcion", competidor.Descripcion)
            };

            DataTable tabla = dalCon.ConsultaProcAlmacenado("AltaCompetidor", parametros);

            foreach (DataRow row in tabla.Rows)
            {
                return Convert.ToInt32(row["IdCompetencia"]);
            }

            return 0;
        }

        public void ModificarCompetidor(BECompetencia competidor)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@IdCompetencia", competidor.IdCompetencia),
                new SqlParameter("@Nombre", competidor.Nombre),
                new SqlParameter("@Marketplace", competidor.Marketplace),
                new SqlParameter("@Descripcion", competidor.Descripcion)
            };

            dalCon.EjecutarProcAlmacenado("ModificarCompetidor", parametros);
        }

        public void ModificarEstado(int idCompetencia, bool activo)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@IdCompetencia", idCompetencia),
                new SqlParameter("@Estado", activo ? "Activo" : "Inactivo")
            };

            dalCon.EjecutarProcAlmacenado("ModificarEstadoCompetidor", parametros);
        }

        /// <summary>
        /// Cantidad de publicaciones mapeadas activas de un competidor. Se usa
        /// para advertir antes de la baja, ya que tambien se pausan (CU-007-034).
        /// </summary>
        public int ContarPublicacionesActivasPorCompetidor(int idCompetencia)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@IdCompetencia", idCompetencia)
            };

            DataTable tabla = dalCon.ConsultaProcAlmacenado("ContarPublicacionesActivasPorCompetidor", parametros);

            foreach (DataRow row in tabla.Rows)
            {
                return Convert.ToInt32(row["Cantidad"]);
            }

            return 0;
        }

        /// <summary>
        /// Cantidad de mapeos existentes para esa combinacion de producto propio
        /// y competidor. Se usa para evitar mapeos duplicados (CU-007-036).
        /// </summary>
        public int ContarMapeosProductoCompetidor(int idProducto, int idCompetencia)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@IdProducto", idProducto),
                new SqlParameter("@IdCompetencia", idCompetencia)
            };

            DataTable tabla = dalCon.ConsultaProcAlmacenado("ExisteMapeoProductoCompetencia", parametros);

            foreach (DataRow row in tabla.Rows)
            {
                return Convert.ToInt32(row["Cantidad"]);
            }

            return 0;
        }

        public int MapearProducto(int idProducto, int idCompetencia, string url)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@IdProducto", idProducto),
                new SqlParameter("@IdCompetencia", idCompetencia),
                new SqlParameter("@Url", url)
            };

            DataTable tabla = dalCon.ConsultaProcAlmacenado("AltaMapeoProductoCompetencia", parametros);

            foreach (DataRow row in tabla.Rows)
            {
                return Convert.ToInt32(row["IdProductoCompetencia"]);
            }

            return 0;
        }

        public List<BEComparacionPrecio> TraerComparacionPrecios()
        {
            return MapearComparaciones(dalCon.ConsultaProcAlmacenado("TraerComparacionPrecios", null));
        }

        public List<BEComparacionPrecio> FiltrarComparacionPrecios(int? idProducto, string categoria, int? idCompetencia)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@IdProducto", idProducto),
                new SqlParameter("@Categoria", categoria),
                new SqlParameter("@IdCompetencia", idCompetencia)
            };

            return MapearComparaciones(dalCon.ConsultaProcAlmacenado("FiltrarComparacionPrecios", parametros));
        }

        public BEComparacionPrecio TraerComparacionPrecioPorId(int idProductoCompetencia)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@IdProductoCompetencia", idProductoCompetencia)
            };

            List<BEComparacionPrecio> comparaciones = MapearComparaciones(
                dalCon.ConsultaProcAlmacenado("TraerComparacionPrecioPorId", parametros));

            return comparaciones.Count > 0 ? comparaciones[0] : null;
        }

        public List<BEHistorialPrecioCompetencia> TraerHistorialPrecioCompetencia(int idProductoCompetencia)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@IdProductoCompetencia", idProductoCompetencia)
            };

            DataTable tabla = dalCon.ConsultaProcAlmacenado("TraerHistorialPrecioCompetencia", parametros);
            List<BEHistorialPrecioCompetencia> historial = new List<BEHistorialPrecioCompetencia>();

            foreach (DataRow row in tabla.Rows)
            {
                historial.Add(new BEHistorialPrecioCompetencia
                {
                    IdPrecioCompetencia = Convert.ToInt32(row["IdPrecioCompetencia"]),
                    IdProductoCompetencia = Convert.ToInt32(row["IdProductoCompetencia"]),
                    PrecioCompetencia = Convert.ToDecimal(row["PrecioCompetencia"]),
                    PrecioPropio = Convert.ToDecimal(row["PrecioPropio"]),
                    DiferenciaPorcentaje = row["DiferenciaPorcentaje"] == DBNull.Value
                        ? (decimal?)null
                        : Convert.ToDecimal(row["DiferenciaPorcentaje"]),
                    FechaConsulta = Convert.ToDateTime(row["FechaConsulta"])
                });
            }

            return historial;
        }

        private List<BECompetencia> MapearCompetidores(DataTable tabla)
        {
            List<BECompetencia> competidores = new List<BECompetencia>();

            foreach (DataRow row in tabla.Rows)
            {
                competidores.Add(new BECompetencia
                {
                    IdCompetencia = Convert.ToInt32(row["IdCompetencia"]),
                    Nombre = row["Nombre"].ToString(),
                    Marketplace = row["Marketplace"].ToString(),
                    Descripcion = row["Descripcion"].ToString(),
                    Estado = row["Estado"].ToString()
                });
            }

            return competidores;
        }

        private List<BEComparacionPrecio> MapearComparaciones(DataTable tabla)
        {
            List<BEComparacionPrecio> comparaciones = new List<BEComparacionPrecio>();

            foreach (DataRow row in tabla.Rows)
            {
                comparaciones.Add(new BEComparacionPrecio
                {
                    IdProductoCompetencia = Convert.ToInt32(row["IdProductoCompetencia"]),
                    IdProducto = Convert.ToInt32(row["IdProducto"]),
                    CodigoProducto = row["CodigoProducto"].ToString(),
                    NombreProducto = row["NombreProducto"].ToString(),
                    Categoria = row["Categoria"].ToString(),
                    IdCompetencia = Convert.ToInt32(row["IdCompetencia"]),
                    NombreCompetidor = row["NombreCompetidor"].ToString(),
                    Marketplace = row["Marketplace"].ToString(),
                    Url = row["URL"].ToString(),
                    PrecioPropio = Convert.ToDecimal(row["PrecioPropio"]),
                    PrecioCompetencia = row["PrecioCompetencia"] == DBNull.Value
                        ? (decimal?)null
                        : Convert.ToDecimal(row["PrecioCompetencia"]),
                    DiferenciaPorcentaje = row["DiferenciaPorcentaje"] == DBNull.Value
                        ? (decimal?)null
                        : Convert.ToDecimal(row["DiferenciaPorcentaje"]),
                    FechaConsulta = row["FechaConsulta"] == DBNull.Value
                        ? (DateTime?)null
                        : Convert.ToDateTime(row["FechaConsulta"])
                });
            }

            return comparaciones;
        }
    }
}
