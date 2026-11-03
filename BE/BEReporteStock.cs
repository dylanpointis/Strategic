namespace BE
{
    // Fila del reporte de stock (CU-002-008): stock actual del producto y
    // unidades vendidas en el periodo del reporte
    public class BEReporteStock
    {
        public const string SituacionSinStock = "SinStock";
        public const string SituacionBajoMinimo = "BajoMinimo";
        public const string SituacionSobreMaximo = "SobreMaximo";
        public const string SituacionNormal = "Normal";

        public int IdProducto { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Categoria { get; set; }
        public int Stock { get; set; }
        public int? StockMinimo { get; set; }
        public int? StockMaximo { get; set; }
        public int UnidadesVendidas { get; set; }

        public bool BajoMinimo
        {
            get { return StockMinimo.HasValue && Stock <= StockMinimo.Value; }
        }

        // Codigo de la situacion, igual en todos los idiomas: es el que va al XML
        public string Situacion
        {
            get
            {
                if (Stock <= 0)
                {
                    return SituacionSinStock;
                }

                if (BajoMinimo)
                {
                    return SituacionBajoMinimo;
                }

                return StockMaximo.HasValue && Stock > StockMaximo.Value ? SituacionSobreMaximo : SituacionNormal;
            }
        }

        // Clave de traduccion de la situacion, para mostrarla en pantalla
        public string SituacionTexto
        {
            get { return "reportes.situacion." + Situacion; }
        }
    }
}
