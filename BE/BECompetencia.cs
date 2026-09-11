namespace BE
{
    /// <summary>
    /// Competidor registrado en el sistema. Cada competidor puede tener
    /// varias publicaciones asociadas a productos propios.
    /// </summary>
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

        /// <summary>
        /// Texto del botón que cambia el estado del competidor en el listado.
        /// </summary>
        public string AccionEstado
        {
            get { return Activo ? "Dar de baja" : "Reactivar"; }
        }
    }
}
