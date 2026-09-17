using System.Collections.Generic;

namespace BE
{
    // Digito verificador de una tabla: el DVH y DVV globales de siempre, mas
    // el DVH de cada fila y el DVV de cada columna, que son los que permiten
    // decir que registro y que campo no coinciden
    public class BEDigitoVerificador
    {
        public string Tabla { get; set; }
        public string DVH { get; set; }
        public string DVV { get; set; }
        public List<BEDigitoVerificadorFila> Filas { get; set; }
        public List<BEDigitoVerificadorColumna> Columnas { get; set; }

        // Una tabla recien creada no tiene DV guardado todavia
        public bool Calculado
        {
            get { return !string.IsNullOrEmpty(DVH) && !string.IsNullOrEmpty(DVV); }
        }

        public BEDigitoVerificador()
        {
            Filas = new List<BEDigitoVerificadorFila>();
            Columnas = new List<BEDigitoVerificadorColumna>();
        }

        public BEDigitoVerificador(string tabla, string dvh, string dvv) : this()
        {
            Tabla = tabla;
            DVH = dvh;
            DVV = dvv;
        }
    }
}
