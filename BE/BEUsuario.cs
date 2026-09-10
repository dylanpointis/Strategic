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

        /// <summary>
        /// Nombre del rol en una propiedad plana: la grilla enlaza por nombre
        /// de campo y no puede navegar hasta Rol.Nombre.
        /// </summary>
        public string NombreRol
        {
            get { return Rol != null ? Rol.Nombre : string.Empty; }
        }

        /// <summary>
        /// Estado del usuario tal como se muestra en el listado. Un usuario
        /// bloqueado sigue estando activo, pero no puede iniciar sesión, así
        /// que conviene distinguirlo.
        /// </summary>
        public string EstadoTexto
        {
            get
            {
                if (!Activo)
                {
                    return "Inactivo";
                }

                return Bloqueado ? "Bloqueado" : "Activo";
            }
        }

        /// <summary>
        /// Texto del botón que cambia el estado del usuario en el listado.
        /// </summary>
        public string AccionEstado
        {
            get { return Activo ? "Dar de baja" : "Reactivar"; }
        }

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
