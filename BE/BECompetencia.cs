namespace BE
{
    
    /// Competidor registrado en el sistema. Cada competidor puede tener
    /// varias publicaciones asociadas a productos propios.
    
    public class BECompetencia
    {
        public int IdCompetencia { get; set; }
        public string Nombre { get; set; }
        public string Marketplace { get; set; }
        public string Descripcion { get; set; }
        public string Estado { get; set; }

        public bool Activo
        {
            get { return Estado == "Activo"; }
        }


        /// Clave de traducción del estado, para mostrarlo en pantalla.

        public string EstadoTexto
        {
            get { return Activo ? "comun.estado.activo" : "comun.estado.inactivo"; }
        }

        
        /// Clave de traducción del botón que cambia el estado del competidor en el listado.
        
        public string AccionEstado
        {
            get { return Activo ? "comun.accion.darDeBaja" : "comun.accion.reactivar"; }
        }
    }
}
