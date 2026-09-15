using System.Collections.Generic;

namespace BE.Composite
{
    
    /// Component del patrón Composite: la abstracción común a todo lo que
    /// puede formar parte del árbol de permisos.
    ///
    /// La implementan dos clases:
    ///   - BEPermiso (Leaf): un permiso simple, una pantalla concreta.
    ///     Está predefinido en la base y no se crea desde la aplicación.
    ///   - BEFamilia (Composite): agrupa permisos simples y otras familias.
    ///
    /// Gracias a esta abstracción, quien consume el árbol -el rol, el menú-
    /// no necesita preguntar de qué tipo es cada elemento.
    ///
    /// En la base los dos son filas de la tabla Permiso y se distinguen por
    /// la columna Tipo; el árbol se arma con Permiso_Componente.
    
    public abstract class BEComponente
    {
        public int CodPermiso { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Tipo { get; set; }
        public bool Activo { get; set; }

        
        /// Indica si el componente puede contener otros. Evita tener que
        /// preguntar por el tipo concreto con "is" fuera de estas clases.
        
        public abstract bool EsFamilia { get; }

        public abstract void AgregarHijo(BEComponente hijo);

        public abstract void QuitarHijo(BEComponente hijo);

        
        /// Elementos que contiene en forma directa. Una hoja devuelve la lista vacía.
        
        public abstract List<BEComponente> ObtenerHijos();

        
        /// Recorre el árbol completo buscando un componente. Es la operación
        /// que usan la detección de referencias circulares y el control de
        /// permisos repetidos al armar un rol o una familia.
        
        public abstract bool Contiene(int codPermiso);

        
        /// Aplana el árbol y devuelve los permisos simples que quedan
        /// alcanzados, sin repetidos. Es lo que va a consultar el menú para
        /// saber qué pantallas habilitar.
        
        public abstract List<BEPermiso> ObtenerPermisosSimples();

        public string EstadoTexto
        {
            get { return Activo ? "Activo" : "Inactivo"; }
        }

        public string AccionEstado
        {
            get { return Activo ? "Dar de baja" : "Reactivar"; }
        }

        
        /// Cantidad de permisos simples alcanzados por el componente.
        
        public int CantidadPermisosSimples
        {
            get { return ObtenerPermisosSimples().Count; }
        }
    }
}
