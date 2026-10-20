using System;

namespace Services
{
    // Se lanza cuando no hay datos sincronizados para analizar. La pantalla
    // la distingue de un error comun para sugerir una sincronizacion
    public class DatosInsuficientesException : Exception
    {
        public DatosInsuficientesException(string mensaje)
            : base(mensaje)
        {
        }
    }
}
