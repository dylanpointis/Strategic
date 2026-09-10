using BE.Composite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    /// <summary>
    /// Acceso a los permisos y familias, y armado del árbol del Composite.
    ///
    /// El árbol se construye a partir de dos consultas -todos los nodos y
    /// todas las relaciones- y se ensambla en memoria. La alternativa sería
    /// una consulta por cada familia que se abre, que en un árbol de varios
    /// niveles se multiplica rápido.
    /// </summary>
    public class DALPermiso
    {
        private readonly DALConexion dalCon = new DALConexion();

        #region Consultas

        public List<BEComponente> TraerListaPermisos(string tipo, bool? activo)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@Tipo", tipo),
                new SqlParameter("@Activo", activo)
            };

            return MapearComponentes(dalCon.ConsultaProcAlmacenado("TraerListaPermisos", parametros));
        }

        public List<BEFamilia> FiltrarFamilias(string nombre, bool? activo)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@Nombre", nombre),
                new SqlParameter("@Activo", activo)
            };

            DataTable tabla = dalCon.ConsultaProcAlmacenado("FiltrarFamilias", parametros);
            List<BEFamilia> familias = new List<BEFamilia>();

            foreach (DataRow row in tabla.Rows)
            {
                BEFamilia familia = (BEFamilia)MapearComponente(row);

                // El listado del CU-005-025 solo necesita cuantos elementos
                // contiene, no hace falta traer el arbol entero
                familia.CantidadComponentesAsignados = Convert.ToInt32(row["CantidadComponentes"]);

                familias.Add(familia);
            }

            return familias;
        }

        public BEComponente TraerPermisoPorId(int codPermiso)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@CodPermiso", codPermiso)
            };

            List<BEComponente> componentes = MapearComponentes(
                dalCon.ConsultaProcAlmacenado("TraerPermisoPorId", parametros));

            return componentes.Count > 0 ? componentes[0] : null;
        }

        public List<BEComponente> TraerHijosDeFamilia(int codPadre)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@CodPadre", codPadre)
            };

            return MapearComponentes(dalCon.ConsultaProcAlmacenado("TraerHijosDeFamilia", parametros));
        }

        public int ContarPermisosConNombre(string nombre, int? codPermisoExcluido)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@Nombre", nombre),
                new SqlParameter("@CodPermisoExcluido", codPermisoExcluido)
            };

            return ContarUno("ExistePermisoConNombre", parametros);
        }

        public int ContarRolesConComponente(int codPermiso)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@CodPermiso", codPermiso)
            };

            return ContarUno("ContarRolesConComponente", parametros);
        }

        public int ContarFamiliasQueContienen(int codPermiso)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@CodPermiso", codPermiso)
            };

            return ContarUno("ContarFamiliasQueContienen", parametros);
        }

        #endregion

        #region Armado del arbol

        /// <summary>
        /// Devuelve la familia con todo su subárbol cargado.
        /// </summary>
        public BEFamilia TraerArbolDeFamilia(int codPermiso)
        {
            return TraerArbolDeComponente(codPermiso) as BEFamilia;
        }

        /// <summary>
        /// Devuelve el componente con su subárbol, sea hoja o familia.
        /// </summary>
        public BEComponente TraerArbolDeComponente(int codPermiso)
        {
            return Armar(codPermiso, TraerCatalogo(), new List<int>());
        }

        /// <summary>
        /// Arma varios componentes de una sola pasada. Consulta el catálogo
        /// una vez sola, en lugar de una vez por componente.
        /// </summary>
        public List<BEComponente> TraerArbolesDeComponentes(List<int> codigos)
        {
            CatalogoPermisos catalogo = TraerCatalogo();
            List<BEComponente> componentes = new List<BEComponente>();

            foreach (int codigo in codigos)
            {
                BEComponente componente = Armar(codigo, catalogo, new List<int>());

                if (componente != null)
                {
                    componentes.Add(componente);
                }
            }

            return componentes;
        }

        /// <summary>
        /// Árbol de permisos de un rol. La raíz es una familia sin persistir
        /// que agrupa los componentes asignados en Rol_Permiso; cada uno se
        /// devuelve con su subárbol completo.
        /// </summary>
        public BEFamilia TraerArbolDeRol(int codRol)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@CodRol", codRol)
            };

            DataTable tabla = dalCon.ConsultaProcAlmacenado("TraerComponentesDeRol", parametros);
            CatalogoPermisos catalogo = TraerCatalogo();
            BEFamilia raiz = new BEFamilia();

            foreach (DataRow row in tabla.Rows)
            {
                int codPermiso = Convert.ToInt32(row["CodPermiso"]);
                BEComponente componente = Armar(codPermiso, catalogo, new List<int>());

                if (componente != null)
                {
                    raiz.AgregarHijo(componente);
                }
            }

            return raiz;
        }

        // Foto de todos los nodos y todas las aristas del arbol
        private CatalogoPermisos TraerCatalogo()
        {
            CatalogoPermisos catalogo = new CatalogoPermisos();

            foreach (DataRow row in dalCon.ConsultaProcAlmacenado("TraerListaPermisos", null).Rows)
            {
                catalogo.Nodos[Convert.ToInt32(row["CodPermiso"])] = row;
            }

            foreach (DataRow row in dalCon.ConsultaProcAlmacenado("TraerRelacionesPermisos", null).Rows)
            {
                int padre = Convert.ToInt32(row["CodPadre"]);

                if (!catalogo.Hijos.ContainsKey(padre))
                {
                    catalogo.Hijos[padre] = new List<int>();
                }

                catalogo.Hijos[padre].Add(Convert.ToInt32(row["CodHijo"]));
            }

            return catalogo;
        }

        // Arma el componente y sus descendientes. "camino" son los nodos por
        // los que ya se paso: si un hijo vuelve a aparecer ahi, la arista se
        // saltea. La BLL no deja crear ciclos, pero si alguno llegara a la
        // base el recorrido no se cuelga.
        private BEComponente Armar(int codPermiso, CatalogoPermisos catalogo, List<int> camino)
        {
            if (!catalogo.Nodos.ContainsKey(codPermiso) || camino.Contains(codPermiso))
            {
                return null;
            }

            BEComponente componente = MapearComponente(catalogo.Nodos[codPermiso]);

            if (!componente.EsFamilia || !catalogo.Hijos.ContainsKey(codPermiso))
            {
                return componente;
            }

            camino.Add(codPermiso);

            foreach (int codHijo in catalogo.Hijos[codPermiso])
            {
                BEComponente hijo = Armar(codHijo, catalogo, camino);

                if (hijo != null)
                {
                    componente.AgregarHijo(hijo);
                }
            }

            camino.RemoveAt(camino.Count - 1);

            return componente;
        }

        private class CatalogoPermisos
        {
            public Dictionary<int, DataRow> Nodos = new Dictionary<int, DataRow>();
            public Dictionary<int, List<int>> Hijos = new Dictionary<int, List<int>>();
        }

        #endregion

        #region Escritura

        public int AltaFamilia(string nombre, string descripcion, List<int> componentes)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@Nombre", nombre),
                new SqlParameter("@Descripcion", descripcion),
                new SqlParameter("@Componentes", UnirCodigos(componentes))
            };

            DataTable tabla = dalCon.ConsultaProcAlmacenado("AltaFamilia", parametros);

            foreach (DataRow row in tabla.Rows)
            {
                return Convert.ToInt32(row["CodPermiso"]);
            }

            return 0;
        }

        public void ModificarFamilia(int codPermiso, string nombre, string descripcion, List<int> componentes)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@CodPermiso", codPermiso),
                new SqlParameter("@Nombre", nombre),
                new SqlParameter("@Descripcion", descripcion),
                new SqlParameter("@Componentes", UnirCodigos(componentes))
            };

            dalCon.EjecutarProcAlmacenado("ModificarFamilia", parametros);
        }

        public void ModificarEstadoFamilia(int codPermiso, bool activo)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@CodPermiso", codPermiso),
                new SqlParameter("@Activo", activo)
            };

            dalCon.EjecutarProcAlmacenado("ModificarEstadoFamilia", parametros);
        }

        #endregion

        #region Mapeo

        private List<BEComponente> MapearComponentes(DataTable tabla)
        {
            List<BEComponente> componentes = new List<BEComponente>();

            foreach (DataRow row in tabla.Rows)
            {
                componentes.Add(MapearComponente(row));
            }

            return componentes;
        }

        // Aca se decide que clase concreta del Composite se instancia. Es el
        // unico lugar del sistema que necesita mirar la columna Tipo.
        private BEComponente MapearComponente(DataRow row)
        {
            string tipo = row["Tipo"].ToString();

            BEComponente componente = tipo == BEFamilia.TipoFamilia
                ? (BEComponente)new BEFamilia()
                : new BEPermiso();

            componente.CodPermiso = Convert.ToInt32(row["CodPermiso"]);
            componente.Nombre = row["Nombre"].ToString();
            componente.Descripcion = row["Descripcion"] == DBNull.Value ? string.Empty : row["Descripcion"].ToString();
            componente.Tipo = tipo;
            componente.Activo = Convert.ToBoolean(row["Activo"]);

            return componente;
        }

        private int ContarUno(string nombreProc, SqlParameter[] parametros)
        {
            DataTable tabla = dalCon.ConsultaProcAlmacenado(nombreProc, parametros);

            foreach (DataRow row in tabla.Rows)
            {
                return Convert.ToInt32(row["Cantidad"]);
            }

            return 0;
        }

        // Los procedimientos de alta y modificacion reciben los componentes
        // en una lista separada por comas y la resuelven con STRING_SPLIT
        private string UnirCodigos(List<int> codigos)
        {
            if (codigos == null || codigos.Count == 0)
            {
                return string.Empty;
            }

            string[] textos = new string[codigos.Count];

            for (int i = 0; i < codigos.Count; i++)
            {
                textos[i] = codigos[i].ToString();
            }

            return string.Join(",", textos);
        }

        #endregion
    }
}
