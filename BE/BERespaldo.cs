using System;

namespace BE
{
    // Archivo .bak generado por el sistema
    public class BERespaldo
    {
        public string NombreArchivo { get; set; }
        public string Ruta { get; set; }
        public DateTime Fecha { get; set; }
        public long TamanioBytes { get; set; }

        // Tamanio legible para mostrar en pantalla
        public string TamanioTexto
        {
            get
            {
                if (TamanioBytes >= 1024 * 1024)
                {
                    return string.Format("{0:N1} MB", TamanioBytes / (1024.0 * 1024.0));
                }

                if (TamanioBytes >= 1024)
                {
                    return string.Format("{0:N0} KB", TamanioBytes / 1024.0);
                }

                return string.Format("{0} B", TamanioBytes);
            }
        }
    }
}
