namespace BE
{
    // Correspondencia entre un campo de Strategic y una columna del archivo del cliente
    public class BEMapeoColumna
    {
        public int IdMapeo { get; set; }
        public string ColumnaStrategic { get; set; }
        public string ColumnaCliente { get; set; }

        // Clave de traduccion del nombre del campo, para mostrarlo en pantalla
        public string CampoTexto
        {
            get { return "integracion.campo." + ColumnaStrategic; }
        }
    }
}
