using System.Collections.Generic;

namespace BE
{
    public class BERol
    {
        public int CodRol { get; set; }
        public string Nombre { get; set; }
        public List<BEPermiso> Permisos { get; set; }

        public BERol()
        {
            Permisos = new List<BEPermiso>();
        }

        public BERol(int codRol, string nombre) : this()
        {
            CodRol = codRol;
            Nombre = nombre;
        }
    }
}
