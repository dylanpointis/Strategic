namespace BE
{
    // DVH de un registro. La clave son los valores de la clave primaria
    // separados por "|", para poder volver a encontrar la fila
    public class BEDigitoVerificadorFila
    {
        public string Tabla { get; set; }
        public string Clave { get; set; }
        public string DVH { get; set; }

        public BEDigitoVerificadorFila(string tabla, string clave, string dvh)
        {
            Tabla = tabla;
            Clave = clave;
            DVH = dvh;
        }
    }
}
