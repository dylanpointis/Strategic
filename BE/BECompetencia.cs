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
    }
}
