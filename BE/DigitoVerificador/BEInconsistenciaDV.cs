namespace BE
{
    // Diferencia encontrada al comparar el DV guardado con el calculado.
    // Clave y Columna pueden quedar vacias cuando no se pudo llegar a ese
    // nivel de detalle
    public class BEInconsistenciaDV
    {
        public string Tabla { get; set; }
        public string Clave { get; set; }
        public string Columna { get; set; }
        public string Detalle { get; set; }

        public BEInconsistenciaDV(string tabla, string clave, string columna, string detalle)
        {
            Tabla = tabla;
            Clave = clave;
            Columna = columna;
            Detalle = detalle;
        }
    }
}
