using BE.Composite;
using System.Collections.Generic;

namespace BE
{
    
    /// Rol de usuario. Es el cliente del Composite: agrupa componentes sin
    /// distinguir si son permisos simples o familias, porque los trata a todos
    /// a través de BEComponente.
    ///
    /// Los componentes asignados viven en una BEFamilia sin persistir, que
    /// hace de raíz del árbol del rol. Así el rol hereda gratis las
    /// operaciones recursivas: aplanar el árbol y buscar un permiso adentro.
    
    public class BERol
    {
        public int CodRol { get; set; }
        public string Nombre { get; set; }
        public bool Activo { get; set; }

        
        /// Raíz del árbol de permisos del rol. Sus hijos directos son los
        /// componentes que figuran en Rol_Permiso.
        
        public BEFamilia Componentes { get; private set; }

        public BERol()
        {
            Activo = true;
            Componentes = new BEFamilia();
        }

        public BERol(int codRol, string nombre) : this()
        {
            CodRol = codRol;
            Nombre = nombre;
        }

        
        /// Cantidad de permisos y familias asignados en forma directa, que es
        /// lo que muestra el listado de roles.
        
        public int CantidadComponentes
        {
            get { return Componentes.ObtenerHijos().Count; }
        }

        
        /// Cantidad de componentes asignados segun la consulta del listado,
        /// que no carga el arbol. Se usa en la grilla del CU-005-021.
        
        public int CantidadComponentesAsignados { get; set; }

        
        /// Usuarios activos que tienen el rol asignado. El CU-005-023 no deja
        /// dar de baja un rol que este en uso.
        
        public int UsuariosActivos { get; set; }

        // Claves de traducción del estado y del botón que lo cambia
        public string EstadoTexto
        {
            get { return Activo ? "comun.estado.activo" : "comun.estado.inactivo"; }
        }

        public string AccionEstado
        {
            get { return Activo ? "comun.accion.darDeBaja" : "comun.accion.reactivar"; }
        }

        
        /// Permisos simples que el rol termina habilitando, ya sea porque se
        /// le asignaron sueltos o porque llegan a través de una familia.
        
        public List<BEPermiso> ObtenerPermisosSimples()
        {
            return Componentes.ObtenerPermisosSimples();
        }

        
        /// Indica si el rol habilita un permiso simple por su nombre. Es la
        /// consulta que va a hacer el menú para mostrar cada pantalla.
        
        public bool TienePermiso(string nombrePermiso)
        {
            foreach (BEPermiso permiso in ObtenerPermisosSimples())
            {
                if (string.Equals(permiso.Nombre, nombrePermiso, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
