namespace BE
{
    /// <summary>
    /// Producto dentro del ranking de más vendidos del período.
    /// </summary>
    public class BETopProducto
    {
        public int IdProducto { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Categoria { get; set; }
        public int Unidades { get; set; }
        public decimal Monto { get; set; }
    }
}
