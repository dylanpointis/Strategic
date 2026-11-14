using BLL;
using System;
using System.Diagnostics;
using System.Threading;

namespace Strategic
{
    // Consulta el precio de las publicaciones de la competencia cada cierto
    // tiempo. Arranca con la aplicacion (Global.asax) y corre solo mientras la
    // aplicacion este levantada
    public static class SchedulerMonitoreo
    {
        // Cada cuantos minutos se consultan. 60 = una vez por hora
        private const int IntervaloMinutos = 60;

        // Queda en un campo para que el recolector de basura no lo elimine
        private static Timer timer;

        public static void Iniciar()
        {
            timer = new Timer(Ejecutar, null, TimeSpan.FromMinutes(IntervaloMinutos), Timeout.InfiniteTimeSpan);
        }

        private static void Ejecutar(object estado)
        {
            try
            {
                // Corre sin usuario: si los datos fueron alterados no se recalcula
                // el DV, y con la suscripcion vencida el sistema no se usa
                if (new BLLDigitoVerificador().CompararDV().Count == 0 && new BLLSuscripcion().TieneSuscripcionActiva())
                {
                    new BLLCompetencia().MonitorearPublicaciones(null);
                }
            }
            catch (Exception ex)
            {
                // Una excepcion sin atrapar en este hilo tiraria toda la aplicacion
                Trace.TraceError(ex.ToString());
            }

            // El proximo turno se programa al terminar para que nunca se pisen dos ejecuciones
            timer.Change(TimeSpan.FromMinutes(IntervaloMinutos), Timeout.InfiniteTimeSpan);
        }
    }
}
