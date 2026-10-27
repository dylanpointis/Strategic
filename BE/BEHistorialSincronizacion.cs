using System;
using System.Collections.Generic;

namespace BE
{
    // Una ejecucion de importacion registrada en el historial de sincronizaciones
    public class BEHistorialSincronizacion
    {
        public int IdSincronizacion { get; set; }
        public int IdConfiguracion { get; set; }
        public string TipoArchivo { get; set; }
        public int RegistrosImportados { get; set; }
        public int RegistrosActualizados { get; set; }
        public int RegistrosError { get; set; }
        public string Estado { get; set; }
        public string Detalle { get; set; }
        public DateTime FechaEjecucion { get; set; }

        public int RegistrosProcesados
        {
            get { return RegistrosImportados + RegistrosActualizados + RegistrosError; }
        }

        // Claves de traduccion del tipo y del estado, para mostrarlos en pantalla
        public string TipoTexto
        {
            get { return "integracion.tipo." + TipoArchivo; }
        }

        public string EstadoTexto
        {
            get { return "integracion.estado." + Estado; }
        }

        // El detalle se guarda con un error por linea
        public List<string> LineasDetalle
        {
            get
            {
                List<string> lineas = new List<string>();

                if (string.IsNullOrEmpty(Detalle))
                {
                    return lineas;
                }

                foreach (string linea in Detalle.Split('\n'))
                {
                    if (linea.Trim().Length > 0)
                    {
                        lineas.Add(linea.Trim());
                    }
                }

                return lineas;
            }
        }
    }
}
