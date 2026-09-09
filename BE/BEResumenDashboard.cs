namespace BE
{
    /// <summary>
    /// Indicadores principales del dashboard para el período y la categoría filtrados.
    /// </summary>
    public class BEResumenDashboard
    {
        public decimal FacturacionTotal { get; set; }
        public int CantidadVentas { get; set; }
        public int UnidadesVendidas { get; set; }
    }
}
