using BE;
using DAL;
using Services.Prediccion;
using System;
using System.Collections.Generic;

namespace BLL
{
    
    /// CU-002-006 - Generar Predicciones.
    ///
    /// Cada tipo de predicción arma una serie (fecha, valor) a partir del
    /// histórico del producto, le ajusta una recta por regresión lineal y la
    /// prolonga hacia adelante la cantidad de días pedida. La recta la
    /// calcula el servicio RegresionLineal con ML.NET; acá se decide qué
    /// serie usar y cómo leer el resultado.
    
    public class BLLPrediccion
    {
        // Puntos como maximo en la proyeccion, para que el grafico se lea
        private const int MaximoPuntosProyeccion = 30;

        private readonly DALProducto dalProducto = new DALProducto();
        private readonly DALVenta dalVenta = new DALVenta();

        
        /// Tipos de predicción disponibles, en el orden en que se ofrecen.
        
        public List<KeyValuePair<string, string>> TraerTipos()
        {
            List<KeyValuePair<string, string>> tipos = new List<KeyValuePair<string, string>>();

            tipos.Add(new KeyValuePair<string, string>(BEPrediccion.TipoPrecio, "Precio del producto"));
            tipos.Add(new KeyValuePair<string, string>(BEPrediccion.TipoStock, "Stock disponible"));
            tipos.Add(new KeyValuePair<string, string>(BEPrediccion.TipoDemanda, "Demanda (unidades vendidas por semana)"));

            return tipos;
        }

        public BEPrediccion Generar(string tipo, int idProducto, int horizonteDias)
        {
            if (idProducto <= 0)
            {
                throw new Exception("Debe seleccionar un producto");
            }

            if (horizonteDias < 1 || horizonteDias > 365)
            {
                throw new Exception("El horizonte tiene que estar entre 1 y 365 dias");
            }

            BEProducto producto = dalProducto.TraerProductoPorId(idProducto);

            if (producto == null)
            {
                throw new Exception("No se encontro el producto seleccionado");
            }

            switch (tipo)
            {
                case BEPrediccion.TipoPrecio:
                    return PredecirPrecio(producto, horizonteDias);
                case BEPrediccion.TipoStock:
                    return PredecirStock(producto, horizonteDias);
                case BEPrediccion.TipoDemanda:
                    return PredecirDemanda(producto, horizonteDias);
                default:
                    throw new Exception("Debe seleccionar un tipo de prediccion");
            }
        }

        #region Tipos de prediccion

        private BEPrediccion PredecirPrecio(BEProducto producto, int horizonteDias)
        {
            List<BEPuntoPrediccion> historial = new List<BEPuntoPrediccion>();

            foreach (BEHistorialPrecio registro in dalProducto.TraerHistorialPrecios(producto.IdProducto, null, null))
            {
                historial.Add(Punto(registro.Fecha, registro.Precio));
            }

            BEPrediccion prediccion = Proyectar(historial, horizonteDias, 1);

            prediccion.Tipo = BEPrediccion.TipoPrecio;
            prediccion.NombreTipo = "Precio del producto";
            prediccion.Prefijo = "$";
            prediccion.Unidad = "por dia";

            return Completar(prediccion, producto, horizonteDias);
        }

        private BEPrediccion PredecirStock(BEProducto producto, int horizonteDias)
        {
            List<BEPuntoPrediccion> historial = new List<BEPuntoPrediccion>();

            foreach (BEHistorialStock registro in dalProducto.TraerHistorialStock(producto.IdProducto, null, null))
            {
                historial.Add(Punto(registro.Fecha, registro.Stock));
            }

            BEPrediccion prediccion = Proyectar(historial, horizonteDias, 1);

            prediccion.Tipo = BEPrediccion.TipoStock;
            prediccion.NombreTipo = "Stock disponible";
            prediccion.Prefijo = string.Empty;
            prediccion.Unidad = "unidades por dia";

            return Completar(prediccion, producto, horizonteDias);
        }

        // La demanda se mide por semana: las ventas diarias de un producto son
        // pocas y con muchos ceros, por dia la recta no dice nada
        private BEPrediccion PredecirDemanda(BEProducto producto, int horizonteDias)
        {
            List<BEPuntoPrediccion> historial = AgruparPorSemana(dalVenta.TraerUnidadesPorDia(producto.IdProducto));

            BEPrediccion prediccion = Proyectar(historial, horizonteDias, 7);

            prediccion.Tipo = BEPrediccion.TipoDemanda;
            prediccion.NombreTipo = "Demanda semanal";
            prediccion.Prefijo = string.Empty;
            prediccion.Unidad = "unidades por semana";

            // La pendiente se informa por semana, que es el paso de la serie
            prediccion.Pendiente = prediccion.Pendiente * 7;

            return Completar(prediccion, producto, horizonteDias);
        }

        #endregion

        #region Regresion y proyeccion

        
        /// Ajusta la recta al histórico y la prolonga "horizonteDias" hacia
        /// adelante. "pasoMinimo" es cada cuántos días hay un punto en la
        /// serie (1 en precio y stock, 7 en demanda semanal).
        
        private BEPrediccion Proyectar(List<BEPuntoPrediccion> historial, int horizonteDias, int pasoMinimo)
        {
            // Alternativa 4.1 del CU [#ERR011]
            if (historial.Count < RegresionLineal.MinimoPuntos)
            {
                throw new Exception(string.Format(
                    "No hay datos suficientes para generar predicciones: hacen falta al menos {0} registros y hay {1}",
                    RegresionLineal.MinimoPuntos, historial.Count));
            }

            DateTime origen = historial[0].Fecha.Date;
            List<double> xs = new List<double>();
            List<double> ys = new List<double>();

            foreach (BEPuntoPrediccion punto in historial)
            {
                xs.Add((punto.Fecha.Date - origen).TotalDays);
                ys.Add((double)punto.Valor);
            }

            ResultadoRegresion recta;

            try
            {
                recta = RegresionLineal.Ajustar(xs, ys);
            }
            catch (ArgumentException ex)
            {
                // Datos que no alcanzan para una recta (todos en la misma fecha)
                throw new Exception("No hay datos suficientes para generar predicciones: " + ex.Message);
            }
            catch (Exception ex)
            {
                // Alternativa 4.2 del CU [#ERR024]
                throw new Exception("Error al ejecutar el algoritmo de prediccion: " + ex.Message);
            }

            BEPrediccion prediccion = new BEPrediccion();
            prediccion.Historial = historial;
            prediccion.CantidadPuntos = historial.Count;
            prediccion.Pendiente = (decimal)recta.Pendiente;
            prediccion.R2 = recta.R2;
            prediccion.ErrorMedio = (decimal)recta.ErrorMedioAbsoluto;

            DateTime fechaBase = historial[historial.Count - 1].Fecha.Date;
            double xBase = (fechaBase - origen).TotalDays;

            prediccion.FechaBase = fechaBase;
            prediccion.ValorActual = historial[historial.Count - 1].Valor;

            // Un punto cada "paso" dias, sin pasar el maximo que se grafica bien
            int paso = Math.Max(pasoMinimo, (int)Math.Ceiling(horizonteDias / (double)MaximoPuntosProyeccion));

            for (int dias = paso; dias <= horizonteDias; dias += paso)
            {
                prediccion.Proyeccion.Add(PuntoProyectado(fechaBase.AddDays(dias), recta.Predecir(xBase + dias)));
            }

            // El horizonte se cubre entero aunque no sea multiplo del paso
            if (prediccion.Proyeccion.Count == 0 || prediccion.Proyeccion[prediccion.Proyeccion.Count - 1].Fecha < fechaBase.AddDays(horizonteDias))
            {
                prediccion.Proyeccion.Add(PuntoProyectado(fechaBase.AddDays(horizonteDias), recta.Predecir(xBase + horizonteDias)));
            }

            prediccion.FechaFin = prediccion.Proyeccion[prediccion.Proyeccion.Count - 1].Fecha;
            prediccion.ValorPredicho = prediccion.Proyeccion[prediccion.Proyeccion.Count - 1].Valor;

            // Solo tiene sentido con una recta que baja: es el dia en que cruza el cero
            if (recta.Pendiente < 0)
            {
                double xCero = -recta.Ordenada / recta.Pendiente;

                if (xCero > xBase)
                {
                    prediccion.FechaQuiebre = origen.AddDays(Math.Ceiling(xCero));
                }
            }

            return prediccion;
        }

        private BEPrediccion Completar(BEPrediccion prediccion, BEProducto producto, int horizonteDias)
        {
            prediccion.IdProducto = producto.IdProducto;
            prediccion.NombreProducto = producto.Nombre;
            prediccion.HorizonteDias = horizonteDias;

            prediccion.VariacionPorcentaje = prediccion.ValorActual != 0
                ? Math.Round((prediccion.ValorPredicho - prediccion.ValorActual) / prediccion.ValorActual * 100, 2)
                : 0;

            prediccion.Tendencia = CalcularTendencia(prediccion.VariacionPorcentaje);
            prediccion.Confianza = CalcularConfianza(prediccion.R2);

            // La fecha de quiebre solo se informa en stock: en precio o demanda
            // que la recta cruce el cero no significa nada
            if (prediccion.Tipo != BEPrediccion.TipoStock)
            {
                prediccion.FechaQuiebre = null;
            }

            return prediccion;
        }

        #endregion

        #region Ayudas

        
        /// Suma las unidades por semana (de lunes a domingo) y rellena con
        /// cero las semanas sin ventas entre la primera y la última, para que
        /// la recta vea la caída real y no solo los días con movimiento.
        
        public List<BEPuntoPrediccion> AgruparPorSemana(List<KeyValuePair<DateTime, int>> unidadesPorDia)
        {
            List<BEPuntoPrediccion> semanas = new List<BEPuntoPrediccion>();

            if (unidadesPorDia.Count == 0)
            {
                return semanas;
            }

            Dictionary<DateTime, int> porSemana = new Dictionary<DateTime, int>();

            foreach (KeyValuePair<DateTime, int> dia in unidadesPorDia)
            {
                DateTime lunes = InicioDeSemana(dia.Key);

                if (!porSemana.ContainsKey(lunes))
                {
                    porSemana[lunes] = 0;
                }

                porSemana[lunes] = porSemana[lunes] + dia.Value;
            }

            DateTime primera = InicioDeSemana(unidadesPorDia[0].Key);
            DateTime ultima = InicioDeSemana(unidadesPorDia[unidadesPorDia.Count - 1].Key);

            for (DateTime semana = primera; semana <= ultima; semana = semana.AddDays(7))
            {
                semanas.Add(Punto(semana, porSemana.ContainsKey(semana) ? porSemana[semana] : 0));
            }

            return semanas;
        }

        private DateTime InicioDeSemana(DateTime fecha)
        {
            int desdeLunes = ((int)fecha.DayOfWeek + 6) % 7;

            return fecha.Date.AddDays(-desdeLunes);
        }

        private string CalcularTendencia(decimal variacion)
        {
            if (variacion > 2)
            {
                return "En alza";
            }

            return variacion < -2 ? "En baja" : "Estable";
        }

        // Umbrales usuales para leer el R2 de una regresion simple
        private string CalcularConfianza(double r2)
        {
            if (r2 >= 0.8)
            {
                return "Alta";
            }

            return r2 >= 0.5 ? "Media" : "Baja";
        }

        private BEPuntoPrediccion Punto(DateTime fecha, decimal valor)
        {
            BEPuntoPrediccion punto = new BEPuntoPrediccion();

            punto.Fecha = fecha;
            punto.Valor = valor;
            punto.EsProyeccion = false;

            return punto;
        }

        // Precio, stock y unidades no pueden ser negativos: la recta se corta en cero
        private BEPuntoPrediccion PuntoProyectado(DateTime fecha, double valor)
        {
            BEPuntoPrediccion punto = new BEPuntoPrediccion();

            punto.Fecha = fecha;
            punto.Valor = Math.Round((decimal)Math.Max(0, valor), 2);
            punto.EsProyeccion = true;

            return punto;
        }

        #endregion
    }
}
