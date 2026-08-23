using System.Collections.Generic;

namespace BE
{
    public class BEPermiso
    {
        public int CodPermiso { get; set; }
        public string Nombre { get; set; }
        public string Tipo { get; set; }
        public List<BEPermiso> Hijos { get; set; }

        public BEPermiso()
        {
            Hijos = new List<BEPermiso>();
        }

        public bool EsFamilia
        {
            get { return Tipo == "Familia"; }
        }
    }
}
