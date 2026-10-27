using BE;
using DAL;
using Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;

namespace BLL
{
    // Modulo 006: configuracion del mapeo, importacion de archivos XML del
    // cliente e historial de sincronizaciones
    public class BLLIntegracion
    {
        private const string ModuloIntegraciones = "Integraciones";
        private const int CriticidadCambio = 2;

        // Largo de la columna Detalle de HistorialSincronizacion
        private const int LargoMaximoDetalle = 2000;

        public const string TipoProductos = "Productos";
        public const string TipoVentas = "Ventas";
        public const string TipoStock = "Stock";

        public static readonly string[] TiposArchivo = { TipoProductos, TipoVentas, TipoStock };

        private static readonly string[] EstadosProducto = { "Activo", "Pausado", "Inactivo" };
        private static readonly string[] EstadosVenta = { "Completada", "Pendiente", "Cancelada" };

        // Formatos de fecha aceptados en los archivos del cliente
        private static readonly string[] FormatosFecha =
        {
            "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm", "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm", "yyyy-MM-dd",
            "dd/MM/yyyy HH:mm:ss", "dd/MM/yyyy HH:mm", "dd/MM/yyyy"
        };

        // Campos de Strategic que se pueden mapear por tipo de archivo. El nombre
        // es tambien el atributo que reciben los SP de importacion
        private static readonly Dictionary<string, List<BECampoIntegracion>> Campos = new Dictionary<string, List<BECampoIntegracion>>
        {
            {
                TipoProductos, new List<BECampoIntegracion>
                {
                    new BECampoIntegracion("Codigo", true),
                    new BECampoIntegracion("Nombre", true),
                    new BECampoIntegracion("Precio", true),
                    new BECampoIntegracion("Stock", true),
                    new BECampoIntegracion("Estado", false),
                    new BECampoIntegracion("Categoria", false),
                    new BECampoIntegracion("Marca", false),
                    new BECampoIntegracion("StockMinimo", false),
                    new BECampoIntegracion("StockMaximo", false)
                }
            },
            {
                TipoVentas, new List<BECampoIntegracion>
                {
                    new BECampoIntegracion("NroVenta", true),
                    new BECampoIntegracion("Fecha", true),
                    new BECampoIntegracion("CodigoProducto", true),
                    new BECampoIntegracion("Cantidad", true),
                    new BECampoIntegracion("PrecioVenta", true),
                    new BECampoIntegracion("Estado", false)
                }
            },
            {
                TipoStock, new List<BECampoIntegracion>
                {
                    new BECampoIntegracion("CodigoProducto", true),
                    new BECampoIntegracion("Stock", true)
                }
            }
        };

        private readonly DALIntegracion dalIntegracion = new DALIntegracion();
        private readonly BLLEvento bllEvento = new BLLEvento();
        private readonly BLLDigitoVerificador bllDigitoVerificador = new BLLDigitoVerificador();

        public List<BECampoIntegracion> TraerCampos(string tipoArchivo)
        {
            ValidarTipo(tipoArchivo);

            return Campos[tipoArchivo];
        }

        // Null si el tipo de archivo todavia no tiene configuracion
        public BEConfiguracionIntegracion ConsultarConfiguracionIntegracion(string tipoArchivo)
        {
            ValidarTipo(tipoArchivo);

            return dalIntegracion.TraerConfiguracionIntegracion(tipoArchivo);
        }

        #region Configurar Integracion - CU-006-029

        // Lee el archivo de muestra con XmlDocument y devuelve los nombres de los
        // campos de sus registros: los hijos de cada nodo bajo la raiz
        public List<string> LeerColumnasArchivo(Stream archivo, string nombreArchivo)
        {
            ValidarExtension(nombreArchivo);

            XmlDocument documento = new XmlDocument();
            documento.XmlResolver = null;

            try
            {
                using (XmlReader lector = XmlReader.Create(archivo, ConfiguracionLectura()))
                {
                    documento.Load(lector);
                }
            }
            catch (XmlException)
            {
                // Alternativa 4.1 [#ERR025]
                throw new Exception(Traductor.T("error.archivoInvalido"));
            }

            // Se recorren todos los registros y no solo el primero: un campo opcional
            // puede faltar en algunos. Se guarda cada nombre una vez, en orden de aparicion
            List<string> columnas = new List<string>();

            foreach (XmlNode registro in documento.DocumentElement.ChildNodes)
            {
                if (registro.NodeType != XmlNodeType.Element)
                {
                    continue;
                }

                foreach (XmlNode campo in registro.ChildNodes)
                {
                    if (campo.NodeType == XmlNodeType.Element && !columnas.Contains(campo.LocalName))
                    {
                        columnas.Add(campo.LocalName);
                    }
                }
            }

            if (columnas.Count == 0)
            {
                throw new Exception(Traductor.T("error.archivoInvalido"));
            }

            return columnas;
        }

        // Valida el mapeo armado en pantalla y lo guarda. Si el tipo ya tenia
        // configuracion, el SP reemplaza su mapeo (alternativa 6.2 [#ERR026])
        public void RegistrarConfiguracionIntegracion(BEConfiguracionIntegracion configuracion, string nombreUsuarioEnSesion)
        {
            ValidarTipo(configuracion.TipoArchivo);

            List<BECampoIntegracion> campos = Campos[configuracion.TipoArchivo];

            // Solo quedan los campos con una columna asignada (los "(Sin asignar)" se
            // descartan) y que sean campos validos para el tipo
            configuracion.Mapeos = configuracion.Mapeos
                .Where(m => !string.IsNullOrWhiteSpace(m.ColumnaCliente) && campos.Any(c => c.Nombre == m.ColumnaStrategic))
                .ToList();

            // Alternativa 6.1 [#ERR001]: todos los obligatorios mapeados
            List<string> faltantes = campos
                .Where(c => c.Obligatorio && configuracion.ColumnaDe(c.Nombre) == null)
                .Select(c => Traductor.T(c.CampoTexto))
                .ToList();

            if (faltantes.Count > 0)
            {
                throw new Exception(Traductor.T("error.camposObligatoriosMapeo", string.Join(", ", faltantes)));
            }

            // Una misma columna del cliente no puede alimentar dos campos
            string repetida = configuracion.Mapeos
                .GroupBy(m => m.ColumnaCliente)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .FirstOrDefault();

            if (repetida != null)
            {
                throw new Exception(Traductor.T("error.columnaRepetidaMapeo", repetida));
            }

            dalIntegracion.GuardarConfiguracionIntegracion(configuracion);

            bllDigitoVerificador.PersistirDV("ConfiguracionIntegracion");
            bllDigitoVerificador.PersistirDV("MapeoColumna");

            bllEvento.RegistrarEvento(new Evento(
                nombreUsuarioEnSesion,
                ModuloIntegraciones,
                "Configuracion de integracion: " + configuracion.TipoArchivo,
                CriticidadCambio));
        }

        #endregion

        #region Importar y sincronizar Datos - CU-006-030

        // Lee el archivo, aplica el mapeo, valida cada registro e importa los validos.
        // Siempre devuelve un resultado (tambien si falla) y deja la ejecucion en el historial
        public BEResultadoIntegracion ImportarDatos(BEConfiguracionIntegracion configuracion, Stream archivo, string nombreArchivo, string nombreUsuarioEnSesion)
        {
            BEResultadoIntegracion resultado = new BEResultadoIntegracion { TipoArchivo = configuracion.TipoArchivo };

            // Cada registro es un diccionario columna del cliente -> valor
            List<Dictionary<string, string>> registros;
            HashSet<string> columnasArchivo;

            // Paso 1: leer el archivo. Alternativa 6.1 [#ERR025]: no es XML o esta vacio
            try
            {
                ValidarExtension(nombreArchivo);
                registros = LeerRegistros(archivo, out columnasArchivo);
            }
            catch (Exception)
            {
                return RegistrarFalla(configuracion, resultado, "error.archivoInvalido", Traductor.T("error.archivoInvalido"));
            }

            if (registros.Count == 0)
            {
                return RegistrarFalla(configuracion, resultado, "error.archivoInvalido", Traductor.T("error.archivoInvalido"));
            }

            // Paso 2: controlar que el archivo traiga todas las columnas mapeadas.
            // Alternativa 6.2 [#ERR027]. Las columnas de mas no molestan: se ignoran
            List<string> faltantes = configuracion.Mapeos
                .Where(m => !columnasArchivo.Contains(m.ColumnaCliente))
                .Select(m => Traductor.T("integracion.error.columnaFaltante", m.ColumnaCliente, Traductor.T(m.CampoTexto)))
                .ToList();

            if (faltantes.Count > 0)
            {
                resultado.Errores.AddRange(faltantes);
                return RegistrarFalla(configuracion, resultado, "error.columnasNoCoinciden", null);
            }

            // Paso 3: pasar cada registro a los campos de Strategic y validarlo.
            // Alternativa 6.3 [#ERR028]: los invalidos se omiten y su motivo queda en Errores
            List<Dictionary<string, string>> validos = TransformarRegistros(configuracion, registros, resultado.Errores);
            resultado.RegistrosError = registros.Count - validos.Count;

            if (validos.Count == 0)
            {
                return RegistrarFalla(configuracion, resultado, "integracion.resultado.sinValidos", null);
            }

            // Paso 4: guardar los validos. La DAL los inserta uno por uno en una
            // transaccion: si alguno falla hace rollback y no queda nada a medias.
            // Alternativa 6.4 [#ERR029]
            try
            {
                int importados;
                int actualizados;

                dalIntegracion.ImportarDatos(configuracion.TipoArchivo, validos, out importados, out actualizados);

                resultado.RegistrosImportados = importados;
                resultado.RegistrosActualizados = actualizados;
            }
            catch (Exception ex)
            {
                resultado.Errores.Insert(0, ex.Message);
                return RegistrarFalla(configuracion, resultado, "error.importacionRevertida", null);
            }

            // Paso 5: cerrar. Las tablas que acaba de escribir la importacion
            // tienen digito verificador y hay que recalcularlo para que el login
            // no lo marque como alterado
            RecalcularDigitoVerificador(configuracion.TipoArchivo);

            // Con algun registro omitido la importacion queda "Con errores", aunque el resto entro
            bool conErrores = resultado.RegistrosError > 0;

            resultado.Estado = conErrores ? BEResultadoIntegracion.EstadoConErrores : BEResultadoIntegracion.EstadoExitosa;
            resultado.Mensaje = conErrores ? "integracion.resultado.conErrores" : "integracion.resultado.exitosa";

            RegistrarHistorial(configuracion, resultado);

            bllEvento.RegistrarEvento(new Evento(
                nombreUsuarioEnSesion,
                ModuloIntegraciones,
                string.Format("Importacion de {0}: {1} importados, {2} actualizados, {3} con error",
                    configuracion.TipoArchivo, resultado.RegistrosImportados, resultado.RegistrosActualizados, resultado.RegistrosError),
                CriticidadCambio));

            return resultado;
        }

        // Recorre el archivo con XmlTextReader: cada hijo de la raiz es un
        // registro y cada hijo de un registro es un campo con su valor
        private List<Dictionary<string, string>> LeerRegistros(Stream archivo, out HashSet<string> columnas)
        {
            List<Dictionary<string, string>> registros = new List<Dictionary<string, string>>();
            columnas = new HashSet<string>();

            using (XmlTextReader lector = new XmlTextReader(archivo))
            {
                // Sin DTD ni recursos externos: el archivo lo sube el usuario y
                // podria intentar leer archivos del servidor (ataque XXE)
                lector.DtdProcessing = DtdProcessing.Prohibit;
                lector.XmlResolver = null;
                lector.WhitespaceHandling = WhitespaceHandling.None;

                // XmlTextReader avanza nodo por nodo; Depth dice el nivel:
                // 0 = raiz (<Catalogo>), 1 = registro (<Articulo>), 2 = campo (<SKU>)
                while (lector.Read())
                {
                    if (lector.NodeType != XmlNodeType.Element || lector.Depth != 1)
                    {
                        continue;
                    }

                    // Empieza un registro nuevo
                    Dictionary<string, string> registro = new Dictionary<string, string>();
                    registros.Add(registro);

                    // <Articulo/> sin campos: queda vacio y la validacion lo rechaza
                    if (lector.IsEmptyElement)
                    {
                        continue;
                    }

                    int profundidad = lector.Depth;
                    lector.Read();

                    // Lee los campos hasta llegar al cierre del registro (</Articulo>)
                    while (!lector.EOF && !(lector.NodeType == XmlNodeType.EndElement && lector.Depth == profundidad))
                    {
                        if (lector.NodeType == XmlNodeType.Element && lector.Depth == profundidad + 1)
                        {
                            string nombre = lector.LocalName;
                            string valor;

                            if (lector.IsEmptyElement)
                            {
                                valor = string.Empty;
                                lector.Read();
                            }
                            else
                            {
                                // Lee el texto del campo y deja el lector en el nodo
                                // siguiente; por eso despues se hace continue sin Read()
                                valor = lector.ReadElementContentAsString();
                            }

                            registro[nombre] = valor.Trim();
                            columnas.Add(nombre);
                            continue;
                        }

                        // Cualquier otro nodo (comentarios, etc.) se saltea
                        lector.Read();
                    }
                }
            }

            return registros;
        }

        // Aplica el mapeo y valida cada registro. Devuelve los validos con los
        // campos de Strategic y los valores en formato invariante; los errores
        // se agregan a la lista con el numero de registro
        private List<Dictionary<string, string>> TransformarRegistros(BEConfiguracionIntegracion configuracion, List<Dictionary<string, string>> registros, List<string> errores)
        {
            List<Dictionary<string, string>> validos = new List<Dictionary<string, string>>();

            // Ventas y Stock tienen que referirse a productos que existan. Se traen
            // los codigos una sola vez en vez de consultar la base por cada registro
            HashSet<string> codigosProductos = configuracion.TipoArchivo == TipoProductos
                ? null
                : new HashSet<string>(dalIntegracion.TraerCodigosProductos(), StringComparer.OrdinalIgnoreCase);

            // Codigos ya vistos en este archivo, para detectar repetidos
            HashSet<string> codigosEnArchivo = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < registros.Count; i++)
            {
                // Numero de registro tal como lo cuenta el usuario (desde 1), para los mensajes
                int numero = i + 1;

                // Mapeo: de { "SKU": "SKU-1001" } a { "Codigo": "SKU-1001" }. Los
                // campos sin columna asignada o con valor vacio no se cargan
                Dictionary<string, string> valores = new Dictionary<string, string>();

                foreach (BECampoIntegracion campo in Campos[configuracion.TipoArchivo])
                {
                    string columna = configuracion.ColumnaDe(campo.Nombre);
                    string valor;

                    if (columna != null && registros[i].TryGetValue(columna, out valor) && valor.Length > 0)
                    {
                        valores[campo.Nombre] = valor;
                    }
                }

                string error = ValidarRegistro(configuracion.TipoArchivo, numero, valores, codigosProductos, codigosEnArchivo);

                if (error != null)
                {
                    errores.Add(error);
                    continue;
                }

                validos.Add(valores);
            }

            return validos;
        }

        // Devuelve la descripcion del primer problema del registro, o null si es valido.
        // Normaliza los valores en el diccionario a medida que los valida
        private string ValidarRegistro(string tipoArchivo, int numero, Dictionary<string, string> valores, HashSet<string> codigosProductos, HashSet<string> codigosEnArchivo)
        {
            foreach (BECampoIntegracion campo in Campos[tipoArchivo])
            {
                if (campo.Obligatorio && !valores.ContainsKey(campo.Nombre))
                {
                    return Traductor.T("integracion.error.campoVacio", numero, Traductor.T(campo.CampoTexto));
                }
            }

            // Cada validacion devuelve null si esta bien o el mensaje del problema;
            // el ?? corta en el primer error que aparece
            string error;

            switch (tipoArchivo)
            {
                case TipoProductos:
                    // Los largos son los de las columnas de la tabla Producto
                    error = ValidarTexto(numero, valores, "Codigo", 50)
                        ?? ValidarTexto(numero, valores, "Nombre", 255)
                        ?? ValidarTexto(numero, valores, "Categoria", 100)
                        ?? ValidarTexto(numero, valores, "Marca", 100)
                        ?? ValidarDecimal(numero, valores, "Precio")
                        ?? ValidarEntero(numero, valores, "Stock", 0)
                        ?? ValidarEntero(numero, valores, "StockMinimo", 0)
                        ?? ValidarEntero(numero, valores, "StockMaximo", 0)
                        ?? ValidarEstado(numero, valores, EstadosProducto);

                    // Add devuelve false si el codigo ya estaba: si un producto viene dos
                    // veces en el archivo se omite el segundo, para no pisar el primero
                    if (error == null && !codigosEnArchivo.Add(valores["Codigo"]))
                    {
                        error = Traductor.T("integracion.error.codigoRepetido", numero, valores["Codigo"]);
                    }

                    return error;

                case TipoVentas:
                    // En ventas un mismo NroVenta se repite a proposito: una fila por item
                    error = ValidarTexto(numero, valores, "NroVenta", 50)
                        ?? ValidarFecha(numero, valores, "Fecha")
                        ?? ValidarEntero(numero, valores, "Cantidad", 1)
                        ?? ValidarDecimal(numero, valores, "PrecioVenta")
                        ?? ValidarEstado(numero, valores, EstadosVenta)
                        ?? ValidarProductoExistente(numero, valores, codigosProductos);

                    return error;

                default:
                    error = ValidarEntero(numero, valores, "Stock", 0)
                        ?? ValidarProductoExistente(numero, valores, codigosProductos);

                    if (error == null && !codigosEnArchivo.Add(valores["CodigoProducto"]))
                    {
                        error = Traductor.T("integracion.error.codigoRepetido", numero, valores["CodigoProducto"]);
                    }

                    return error;
            }
        }

        private string ValidarTexto(int numero, Dictionary<string, string> valores, string campo, int largoMaximo)
        {
            string valor;

            if (valores.TryGetValue(campo, out valor) && valor.Length > largoMaximo)
            {
                return Traductor.T("integracion.error.valorLargo", numero, Traductor.T("integracion.campo." + campo), largoMaximo);
            }

            return null;
        }

        // Acepta punto o coma decimal (sin separador de miles) y un signo $ adelante
        private string ValidarDecimal(int numero, Dictionary<string, string> valores, string campo)
        {
            string valor;

            if (!valores.TryGetValue(campo, out valor))
            {
                return null;
            }

            string texto = valor.Replace("$", string.Empty).Trim();

            if (texto.Contains(",") && !texto.Contains("."))
            {
                texto = texto.Replace(",", ".");
            }

            decimal importe;

            // 99999999.99 es el maximo que entra en un DECIMAL(10,2)
            if (!decimal.TryParse(texto, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out importe) || importe > 99999999.99m)
            {
                return Traductor.T("integracion.error.numeroInvalido", numero, Traductor.T("integracion.campo." + campo), valor);
            }

            // Se reemplaza por el valor normalizado (punto decimal), que es el que entiende el SP
            valores[campo] = Math.Round(importe, 2).ToString(CultureInfo.InvariantCulture);
            return null;
        }

        private string ValidarEntero(int numero, Dictionary<string, string> valores, string campo, int minimo)
        {
            string valor;

            if (!valores.TryGetValue(campo, out valor))
            {
                return null;
            }

            int entero;

            if (!int.TryParse(valor, NumberStyles.Integer, CultureInfo.InvariantCulture, out entero) || entero < minimo)
            {
                return Traductor.T("integracion.error.enteroInvalido", numero, Traductor.T("integracion.campo." + campo), valor, minimo);
            }

            valores[campo] = entero.ToString(CultureInfo.InvariantCulture);
            return null;
        }

        private string ValidarFecha(int numero, Dictionary<string, string> valores, string campo)
        {
            string valor = valores[campo];
            DateTime fecha;

            if (!DateTime.TryParseExact(valor, FormatosFecha, CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha))
            {
                return Traductor.T("integracion.error.fechaInvalida", numero, Traductor.T("integracion.campo." + campo), valor);
            }

            // Formato ISO: SQL Server lo convierte a DATETIME sin depender del idioma
            valores[campo] = fecha.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
            return null;
        }

        // Compara sin distinguir mayusculas y deja el valor escrito como en el sistema
        private string ValidarEstado(int numero, Dictionary<string, string> valores, string[] permitidos)
        {
            string valor;

            if (!valores.TryGetValue("Estado", out valor))
            {
                return null;
            }

            string estado = permitidos.FirstOrDefault(e => string.Equals(e, valor, StringComparison.OrdinalIgnoreCase));

            if (estado == null)
            {
                return Traductor.T("integracion.error.estadoInvalido", numero, valor, string.Join(", ", permitidos));
            }

            valores["Estado"] = estado;
            return null;
        }

        private string ValidarProductoExistente(int numero, Dictionary<string, string> valores, HashSet<string> codigosProductos)
        {
            string codigo = valores["CodigoProducto"];

            if (!codigosProductos.Contains(codigo))
            {
                return Traductor.T("integracion.error.productoInexistente", numero, codigo);
            }

            return null;
        }

        // Cierra una importacion que no guardo nada: estado Fallida, mensaje para
        // la pantalla y registro en el historial con el motivo
        private BEResultadoIntegracion RegistrarFalla(BEConfiguracionIntegracion configuracion, BEResultadoIntegracion resultado, string mensaje, string error)
        {
            resultado.Estado = BEResultadoIntegracion.EstadoFallida;
            resultado.Mensaje = mensaje;

            // Si fallo el SP la transaccion se revirtio: no quedo nada importado
            resultado.RegistrosImportados = 0;
            resultado.RegistrosActualizados = 0;

            if (error != null)
            {
                resultado.Errores.Add(error);
            }

            // Un error critico tambien queda en el historial de sincronizaciones
            RegistrarHistorial(configuracion, resultado);

            return resultado;
        }

        // El DV se recalcula una sola vez al terminar la importacion, nunca por
        // registro: el calculo lee la tabla entera
        private void RecalcularDigitoVerificador(string tipoArchivo)
        {
            if (tipoArchivo == TipoVentas)
            {
                bllDigitoVerificador.PersistirDV("Venta");
                bllDigitoVerificador.PersistirDV("ItemVenta");
                return;
            }

            // Productos y Stock actualizan el producto y dejan el stock en su historial
            bllDigitoVerificador.PersistirDV("Producto");
            bllDigitoVerificador.PersistirDV("HistorialStock");

            if (tipoArchivo == TipoProductos)
            {
                bllDigitoVerificador.PersistirDV("HistorialPrecioProducto");
            }
        }

        private void RegistrarHistorial(BEConfiguracionIntegracion configuracion, BEResultadoIntegracion resultado)
        {
            dalIntegracion.RegistrarSincronizacion(new BEHistorialSincronizacion
            {
                IdConfiguracion = configuracion.IdConfiguracion,
                TipoArchivo = configuracion.TipoArchivo,
                RegistrosImportados = resultado.RegistrosImportados,
                RegistrosActualizados = resultado.RegistrosActualizados,
                RegistrosError = resultado.RegistrosError,
                Estado = resultado.Estado,
                Detalle = ArmarDetalle(resultado.Errores)
            });

            bllDigitoVerificador.PersistirDV("HistorialSincronizacion");
        }

        // Un error por linea, recortado al largo de la columna
        private string ArmarDetalle(List<string> errores)
        {
            if (errores.Count == 0)
            {
                return null;
            }

            List<string> lineas = new List<string>();
            int largo = 0;

            for (int i = 0; i < errores.Count; i++)
            {
                // Se reserva lugar para la linea "... y N errores mas" por si no entran todos
                string resto = Traductor.T("integracion.masErrores", errores.Count - i);

                if (largo + errores[i].Length + 1 + resto.Length > LargoMaximoDetalle)
                {
                    lineas.Add(resto);
                    break;
                }

                lineas.Add(errores[i]);
                largo = largo + errores[i].Length + 1;
            }

            return string.Join("\n", lineas);
        }

        private void ValidarExtension(string nombreArchivo)
        {
            if (string.IsNullOrEmpty(nombreArchivo) || !string.Equals(Path.GetExtension(nombreArchivo), ".xml", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception(Traductor.T("error.archivoInvalido"));
            }
        }

        private XmlReaderSettings ConfiguracionLectura()
        {
            // Sin DTD ni recursos externos: el archivo lo sube el usuario
            return new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
                IgnoreWhitespace = true,
                IgnoreComments = true
            };
        }

        #endregion

        #region Consultar Historial de Sincronizaciones - CU-006-031

        public List<BEHistorialSincronizacion> TraerHistorialSincronizaciones()
        {
            return dalIntegracion.TraerHistorialSincronizaciones();
        }

        // Los filtros se aplican sobre la lista ya consultada
        public List<BEHistorialSincronizacion> FiltrarHistorial(List<BEHistorialSincronizacion> historial, string tipoArchivo, string estado, DateTime? fechaDesde, DateTime? fechaHasta)
        {
            if (fechaDesde.HasValue && fechaHasta.HasValue && fechaHasta.Value < fechaDesde.Value)
            {
                throw new Exception(Traductor.T("filtroFechas.errorRango"));
            }

            return historial
                .Where(h => string.IsNullOrEmpty(tipoArchivo) || h.TipoArchivo == tipoArchivo)
                .Where(h => string.IsNullOrEmpty(estado) || h.Estado == estado)
                .Where(h => !fechaDesde.HasValue || h.FechaEjecucion >= fechaDesde.Value.Date)
                .Where(h => !fechaHasta.HasValue || h.FechaEjecucion < fechaHasta.Value.Date.AddDays(1))
                .ToList();
        }

        #endregion

        private void ValidarTipo(string tipoArchivo)
        {
            if (!TiposArchivo.Contains(tipoArchivo))
            {
                throw new Exception(Traductor.T("error.tipoArchivoInvalido"));
            }
        }
    }
}
