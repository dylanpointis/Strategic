namespace BE
{
    public class BEUsuario
    {
        public string NombreUsuario { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Clave { get; set; }
        public int CodRol { get; set; }
        public bool Bloqueado { get; set; }
        public bool Activo { get; set; }
        public int ContFallidos { get; set; }
        public BERol Rol { get; set; }

        public BEUsuario()
        {
            Rol = new BERol();
        }

        public BEUsuario(string nombreUsuario, string nombre, string apellido, string email, string clave, int codRol, bool bloqueado, bool activo)
        {
            NombreUsuario = nombreUsuario;
            Nombre = nombre;
            Apellido = apellido;
            Email = email;
            Clave = clave;
            CodRol = codRol;
            Bloqueado = bloqueado;
            Activo = activo;
            Rol = new BERol();
        }
    }
}
