using System;
using System.Collections.Generic;

namespace BE
{
    // Configuracion de integracion de un tipo de archivo (Productos, Ventas o
    // Stock): que columna del archivo del cliente corresponde a cada campo
    public class BEConfiguracionIntegracion
    {
        public int IdConfiguracion { get; set; }
        public string TipoArchivo { get; set; }
        public DateTime FechaConfiguracion { get; set; }
        public List<BEMapeoColumna> Mapeos { get; set; }

        public BEConfiguracionIntegracion()
        {
            Mapeos = new List<BEMapeoColumna>();
        }

        // Columna del cliente asignada a un campo de Strategic, o null si no se mapeo
        public string ColumnaDe(string columnaStrategic)
        {
            foreach (BEMapeoColumna mapeo in Mapeos)
            {
                if (mapeo.ColumnaStrategic == columnaStrategic)
                {
                    return mapeo.ColumnaCliente;
                }
            }

            return null;
        }
    }
}
