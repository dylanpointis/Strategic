using Microsoft.ML;
using Microsoft.ML.Trainers;
using System;
using System.Collections.Generic;

namespace Services.Prediccion
{
    
    /// Regresión lineal simple sobre una serie (x, y) usando ML.NET.
    ///
    /// Es la operación que comparten todas las predicciones: precio, stock y
    /// demanda son series distintas, pero la recta se ajusta igual. Los datos
    /// se escalan a [0, 1] antes de entrenar -el descenso por gradiente no
    /// converge bien con precios de seis cifras- y la recta se devuelve ya
    /// convertida a las unidades originales.
    
    public static class RegresionLineal
    {
        public const int MinimoPuntos = 3;

        // Pasadas del descenso por gradiente. Con los datos escalados y una
        // sola variable, alcanzan para llegar a la recta de minimos cuadrados.
        private const int Iteraciones = 6000;
        private const float TasaDeAprendizaje = 0.05f;

        
        /// Fila que consume ML.NET. Tiene que ser pública y con float.
        
        public class PuntoRegresion
        {
            public float X { get; set; }
            public float Y { get; set; }
        }

        public static ResultadoRegresion Ajustar(IList<double> xs, IList<double> ys)
        {
            Validar(xs, ys);

            CargadorNativo.AsegurarCpuMath();

            double xMin, xMax, yMin, yMax;
            Rango(xs, out xMin, out xMax);
            Rango(ys, out yMin, out yMax);

            double anchoX = xMax - xMin;
            double anchoY = yMax > yMin ? yMax - yMin : 1;

            List<PuntoRegresion> puntos = new List<PuntoRegresion>();

            for (int i = 0; i < xs.Count; i++)
            {
                puntos.Add(new PuntoRegresion
                {
                    X = (float)((xs[i] - xMin) / anchoX),
                    Y = (float)((ys[i] - yMin) / anchoY)
                });
            }

            // La semilla fija hace que la misma serie de siempre la misma recta
            MLContext contexto = new MLContext(seed: 1);
            IDataView datos = contexto.Data.LoadFromEnumerable(puntos);

            OnlineGradientDescentTrainer.Options opciones = new OnlineGradientDescentTrainer.Options();
            opciones.LabelColumnName = "Y";
            opciones.FeatureColumnName = "Features";
            opciones.LearningRate = TasaDeAprendizaje;
            opciones.NumberOfIterations = Iteraciones;
            // Tasa fija y chica: la que decrece con las pasadas se apaga antes
            // de llegar a la recta
            opciones.DecreaseLearningRate = false;
            opciones.L2Regularization = 0f;
            opciones.Shuffle = false;

            // Sin promediar los pesos: el promedio arrastra las primeras pasadas,
            // que todavia estan lejos, y la recta queda corrida
            opciones.Averaged = false;

            // El cache evita rearmar la columna Features en cada una de las
            // miles de pasadas: baja el tiempo de entrenamiento de segundos a
            // decimas de segundo
            var pipeline = contexto.Transforms.Concatenate("Features", "X")
                .AppendCacheCheckpoint(contexto)
                .Append(contexto.Regression.Trainers.OnlineGradientDescent(opciones));

            var modelo = pipeline.Fit(datos);
            LinearRegressionModelParameters recta = modelo.LastTransformer.Model;

            // De la recta escalada a la recta en unidades originales
            double pendienteEscalada = recta.Weights[0];
            double ordenadaEscalada = recta.Bias;

            ResultadoRegresion resultado = new ResultadoRegresion();
            resultado.Pendiente = pendienteEscalada * anchoY / anchoX;
            resultado.Ordenada = yMin + ordenadaEscalada * anchoY - resultado.Pendiente * xMin;
            resultado.CantidadPuntos = xs.Count;

            CalcularBondad(xs, ys, resultado);

            return resultado;
        }

        private static void Validar(IList<double> xs, IList<double> ys)
        {
            if (xs == null || ys == null || xs.Count != ys.Count)
            {
                throw new ArgumentException("Las series de x e y tienen que tener la misma cantidad de puntos");
            }

            if (xs.Count < MinimoPuntos)
            {
                throw new ArgumentException(string.Format("Hacen falta al menos {0} puntos para ajustar una recta", MinimoPuntos));
            }

            double xMin, xMax;
            Rango(xs, out xMin, out xMax);

            if (xMax <= xMin)
            {
                throw new ArgumentException("Todos los puntos estan en la misma fecha: no hay una evolucion que proyectar");
            }
        }

        private static void Rango(IList<double> valores, out double minimo, out double maximo)
        {
            minimo = double.MaxValue;
            maximo = double.MinValue;

            foreach (double valor in valores)
            {
                if (valor < minimo) minimo = valor;
                if (valor > maximo) maximo = valor;
            }
        }

        // R2 y error medio, calculados en las unidades originales sobre la
        // recta ya convertida, que es la que se le muestra al usuario
        private static void CalcularBondad(IList<double> xs, IList<double> ys, ResultadoRegresion resultado)
        {
            double media = 0;

            foreach (double y in ys)
            {
                media += y;
            }

            media /= ys.Count;

            double sumaResiduos = 0;
            double sumaTotal = 0;
            double sumaAbsoluta = 0;

            for (int i = 0; i < xs.Count; i++)
            {
                double estimado = resultado.Predecir(xs[i]);
                double residuo = ys[i] - estimado;

                sumaResiduos += residuo * residuo;
                sumaTotal += (ys[i] - media) * (ys[i] - media);
                sumaAbsoluta += Math.Abs(residuo);
            }

            // Si todos los y son iguales no hay variacion que explicar
            resultado.R2 = sumaTotal > 0 ? Math.Max(0, 1 - sumaResiduos / sumaTotal) : 1;
            resultado.ErrorMedioAbsoluto = sumaAbsoluta / xs.Count;
        }
    }
}
