namespace BE
{
    // Campo de Strategic que se puede mapear al importar un tipo de archivo
    public class BECampoIntegracion
    {
        public BECampoIntegracion(string nombre, bool obligatorio)
        {
            Nombre = nombre;
            Obligatorio = obligatorio;
        }

        public string Nombre { get; private set; }
        public bool Obligatorio { get; private set; }

        // Clave de traduccion del nombre del campo, para mostrarlo en pantalla
        public string CampoTexto
        {
            get { return "integracion.campo." + Nombre; }
        }
    }
}
