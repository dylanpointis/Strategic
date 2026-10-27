using BE;
using DAL;
using Services;
using System;
using System.Collections.Generic;

namespace BLL
{
    // CU-002-005 - Generar Recomendaciones.
    // Las recomendaciones se arman en el momento recorriendo los productos con
    // reglas fijas: stock contra el minimo, precio contra la competencia y
    // ventas del ultimo mes contra el anterior. No se guardan solas: solo se
    // persiste la que el usuario elige guardar
    public class BLLRecomendacion
    {
        public const string ReglaSinStock = "SinStock";
        public const string ReglaStockBajo = "StockBajo";
        public const string ReglaSobrestock = "Sobrestock";
        public const string ReglaPrecioAlto = "PrecioAlto";
        public const string ReglaPrecioAltoVentasBajas = "PrecioAltoVentasBajas";
        public const string ReglaCaidaVentas = "CaidaVentas";

        private const string ModuloRecomendaciones = "Recomendaciones";
        private const int CriticidadCambio = 2;

        // El precio es alto si supera en mas de un 15% al de la competencia
        private const decimal MargenPrecioAlto = 0.15m;

        // Una caida de ventas es relevante a partir del 30%
        private const decimal PorcentajeCaidaVentas = 0.30m;

        // Hay sobrestock si alcanza para mas de 3 meses al ritmo de venta promedio
        private const int MesesSobrestock = 3;

        private const int DiasPorMes = 30;

        private readonly BLLProducto bllProducto = new BLLProducto();
        private readonly BLLCompetencia bllCompetencia = new BLLCompetencia();
        private readonly BLLVenta bllVenta = new BLLVenta();
        private readonly BLLEvento bllEvento = new BLLEvento();
        private readonly DALRecomendacion dalRecomendacion = new DALRecomendacion();
        private readonly BLLDigitoVerificador bllDigitoVerificador = new BLLDigitoVerificador();

        #region Generacion

        public List<BERecomendacion> GenerarRecomendaciones()
        {
            try
            {
                List<BEProducto> productos = TraerProductosAnalizables();

                // Alternativa 2.1 del CU [#ERR009]
                if (productos.Count == 0)
                {
                    throw new DatosInsuficientesException(Traductor.T("recomendaciones.sinDatos"));
                }

                List<BERecomendacion> recomendaciones = new List<BERecomendacion>();
                DateTime fechaGeneracion = DateTime.Now;

                foreach (BEProducto producto in productos)
                {
                    BERecomendacion recomendacion = CompararStockConStockMinimo(producto);

                    if (recomendacion != null)
                    {
                        AgregarRecomendacion(recomendaciones, recomendacion, producto, fechaGeneracion);
                    }
                }

                // La publicacion mas barata de cada producto con precio alto se
                // guarda para la regla combinada con las ventas
                Dictionary<int, BEProductoCompetencia> preciosAltos = new Dictionary<int, BEProductoCompetencia>();

                foreach (BEProducto producto in productos)
                {
                    List<BEProductoCompetencia> publicaciones = bllCompetencia.TraerProductoCompetencia(producto.IdProducto);
                    BEProductoCompetencia masBarata = CompararPrecioConCompetencia(producto, publicaciones);

                    if (masBarata != null)
                    {
                        preciosAltos.Add(producto.IdProducto, masBarata);
                        AgregarRecomendacion(recomendaciones, CrearPrecioAlto(producto, masBarata), producto, fechaGeneracion);
                    }
                }

                // Los periodos terminan en la ultima sincronizacion y no en la
                // fecha de hoy: con datos viejos, todas las ventas parecerian caer
                DateTime fechaReferencia = CalcularFechaReferencia(productos);

                List<BEVenta> ventasUltimoMes = QuitarCanceladas(
                    bllVenta.TraerListaVentaPorFecha(fechaReferencia.AddDays(-(DiasPorMes - 1)), fechaReferencia));
                List<BEVenta> ventasUltimosDosMeses = QuitarCanceladas(
                    bllVenta.TraerListaVentaPorFecha(fechaReferencia.AddDays(-(DiasPorMes * 2 - 1)), fechaReferencia));

                foreach (BEProducto producto in productos)
                {
                    int unidadesMesActual = SumarUnidades(ventasUltimoMes, producto.IdProducto);
                    int unidadesMesAnterior = SumarUnidades(ventasUltimosDosMeses, producto.IdProducto) - unidadesMesActual;

                    BEProductoCompetencia precioAlto;
                    preciosAltos.TryGetValue(producto.IdProducto, out precioAlto);

                    foreach (BERecomendacion recomendacion in CompararVentasMesAnteriorYActual(
                        producto, unidadesMesAnterior, unidadesMesActual, fechaReferencia, precioAlto))
                    {
                        // La regla combinada reemplaza a la de precio alto del mismo producto
                        if (recomendacion.Regla == ReglaPrecioAltoVentasBajas)
                        {
                            recomendaciones.RemoveAll(r => r.IdProducto == producto.IdProducto && r.Regla == ReglaPrecioAlto);
                        }

                        AgregarRecomendacion(recomendaciones, recomendacion, producto, fechaGeneracion);
                    }
                }

                Ordenar(recomendaciones);

                return recomendaciones;
            }
            catch (DatosInsuficientesException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Alternativa 3.2 del CU [#ERR018]
                throw new Exception(Traductor.T("error.analisisRecomendaciones", ex.Message));
            }
        }

        // Los productos inactivos no se venden, no tiene sentido recomendar sobre ellos
        private List<BEProducto> TraerProductosAnalizables()
        {
            List<BEProducto> productos = new List<BEProducto>();

            foreach (BEProducto producto in bllProducto.TraerListaProductos())
            {
                if (producto.Estado != "Inactivo")
                {
                    productos.Add(producto);
                }
            }

            return productos;
        }

        private BERecomendacion CompararStockConStockMinimo(BEProducto producto)
        {
            if (producto.Stock <= 0)
            {
                return new BERecomendacion
                {
                    Regla = ReglaSinStock,
                    Tipo = BERecomendacion.TipoStock,
                    Prioridad = BERecomendacion.PrioridadAlta,
                    Titulo = Traductor.T("recomendaciones.sinStock.titulo"),
                    Descripcion = Traductor.T("recomendaciones.sinStock.descripcion", producto.Nombre),
                    Justificacion = Traductor.T("recomendaciones.sinStock.justificacion",
                        producto.Stock,
                        producto.StockMinimo.HasValue ? producto.StockMinimo.Value.ToString() : Traductor.T("recomendaciones.sinDefinir")),
                    AccionSugerida = Traductor.T("recomendaciones.sinStock.accion")
                };
            }

            if (!producto.StockBajoMinimo)
            {
                return null;
            }

            // Si hay un maximo definido se sugiere cuanto reponer para llegar a el
            string accion = producto.StockMaximo.HasValue && producto.StockMaximo.Value > producto.Stock
                ? Traductor.T("recomendaciones.stockBajo.accion", producto.StockMaximo.Value - producto.Stock, producto.StockMaximo.Value)
                : Traductor.T("recomendaciones.stockBajo.accionSinMaximo", producto.StockMinimo.Value);

            return new BERecomendacion
            {
                Regla = ReglaStockBajo,
                Tipo = BERecomendacion.TipoStock,
                Prioridad = BERecomendacion.PrioridadMedia,
                Titulo = Traductor.T("recomendaciones.stockBajo.titulo"),
                Descripcion = Traductor.T("recomendaciones.stockBajo.descripcion", producto.Nombre, producto.Stock, producto.StockMinimo.Value),
                Justificacion = Traductor.T("recomendaciones.stockBajo.justificacion", producto.Stock, producto.StockMinimo.Value),
                AccionSugerida = accion
            };
        }

        // Devuelve la publicacion mas barata si el precio propio la supera en
        // mas del margen; si no hay precio alto devuelve null
        private BEProductoCompetencia CompararPrecioConCompetencia(BEProducto producto, List<BEProductoCompetencia> publicaciones)
        {
            BEProductoCompetencia masBarata = null;

            foreach (BEProductoCompetencia publicacion in publicaciones)
            {
                // Solo cuentan las publicaciones monitoreadas que ya tienen un precio relevado
                if (publicacion.Estado != "Activa" || !publicacion.UltimoPrecio.HasValue || publicacion.UltimoPrecio.Value <= 0)
                {
                    continue;
                }

                if (masBarata == null || publicacion.UltimoPrecio.Value < masBarata.UltimoPrecio.Value)
                {
                    masBarata = publicacion;
                }
            }

            if (masBarata == null || producto.Precio <= masBarata.UltimoPrecio.Value * (1 + MargenPrecioAlto))
            {
                return null;
            }

            return masBarata;
        }

        private BERecomendacion CrearPrecioAlto(BEProducto producto, BEProductoCompetencia masBarata)
        {
            return new BERecomendacion
            {
                Regla = ReglaPrecioAlto,
                Tipo = BERecomendacion.TipoPrecio,
                Prioridad = BERecomendacion.PrioridadMedia,
                Titulo = Traductor.T("recomendaciones.precioAlto.titulo"),
                Descripcion = Traductor.T("recomendaciones.precioAlto.descripcion",
                    producto.Nombre,
                    producto.Precio,
                    DiferenciaPorcentaje(producto.Precio, masBarata.UltimoPrecio.Value),
                    masBarata.NombreCompetidor,
                    masBarata.UltimoPrecio.Value),
                Justificacion = Traductor.T("recomendaciones.precioAlto.justificacion",
                    MargenPrecioAlto * 100,
                    masBarata.FechaUltimoPrecio),
                AccionSugerida = Traductor.T("recomendaciones.precioAlto.accion",
                    PrecioMaximoSugerido(masBarata),
                    MargenPrecioAlto * 100)
            };
        }

        // Compara las unidades vendidas en los ultimos 30 dias contra los 30
        // anteriores. Puede devolver mas de una recomendacion para el producto
        private List<BERecomendacion> CompararVentasMesAnteriorYActual(BEProducto producto, int unidadesMesAnterior, int unidadesMesActual,
            DateTime fechaReferencia, BEProductoCompetencia precioAlto)
        {
            List<BERecomendacion> recomendaciones = new List<BERecomendacion>();

            DateTime inicioMesActual = fechaReferencia.AddDays(-(DiasPorMes - 1));
            DateTime finMesAnterior = inicioMesActual.AddDays(-1);
            DateTime inicioMesAnterior = fechaReferencia.AddDays(-(DiasPorMes * 2 - 1));

            decimal promedioMensual = (unidadesMesAnterior + unidadesMesActual) / 2m;

            if (producto.Stock > 0 && promedioMensual == 0)
            {
                recomendaciones.Add(CrearSobrestock(producto,
                    Traductor.T("recomendaciones.sobrestock.descripcionSinVentas", producto.Nombre, producto.Stock),
                    Traductor.T("recomendaciones.sobrestock.justificacionSinVentas", inicioMesAnterior, fechaReferencia, producto.Stock)));
            }
            else if (producto.Stock > promedioMensual * MesesSobrestock)
            {
                recomendaciones.Add(CrearSobrestock(producto,
                    Traductor.T("recomendaciones.sobrestock.descripcion", producto.Nombre, producto.Stock, promedioMensual),
                    Traductor.T("recomendaciones.sobrestock.justificacion",
                        MesesSobrestock, inicioMesAnterior, fechaReferencia, producto.Stock, promedioMensual * MesesSobrestock)));
            }

            if (unidadesMesAnterior <= 0 || unidadesMesActual >= unidadesMesAnterior)
            {
                return recomendaciones;
            }

            if (precioAlto != null)
            {
                recomendaciones.Add(new BERecomendacion
                {
                    Regla = ReglaPrecioAltoVentasBajas,
                    Tipo = BERecomendacion.TipoPrecio,
                    Prioridad = BERecomendacion.PrioridadAlta,
                    Titulo = Traductor.T("recomendaciones.precioAltoVentasBajas.titulo"),
                    Descripcion = Traductor.T("recomendaciones.precioAltoVentasBajas.descripcion",
                        producto.Nombre,
                        DiferenciaPorcentaje(producto.Precio, precioAlto.UltimoPrecio.Value),
                        precioAlto.NombreCompetidor,
                        precioAlto.UltimoPrecio.Value,
                        unidadesMesAnterior,
                        unidadesMesActual),
                    Justificacion = Traductor.T("recomendaciones.precioAltoVentasBajas.justificacion",
                        MargenPrecioAlto * 100,
                        inicioMesActual,
                        fechaReferencia,
                        inicioMesAnterior,
                        finMesAnterior),
                    AccionSugerida = Traductor.T("recomendaciones.precioAltoVentasBajas.accion", PrecioMaximoSugerido(precioAlto))
                });
            }
            else if (unidadesMesActual <= unidadesMesAnterior * (1 - PorcentajeCaidaVentas))
            {
                recomendaciones.Add(new BERecomendacion
                {
                    Regla = ReglaCaidaVentas,
                    Tipo = BERecomendacion.TipoVentas,
                    Prioridad = BERecomendacion.PrioridadMedia,
                    Titulo = Traductor.T("recomendaciones.caidaVentas.titulo"),
                    Descripcion = Traductor.T("recomendaciones.caidaVentas.descripcion",
                        producto.Nombre,
                        unidadesMesActual,
                        unidadesMesAnterior,
                        DiferenciaPorcentaje(unidadesMesActual, unidadesMesAnterior)),
                    Justificacion = Traductor.T("recomendaciones.caidaVentas.justificacion",
                        PorcentajeCaidaVentas * 100,
                        inicioMesActual,
                        fechaReferencia,
                        inicioMesAnterior,
                        finMesAnterior),
                    AccionSugerida = Traductor.T("recomendaciones.caidaVentas.accion")
                });
            }

            return recomendaciones;
        }

        private BERecomendacion CrearSobrestock(BEProducto producto, string descripcion, string justificacion)
        {
            return new BERecomendacion
            {
                Regla = ReglaSobrestock,
                Tipo = BERecomendacion.TipoStock,
                Prioridad = BERecomendacion.PrioridadBaja,
                Titulo = Traductor.T("recomendaciones.sobrestock.titulo"),
                Descripcion = descripcion,
                Justificacion = justificacion,
                AccionSugerida = Traductor.T("recomendaciones.sobrestock.accion")
            };
        }

        private void AgregarRecomendacion(List<BERecomendacion> recomendaciones, BERecomendacion recomendacion, BEProducto producto, DateTime fechaGeneracion)
        {
            recomendacion.IdProducto = producto.IdProducto;
            recomendacion.CodigoProducto = producto.Codigo;
            recomendacion.NombreProducto = producto.Nombre;
            recomendacion.FechaGeneracion = fechaGeneracion;

            recomendaciones.Add(recomendacion);
        }

        // Primero las de prioridad alta; dentro de cada prioridad, por tipo y producto
        private void Ordenar(List<BERecomendacion> recomendaciones)
        {
            recomendaciones.Sort(delegate (BERecomendacion a, BERecomendacion b)
            {
                int resultado = a.OrdenPrioridad.CompareTo(b.OrdenPrioridad);

                if (resultado == 0)
                {
                    resultado = string.Compare(a.Tipo, b.Tipo, StringComparison.Ordinal);
                }

                if (resultado == 0)
                {
                    resultado = string.Compare(a.NombreProducto, b.NombreProducto, StringComparison.CurrentCulture);
                }

                return resultado;
            });

            for (int i = 0; i < recomendaciones.Count; i++)
            {
                recomendaciones[i].Numero = i + 1;
            }
        }

        private DateTime CalcularFechaReferencia(List<BEProducto> productos)
        {
            DateTime? ultimaSincronizacion = null;

            foreach (BEProducto producto in productos)
            {
                if (producto.FechaSincronizacion.HasValue
                    && (!ultimaSincronizacion.HasValue || producto.FechaSincronizacion.Value > ultimaSincronizacion.Value))
                {
                    ultimaSincronizacion = producto.FechaSincronizacion.Value;
                }
            }

            if (!ultimaSincronizacion.HasValue || ultimaSincronizacion.Value.Date > DateTime.Today)
            {
                return DateTime.Today;
            }

            return ultimaSincronizacion.Value.Date;
        }

        // Las ventas canceladas no cuentan, igual que en el dashboard
        private List<BEVenta> QuitarCanceladas(List<BEVenta> ventas)
        {
            List<BEVenta> vigentes = new List<BEVenta>();

            foreach (BEVenta venta in ventas)
            {
                if (venta.Estado != "Cancelada")
                {
                    vigentes.Add(venta);
                }
            }

            return vigentes;
        }

        private int SumarUnidades(List<BEVenta> ventas, int idProducto)
        {
            int unidades = 0;

            foreach (BEVenta venta in ventas)
            {
                foreach (BEItemVenta item in venta.Items)
                {
                    if (item.IdProducto == idProducto)
                    {
                        unidades = unidades + item.Cantidad;
                    }
                }
            }

            return unidades;
        }

        private decimal DiferenciaPorcentaje(decimal valor, decimal referencia)
        {
            return Math.Round((valor - referencia) / referencia * 100, 1);
        }

        // Precio mas alto que todavia queda dentro del margen de la competencia
        private decimal PrecioMaximoSugerido(BEProductoCompetencia masBarata)
        {
            return Math.Round(masBarata.UltimoPrecio.Value * (1 + MargenPrecioAlto), 2);
        }

        #endregion

        #region Filtros

        public List<BERecomendacion> FiltrarRecomendaciones(List<BERecomendacion> recomendaciones, string tipo, string prioridad, int? idProducto)
        {
            if (idProducto.HasValue && idProducto.Value <= 0)
            {
                throw new Exception(Traductor.T("error.productoInvalido"));
            }

            List<BERecomendacion> filtradas = new List<BERecomendacion>();

            if (recomendaciones == null)
            {
                return filtradas;
            }

            foreach (BERecomendacion recomendacion in recomendaciones)
            {
                if ((string.IsNullOrEmpty(tipo) || recomendacion.Tipo == tipo)
                    && (string.IsNullOrEmpty(prioridad) || recomendacion.Prioridad == prioridad)
                    && (!idProducto.HasValue || recomendacion.IdProducto == idProducto.Value))
                {
                    filtradas.Add(recomendacion);
                }
            }

            return filtradas;
        }

        public int ContarPorPrioridad(List<BERecomendacion> recomendaciones, string prioridad)
        {
            int cantidad = 0;

            foreach (BERecomendacion recomendacion in recomendaciones)
            {
                if (recomendacion.Prioridad == prioridad)
                {
                    cantidad = cantidad + 1;
                }
            }

            return cantidad;
        }

        #endregion

        #region Guardar

        // El usuario que guarda viaja en la propia recomendacion (NombreUsuario)
        public void RegistrarRecomendacion(BERecomendacion recomendacion)
        {
            if (recomendacion == null || recomendacion.IdProducto <= 0)
            {
                throw new Exception(Traductor.T("error.seleccioneRecomendacion"));
            }

            // La misma regla para el mismo producto se guarda una sola vez por dia
            if (recomendacion.Guardada
                || dalRecomendacion.ContarRecomendacionesGuardadas(recomendacion.IdProducto, recomendacion.Regla, recomendacion.FechaGeneracion) > 0)
            {
                throw new Exception(Traductor.T("error.recomendacionYaGuardada"));
            }

            recomendacion.IdRecomendacion = dalRecomendacion.RegistrarRecomendacion(recomendacion);
            recomendacion.Guardada = true;

            bllDigitoVerificador.PersistirDV("Recomendacion");

            bllEvento.RegistrarEvento(new Evento(
                recomendacion.NombreUsuario,
                ModuloRecomendaciones,
                string.Format("Recomendacion guardada: {0} - {1}", recomendacion.Regla, recomendacion.NombreProducto),
                CriticidadCambio));
        }

        #endregion
    }
}
