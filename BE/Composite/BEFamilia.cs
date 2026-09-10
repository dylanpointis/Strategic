using System.Collections.Generic;

namespace BE.Composite
{
    /// <summary>
    /// Composite del patrón: agrupa permisos simples y otras familias, con la
    /// profundidad que haga falta. Las familias sí se crean desde la
    /// aplicación, combinando piezas que ya existen (CU-005-026).
    /// </summary>
    public class BEFamilia : BEComponente
    {
        public const string TipoFamilia = "Familia";

        private readonly List<BEComponente> hijos = new List<BEComponente>();

        public BEFamilia()
        {
            Tipo = TipoFamilia;
            Activo = true;
        }

        public BEFamilia(int codPermiso, string nombre) : this()
        {
            CodPermiso = codPermiso;
            Nombre = nombre;
        }

        public override bool EsFamilia
        {
            get { return true; }
        }

        /// <summary>
        /// Cantidad de componentes que contiene en forma directa, tal como la
        /// devuelve la consulta del listado. El listado de familias no carga
        /// el arbol entero, solo necesita el numero.
        /// </summary>
        public int CantidadComponentesAsignados { get; set; }

        /// <summary>
        /// Agrega un componente. Si ya estaba en forma directa no se duplica.
        /// </summary>
        public override void AgregarHijo(BEComponente hijo)
        {
            if (hijo == null || TieneHijoDirecto(hijo.CodPermiso))
            {
                return;
            }

            hijos.Add(hijo);
        }

        public override void QuitarHijo(BEComponente hijo)
        {
            if (hijo == null)
            {
                return;
            }

            for (int i = hijos.Count - 1; i >= 0; i--)
            {
                if (hijos[i].CodPermiso == hijo.CodPermiso)
                {
                    hijos.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Devuelve una copia de la lista: los hijos se agregan y se quitan
        /// con los métodos de la clase, no manipulando la lista desde afuera.
        /// </summary>
        public override List<BEComponente> ObtenerHijos()
        {
            return new List<BEComponente>(hijos);
        }

        public void LimpiarHijos()
        {
            hijos.Clear();
        }

        public bool TieneHijoDirecto(int codPermiso)
        {
            foreach (BEComponente hijo in hijos)
            {
                if (hijo.CodPermiso == codPermiso)
                {
                    return true;
                }
            }

            return false;
        }

        public override bool Contiene(int codPermiso)
        {
            if (CodPermiso == codPermiso)
            {
                return true;
            }

            foreach (BEComponente hijo in hijos)
            {
                if (hijo.Contiene(codPermiso))
                {
                    return true;
                }
            }

            return false;
        }

        public override List<BEPermiso> ObtenerPermisosSimples()
        {
            List<BEPermiso> permisos = new List<BEPermiso>();

            foreach (BEComponente hijo in hijos)
            {
                foreach (BEPermiso permiso in hijo.ObtenerPermisosSimples())
                {
                    // Dos ramas distintas pueden llegar al mismo permiso simple
                    if (!ContieneCodigo(permisos, permiso.CodPermiso))
                    {
                        permisos.Add(permiso);
                    }
                }
            }

            return permisos;
        }

        private bool ContieneCodigo(List<BEPermiso> permisos, int codPermiso)
        {
            foreach (BEPermiso permiso in permisos)
            {
                if (permiso.CodPermiso == codPermiso)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
