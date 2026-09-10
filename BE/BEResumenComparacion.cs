namespace BE
{
    /// <summary>
    /// Indicadores de la comparación de precios que se está viendo en pantalla.
    /// </summary>
    public class BEResumenComparacion
    {
        public int PublicacionesMonitoreadas { get; set; }
        public int ProductosMasCaros { get; set; }
        public int ProductosMasBaratos { get; set; }
        public int SinPrecioRegistrado { get; set; }

        /// <summary>
        /// Promedio de las diferencias porcentuales de las publicaciones que
        /// tienen precio registrado.
        /// </summary>
        public decimal DiferenciaPromedio { get; set; }
    }
}
