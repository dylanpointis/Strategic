namespace BLL
{
    // El monitoreo esta detenido: no se consulta su precio y se puede reactivar
    // o desactivar. Usa el comportamiento comun de EstadoPublicacion, sin cambios
    public class PublicacionPausada : EstadoPublicacion
    {
        public override string Nombre
        {
            get { return "Pausada"; }
        }
    }
}
