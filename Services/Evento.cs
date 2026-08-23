namespace Services
{
    public class Evento
    {
        public long CodEvento { get; set; }
        public string NombreUsuario { get; set; }
        public string Modulo { get; set; }
        public string Descripcion { get; set; }
        public int Criticidad { get; set; }
        public string Fecha { get; set; }
        public string Hora { get; set; }

        public Evento(string nombreUsuario, string modulo, string descripcion, int criticidad)
        {
            NombreUsuario = nombreUsuario;
            Modulo = modulo;
            Descripcion = descripcion;
            Criticidad = criticidad;
        }
    }
}
