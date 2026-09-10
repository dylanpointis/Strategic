using BE.Composite;
using System.Collections.Generic;

namespace BE
{
    /// <summary>
    /// Rol de usuario. Es el cliente del Composite: agrupa componentes sin
    /// distinguir si son permisos simples o familias, porque los trata a todos
    /// a través de BEComponente.
    ///
    /// Los componentes asignados viven en una BEFamilia sin persistir, que
    /// hace de raíz del árbol del rol. Así el rol hereda gratis las
    /// operaciones recursivas: aplanar el árbol y buscar un permiso adentro.
    /// </summary>
    public class BERol
    {
        public int CodRol { get; set; }
        public string Nombre { get; set; }
        public bool Activo { get; set; }

        /// <summary>
        /// Raíz del árbol de permisos del rol. Sus hijos directos son los
        /// componentes que figuran en Rol_Permiso.
        /// </summary>
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

        /// <summary>
        /// Cantidad de permisos y familias asignados en forma directa, que es
        /// lo que muestra el listado de roles.
        /// </summary>
        public int CantidadComponentes
        {
            get { return Componentes.ObtenerHijos().Count; }
        }

        /// <summary>
        /// Cantidad de componentes asignados segun la consulta del listado,
        /// que no carga el arbol. Se usa en la grilla del CU-005-021.
        /// </summary>
        public int CantidadComponentesAsignados { get; set; }

        /// <summary>
        /// Usuarios activos que tienen el rol asignado. El CU-005-023 no deja
        /// dar de baja un rol que este en uso.
        /// </summary>
        public int UsuariosActivos { get; set; }

        public string EstadoTexto
        {
            get { return Activo ? "Activo" : "Inactivo"; }
        }

        public string AccionEstado
        {
            get { return Activo ? "Dar de baja" : "Reactivar"; }
        }

        /// <summary>
        /// Permisos simples que el rol termina habilitando, ya sea porque se
        /// le asignaron sueltos o porque llegan a través de una familia.
        /// </summary>
        public List<BEPermiso> ObtenerPermisosSimples()
        {
            return Componentes.ObtenerPermisosSimples();
        }

        /// <summary>
        /// Indica si el rol habilita un permiso simple por su nombre. Es la
        /// consulta que va a hacer el menú para mostrar cada pantalla.
        /// </summary>
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
