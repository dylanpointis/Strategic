using System;

namespace Services
{
    // Simula la consulta del precio de una publicacion de la competencia. No
    // entra al marketplace: parte del ultimo precio y lo varia hasta un 5%.
    // Conectarse a la API del marketplace solo cambiaria esta clase
    public class ConsultorPreciosSimulado
    {
        private const double VariacionMaxima = 0.05;

        private static readonly Random azar = new Random();

        public decimal ConsultarPrecio(string url, decimal ultimoPrecio)
        {
            double variacion;

            lock (azar)
            {
                variacion = azar.NextDouble() * VariacionMaxima * 2 - VariacionMaxima;
            }

            return Math.Round(ultimoPrecio * (decimal)(1 + variacion), 2);
        }
    }
}
