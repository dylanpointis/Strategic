using System;
using System.Collections.Generic;

namespace BE.Composite
{
    /// <summary>
    /// Leaf del patrón Composite: un permiso simple, la unidad atómica del
    /// árbol. Representa una acción concreta del sistema, que en Strategic
    /// es el acceso a una pantalla: el nombre del permiso coincide con el
    /// nombre del aspx.
    ///
    /// Los permisos simples vienen precargados en el script de la base y no
    /// se crean ni se modifican desde la aplicación, por eso no tienen ABM.
    /// </summary>
    public class BEPermiso : BEComponente
    {
        public const string TipoSimple = "Simple";

        public BEPermiso()
        {
            Tipo = TipoSimple;
            Activo = true;
        }

        public BEPermiso(int codPermiso, string nombre) : this()
        {
            CodPermiso = codPermiso;
            Nombre = nombre;
        }

        public override bool EsFamilia
        {
            get { return false; }
        }

        // Una hoja no admite hijos: el intento es un error de programacion,
        // no un caso de uso que haya que contemplar
        public override void AgregarHijo(BEComponente hijo)
        {
            throw new InvalidOperationException("Un permiso simple no puede contener otros permisos");
        }

        public override void QuitarHijo(BEComponente hijo)
        {
            throw new InvalidOperationException("Un permiso simple no contiene otros permisos");
        }

        public override List<BEComponente> ObtenerHijos()
        {
            return new List<BEComponente>();
        }

        public override bool Contiene(int codPermiso)
        {
            return CodPermiso == codPermiso;
        }

        public override List<BEPermiso> ObtenerPermisosSimples()
        {
            List<BEPermiso> permisos = new List<BEPermiso>();

            permisos.Add(this);

            return permisos;
        }
    }
}
