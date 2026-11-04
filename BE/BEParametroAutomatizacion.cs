namespace BE
{
    // Parametro modificable de una automatizacion, con su rango permitido
    public class BEParametroAutomatizacion
    {
        public string Clave { get; set; }
        public int Valor { get; set; }
        public int Minimo { get; set; }
        public int Maximo { get; set; }
    }
}
