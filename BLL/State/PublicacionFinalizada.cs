using Services;
using System;

namespace BLL
{
    // Estado final: no se monitorea ni admite ninguna accion
    public class PublicacionFinalizada : EstadoPublicacion
    {
        public override string Nombre
        {
            get { return "Finalizada"; }
        }

        public override void Reactivar(PublicacionMonitoreada publicacion)
        {
            throw new Exception(Traductor.T("error.noReactivarDesactivada"));
        }

        public override void Desactivar(PublicacionMonitoreada publicacion)
        {
            throw new Exception(Traductor.T("error.publicacionYaDesactivada"));
        }
    }
}
