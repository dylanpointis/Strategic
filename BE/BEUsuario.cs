namespace BE
{
    public class BEUsuario
    {
        public string NombreUsuario { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Clave { get; set; }
        // Sal con la que se derivo la clave (PBKDF2); viaja en base64
        public string Salt { get; set; }
        public int CodRol { get; set; }
        public bool Bloqueado { get; set; }
        public bool Activo { get; set; }
        public int ContFallidos { get; set; }
        public BERol Rol { get; set; }

        
        /// Nombre del rol en una propiedad plana: la grilla enlaza por nombre
        /// de campo y no puede navegar hasta Rol.Nombre.
        
        public string NombreRol
        {
            get { return Rol != null ? Rol.Nombre : string.Empty; }
        }

        
        /// Estado del usuario tal como se muestra en el listado. Un usuario
        /// bloqueado sigue estando activo, pero no puede iniciar sesión, así
        /// que conviene distinguirlo. Devuelve la clave de traducción.
        
        public string EstadoTexto
        {
            get
            {
                if (!Activo)
                {
                    return "comun.estado.inactivo";
                }

                return Bloqueado ? "comun.estado.bloqueado" : "comun.estado.activo";
            }
        }

        
        /// Clave de traducción del botón que cambia el estado del usuario en el listado.
        
        public string AccionEstado
        {
            get { return Activo ? "comun.accion.darDeBaja" : "comun.accion.reactivar"; }
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
