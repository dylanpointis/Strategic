namespace BE
{
    /// <summary>
    /// Unidades y monto vendidos agrupados por categoría de producto.
    /// </summary>
    public class BEVentaPorCategoria
    {
        public string Categoria { get; set; }
        public int Unidades { get; set; }
        public decimal Monto { get; set; }
    }
}
