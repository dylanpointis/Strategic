using System;

namespace Services.Prediccion
{
    
    /// Recta ajustada por la regresión lineal, en las unidades originales de
    /// la serie: y = Ordenada + Pendiente * x.
    
    public class ResultadoRegresion
    {
        //Cuánto cambia el valor por cada unidad de x (por día, si x son días).
        public double Pendiente { get; set; }

        //Valor de la recta en x = 0.
        public double Ordenada { get; set; }

        
        /// Coeficiente de determinación: qué parte de la variación de los datos
        /// explica la recta. Va de 0 (nada) a 1 (los puntos están sobre la recta).
        
        public double R2 { get; set; }

        //Error medio absoluto entre los datos y la recta.
        public double ErrorMedioAbsoluto { get; set; }

        public int CantidadPuntos { get; set; }

        public double Predecir(double x)
        {
            return Ordenada + Pendiente * x;
        }
    }
}
