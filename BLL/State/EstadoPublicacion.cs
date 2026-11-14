using Services;
using System;

namespace BLL
{
    // Patron State: cada estado de una publicacion monitoreada decide si se
    // monitorea, que acciones admite y a que estado pasa. Lo comun queda resuelto aca
    public abstract class EstadoPublicacion
    {
        public abstract string Nombre { get; }

        // Por defecto no se monitorea: devuelve si se consulto el precio
        public virtual bool Monitorear(PublicacionMonitoreada publicacion)
        {
            return false;
        }

        // Solo una publicacion activa se puede pausar
        public virtual void Pausar(PublicacionMonitoreada publicacion)
        {
            throw new Exception(Traductor.T("error.soloPausarActiva"));
        }

        public virtual void Reactivar(PublicacionMonitoreada publicacion)
        {
            publicacion.SetEstado(new PublicacionActiva());
        }

        public virtual void Desactivar(PublicacionMonitoreada publicacion)
        {
            publicacion.SetEstado(new PublicacionFinalizada());
        }
    }
}
