using BE;
using Services;
using System;

namespace BLL
{
    // Context del patron State: guarda el estado actual de la publicacion y
    // le delega cada operacion. El estado es el que decide el estado siguiente
    public class PublicacionMonitoreada
    {
        private EstadoPublicacion estado;

        public BEPublicacionCompetencia Datos { get; private set; }

        public PublicacionMonitoreada(BEPublicacionCompetencia datos)
        {
            Datos = datos;
            estado = CrearEstado(datos.Estado);
        }

        public string Estado
        {
            get { return estado.Nombre; }
        }

        public void SetEstado(EstadoPublicacion nuevoEstado)
        {
            estado = nuevoEstado;
        }

        public bool Monitorear()
        {
            return estado.Monitorear(this);
        }

        public void AplicarAccion(string accion)
        {
            switch (accion)
            {
                case "Pausar":
                    estado.Pausar(this);
                    break;
                case "Reactivar":
                    estado.Reactivar(this);
                    break;
                case "Desactivar":
                    estado.Desactivar(this);
                    break;
                default:
                    throw new Exception(Traductor.T("error.accionNoReconocida"));
            }
        }

        // NoEncontrada y Error se comportan como Pausada: el monitoreo esta
        // detenido y la publicacion se puede reactivar o desactivar
        private static EstadoPublicacion CrearEstado(string nombre)
        {
            switch (nombre)
            {
                case "Activa":
                    return new PublicacionActiva();
                case "Finalizada":
                    return new PublicacionFinalizada();
                default:
                    return new PublicacionPausada();
            }
        }
    }
}
