using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace DAL
{
    public class DALIntegracion
    {
        private readonly DALConexion dalCon = new DALConexion();

        // Null si el tipo de archivo todavia no tiene configuracion
        public BEConfiguracionIntegracion TraerConfiguracionIntegracion(string tipoArchivo)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@TipoArchivo", tipoArchivo)
            };

            DataTable tabla = dalCon.ConsultaProcAlmacenado("TraerConfiguracionIntegracion", parametros);

            if (tabla.Rows.Count == 0)
            {
                return null;
            }

            // El SP devuelve una fila por campo mapeado con los datos de la
            // configuracion repetidos: se toman de la primera y se suman los mapeos
            BEConfiguracionIntegracion configuracion = new BEConfiguracionIntegracion
            {
                IdConfiguracion = Convert.ToInt32(tabla.Rows[0]["IdConfiguracion"]),
                TipoArchivo = tabla.Rows[0]["TipoArchivo"].ToString(),
                FechaConfiguracion = Convert.ToDateTime(tabla.Rows[0]["FechaConfiguracion"])
            };

            foreach (DataRow row in tabla.Rows)
            {
                if (row["IdMapeo"] == DBNull.Value)
                {
                    continue;
                }

                configuracion.Mapeos.Add(new BEMapeoColumna
                {
                    IdMapeo = Convert.ToInt32(row["IdMapeo"]),
                    ColumnaStrategic = row["ColumnaStrategic"].ToString(),
                    ColumnaCliente = row["ColumnaCliente"].ToString()
                });
            }

            return configuracion;
        }

        // Alta o reemplazo del mapeo del tipo de archivo. Devuelve el id de la configuracion
        public int GuardarConfiguracionIntegracion(BEConfiguracionIntegracion configuracion)
        {
            // El mapeo viaja como "Codigo=SKU,Nombre=Descripcion,..." y el SP lo
            // separa con STRING_SPLIT (los nombres XML no pueden tener ',' ni '=')
            List<string> mapeos = new List<string>();

            foreach (BEMapeoColumna mapeo in configuracion.Mapeos)
            {
                mapeos.Add(mapeo.ColumnaStrategic + "=" + mapeo.ColumnaCliente);
            }

            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@TipoArchivo", configuracion.TipoArchivo),
                new SqlParameter("@Mapeos", string.Join(",", mapeos))
            };

            DataTable tabla = dalCon.ConsultaProcAlmacenado("GuardarConfiguracionIntegracion", parametros);

            foreach (DataRow row in tabla.Rows)
            {
                return Convert.ToInt32(row["IdConfiguracion"]);
            }

            return 0;
        }

        public List<string> TraerCodigosProductos()
        {
            DataTable tabla = dalCon.ConsultaProcAlmacenado("TraerCodigosProductos", null);
            List<string> codigos = new List<string>();

            foreach (DataRow row in tabla.Rows)
            {
                codigos.Add(row["Codigo"].ToString());
            }

            return codigos;
        }

        // Importa los registros validos uno por uno, todos dentro de la misma
        // transaccion: si alguno falla se deshace todo lo hecho (rollback) y no
        // queda la importacion a medias. Devuelve cuantos se importaron (altas)
        // y cuantos se actualizaron (ya existian)
        public void ImportarDatos(string tipoArchivo, List<Dictionary<string, string>> registros, out int importados, out int actualizados)
        {
            importados = 0;
            actualizados = 0;

            // Ventas que se crearon en esta importacion: las lineas siguientes de
            // la misma venta se le agregan como items en vez de tomarla como existente
            HashSet<string> ventasNuevas = new HashSet<string>();

            using (SqlConnection con = dalCon.AbrirConexion())
            using (SqlTransaction transaccion = con.BeginTransaction())
            {
                try
                {
                    foreach (Dictionary<string, string> registro in registros)
                    {
                        string resultado;

                        switch (tipoArchivo)
                        {
                            case "Productos":
                                resultado = ImportarProducto(registro, transaccion);
                                break;
                            case "Ventas":
                                resultado = ImportarLineaVenta(registro, ventasNuevas, transaccion);
                                break;
                            case "Stock":
                                resultado = ImportarStockProducto(registro, transaccion);
                                break;
                            default:
                                throw new ArgumentException("Tipo de archivo no soportado: " + tipoArchivo);
                        }

                        // Cada SP devuelve si el registro fue un alta o una actualizacion
                        if (resultado == "Importado")
                        {
                            importados++;
                        }
                        else
                        {
                            actualizados++;
                        }
                    }

                    transaccion.Commit();
                }
                catch
                {
                    // Si el error vino de SQL Server, el SP ya deshizo la transaccion
                    // (XACT_ABORT) y Connection queda en null: solo se revierte si sigue abierta
                    if (transaccion.Connection != null)
                    {
                        transaccion.Rollback();
                    }

                    throw;
                }
            }
        }

        // Alta o actualizacion de un producto por su codigo
        private string ImportarProducto(Dictionary<string, string> registro, SqlTransaction transaccion)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@Codigo", Texto(registro, "Codigo")),
                new SqlParameter("@Nombre", Texto(registro, "Nombre")),
                new SqlParameter("@Estado", Texto(registro, "Estado")),
                new SqlParameter("@Precio", Importe(registro, "Precio")),
                new SqlParameter("@Stock", Entero(registro, "Stock")),
                new SqlParameter("@StockMinimo", Entero(registro, "StockMinimo")),
                new SqlParameter("@StockMaximo", Entero(registro, "StockMaximo")),
                new SqlParameter("@Categoria", Texto(registro, "Categoria")),
                new SqlParameter("@Marca", Texto(registro, "Marca"))
            };

            //Ejecuta el SP que da de alta o actualiza el producto, y registra los cambios de precio y stock en el historial.
            return LeerResultado(dalCon.ConsultaProcAlmacenado("ImportarProducto", parametros, transaccion));
        }

        // Una linea de venta. La primera linea de una venta nueva la da de alta;
        // las siguientes de esa misma venta se agregan como items
        private string ImportarLineaVenta(Dictionary<string, string> registro, HashSet<string> ventasNuevas, SqlTransaction transaccion)
        {
            string nroVenta = Texto(registro, "NroVenta");

            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@NroVenta", nroVenta),
                new SqlParameter("@Fecha", Fecha(registro, "Fecha")),
                new SqlParameter("@Estado", Texto(registro, "Estado") ?? "Completada"),
                new SqlParameter("@CodigoProducto", Texto(registro, "CodigoProducto")),
                new SqlParameter("@Cantidad", Entero(registro, "Cantidad")),
                new SqlParameter("@PrecioVenta", Importe(registro, "PrecioVenta")),
                new SqlParameter("@AgregarItem", ventasNuevas.Contains(nroVenta))
            };

            string resultado = LeerResultado(dalCon.ConsultaProcAlmacenado("ImportarLineaVenta", parametros, transaccion));

            if (resultado == "Importado")
            {
                ventasNuevas.Add(nroVenta);
            }

            return resultado;
        }

        // Actualiza el stock de un producto existente
        private string ImportarStockProducto(Dictionary<string, string> registro, SqlTransaction transaccion)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@CodigoProducto", Texto(registro, "CodigoProducto")),
                new SqlParameter("@Stock", Entero(registro, "Stock"))
            };

            return LeerResultado(dalCon.ConsultaProcAlmacenado("ImportarStockProducto", parametros, transaccion));
        }

        private string LeerResultado(DataTable tabla)
        {
            return tabla.Rows[0]["Resultado"].ToString();
        }

        public int RegistrarSincronizacion(BEHistorialSincronizacion sincronizacion)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@IdConfiguracion", sincronizacion.IdConfiguracion),
                new SqlParameter("@TipoArchivo", sincronizacion.TipoArchivo),
                new SqlParameter("@RegistrosImportados", sincronizacion.RegistrosImportados),
                new SqlParameter("@RegistrosActualizados", sincronizacion.RegistrosActualizados),
                new SqlParameter("@RegistrosError", sincronizacion.RegistrosError),
                new SqlParameter("@Estado", sincronizacion.Estado),
                new SqlParameter("@Detalle", sincronizacion.Detalle)
            };

            DataTable tabla = dalCon.ConsultaProcAlmacenado("RegistrarSincronizacion", parametros);

            foreach (DataRow row in tabla.Rows)
            {
                return Convert.ToInt32(row["IdSincronizacion"]);
            }

            return 0;
        }

        public List<BEHistorialSincronizacion> TraerHistorialSincronizaciones()
        {
            DataTable tabla = dalCon.ConsultaProcAlmacenado("TraerHistorialSincronizaciones", null);
            List<BEHistorialSincronizacion> historial = new List<BEHistorialSincronizacion>();

            foreach (DataRow row in tabla.Rows)
            {
                historial.Add(new BEHistorialSincronizacion
                {
                    IdSincronizacion = Convert.ToInt32(row["IdSincronizacion"]),
                    IdConfiguracion = Convert.ToInt32(row["IdConfiguracion"]),
                    TipoArchivo = row["TipoArchivo"].ToString(),
                    RegistrosImportados = Convert.ToInt32(row["RegistrosImportados"]),
                    RegistrosActualizados = Convert.ToInt32(row["RegistrosActualizados"]),
                    RegistrosError = Convert.ToInt32(row["RegistrosError"]),
                    Estado = row["Estado"].ToString(),
                    Detalle = row["Detalle"] == DBNull.Value ? null : row["Detalle"].ToString(),
                    FechaEjecucion = Convert.ToDateTime(row["FechaEjecucion"])
                });
            }

            return historial;
        }

        // La BLL deja los valores validados y en formato invariante (punto decimal,
        // fecha ISO). Aca se convierten al tipo de cada parametro. Un campo que no
        // vino devuelve null, que DALConexion manda como NULL a la base
        private string Texto(Dictionary<string, string> registro, string campo)
        {
            string valor;
            return registro.TryGetValue(campo, out valor) ? valor : null;
        }

        private object Importe(Dictionary<string, string> registro, string campo)
        {
            string valor = Texto(registro, campo);
            return valor == null ? null : (object)decimal.Parse(valor, CultureInfo.InvariantCulture);
        }

        private object Entero(Dictionary<string, string> registro, string campo)
        {
            string valor = Texto(registro, campo);
            return valor == null ? null : (object)int.Parse(valor, CultureInfo.InvariantCulture);
        }

        private object Fecha(Dictionary<string, string> registro, string campo)
        {
            string valor = Texto(registro, campo);
            return valor == null ? null : (object)DateTime.Parse(valor, CultureInfo.InvariantCulture);
        }
    }
}
