namespace BE
{
    // DVV de una columna: hash de todos los valores de esa columna
    public class BEDigitoVerificadorColumna
    {
        public string Tabla { get; set; }
        public string Columna { get; set; }
        public string DVV { get; set; }

        public BEDigitoVerificadorColumna(string tabla, string columna, string dvv)
        {
            Tabla = tabla;
            Columna = columna;
            DVV = dvv;
        }
    }
}
