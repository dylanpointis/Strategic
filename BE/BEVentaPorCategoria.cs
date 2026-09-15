namespace BE
{
    
    /// Unidades y monto vendidos agrupados por categoría de producto.
    
    public class BEVentaPorCategoria
    {
        public string Categoria { get; set; }
        public int Unidades { get; set; }
        public decimal Monto { get; set; }
    }
}
