using System.Collections.Generic;

namespace BE
{
    // Resumen de una importacion que se le muestra al usuario al terminar (CU-006-030)
    public class BEResultadoIntegracion
    {
        public const string EstadoExitosa = "Exitosa";
        public const string EstadoConErrores = "ConErrores";
        public const string EstadoFallida = "Fallida";

        public string TipoArchivo { get; set; }
        public string Estado { get; set; }
        public int RegistrosImportados { get; set; }
        public int RegistrosActualizados { get; set; }
        public int RegistrosError { get; set; }

        // Clave de traduccion del mensaje general del resultado
        public string Mensaje { get; set; }

        // Descripcion de cada registro omitido o del motivo de la falla
        public List<string> Errores { get; set; }

        public BEResultadoIntegracion()
        {
            Errores = new List<string>();
        }

        public int RegistrosProcesados
        {
            get { return RegistrosImportados + RegistrosActualizados + RegistrosError; }
        }

        public string EstadoTexto
        {
            get { return "integracion.estado." + Estado; }
        }
    }
}
