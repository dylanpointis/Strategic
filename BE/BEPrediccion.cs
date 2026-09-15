using System;
using System.Collections.Generic;

namespace BE
{
    
    /// Un punto de la serie de una predicción: dato histórico o proyectado.
    
    public class BEPuntoPrediccion
    {
        public DateTime Fecha { get; set; }
        public decimal Valor { get; set; }
        public bool EsProyeccion { get; set; }
    }

    
    /// Predicción generada en el momento por regresión lineal sobre el
    /// histórico de un producto. No se persiste: cada consulta la recalcula
    /// con los datos que haya en ese instante.
    
    public class BEPrediccion
    {
        public const string TipoPrecio = "Precio";
        public const string TipoStock = "Stock";
        public const string TipoDemanda = "Demanda";

        public string Tipo { get; set; }
        public string NombreTipo { get; set; }

        //Prefijo para mostrar los valores: "$" en precio, vacío en el resto.
        public string Prefijo { get; set; }

        //Texto de la unidad: "por día", "unidades por semana".
        public string Unidad { get; set; }

        public int IdProducto { get; set; }
        public string NombreProducto { get; set; }
        public int HorizonteDias { get; set; }

        //Fecha del último dato real, desde donde arranca la proyección.
        public DateTime FechaBase { get; set; }
        public DateTime FechaFin { get; set; }

        public decimal ValorActual { get; set; }
        public decimal ValorPredicho { get; set; }
        public decimal VariacionPorcentaje { get; set; }

        //Cuánto cambia el valor por cada día (o semana, en demanda).
        public decimal Pendiente { get; set; }

        //Coeficiente de determinación del ajuste, entre 0 y 1.
        public double R2 { get; set; }
        public decimal ErrorMedio { get; set; }
        public int CantidadPuntos { get; set; }

        public string Tendencia { get; set; }
        public string Confianza { get; set; }

        //Solo para stock: fecha estimada en que llega a cero, si la recta baja.
        public DateTime? FechaQuiebre { get; set; }

        public List<BEPuntoPrediccion> Historial { get; set; }
        public List<BEPuntoPrediccion> Proyeccion { get; set; }

        public BEPrediccion()
        {
            Historial = new List<BEPuntoPrediccion>();
            Proyeccion = new List<BEPuntoPrediccion>();
        }
    }
}
