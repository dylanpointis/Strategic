using BE;
using BE.Composite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class DALRol
    {
        private readonly DALConexion dalCon = new DALConexion();
        private readonly DALPermiso dalPermiso = new DALPermiso();

        public List<BERol> TraerListaRoles()
        {
            DataTable tabla = dalCon.ConsultaProcAlmacenado("TraerListaRoles", null);
            List<BERol> roles = new List<BERol>();

            foreach (DataRow row in tabla.Rows)
            {
                roles.Add(new BERol(
                    Convert.ToInt32(row["CodRol"]),
                    row["Nombre"].ToString()));
            }

            return roles;
        }

        public List<BERol> FiltrarRoles(string nombre, bool? activo)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@Nombre", nombre),
                new SqlParameter("@Activo", activo)
            };

            return MapearRoles(dalCon.ConsultaProcAlmacenado("FiltrarRoles", parametros));
        }

        public BERol TraerRolPorId(int codRol)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@CodRol", codRol)
            };

            List<BERol> roles = MapearRoles(dalCon.ConsultaProcAlmacenado("TraerRolPorId", parametros));

            return roles.Count > 0 ? roles[0] : null;
        }

        /// <summary>
        /// Rol con su árbol de permisos cargado.
        /// </summary>
        public BERol TraerRolConComponentes(int codRol)
        {
            BERol rol = TraerRolPorId(codRol);

            if (rol == null)
            {
                return null;
            }

            foreach (BEComponente componente in dalPermiso.TraerArbolDeRol(codRol).ObtenerHijos())
            {
                rol.Componentes.AgregarHijo(componente);
            }

            return rol;
        }

        public int ContarRolesConNombre(string nombre, int? codRolExcluido)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@Nombre", nombre),
                new SqlParameter("@CodRolExcluido", codRolExcluido)
            };

            return ContarUno("ExisteRol", parametros);
        }

        public int ContarUsuariosActivosPorRol(int codRol)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@CodRol", codRol)
            };

            return ContarUno("ContarUsuariosActivosPorRol", parametros);
        }

        public int AltaRol(string nombre, List<int> componentes)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@Nombre", nombre),
                new SqlParameter("@Componentes", UnirCodigos(componentes))
            };

            DataTable tabla = dalCon.ConsultaProcAlmacenado("AltaRol", parametros);

            foreach (DataRow row in tabla.Rows)
            {
                return Convert.ToInt32(row["CodRol"]);
            }

            return 0;
        }

        public void ModificarRol(int codRol, string nombre, List<int> componentes)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@CodRol", codRol),
                new SqlParameter("@Nombre", nombre),
                new SqlParameter("@Componentes", UnirCodigos(componentes))
            };

            dalCon.EjecutarProcAlmacenado("ModificarRol", parametros);
        }

        public void ModificarEstadoRol(int codRol, bool activo)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@CodRol", codRol),
                new SqlParameter("@Activo", activo)
            };

            dalCon.EjecutarProcAlmacenado("ModificarEstadoRol", parametros);
        }

        private List<BERol> MapearRoles(DataTable tabla)
        {
            List<BERol> roles = new List<BERol>();

            foreach (DataRow row in tabla.Rows)
            {
                BERol rol = new BERol(
                    Convert.ToInt32(row["CodRol"]),
                    row["Nombre"].ToString());

                rol.Activo = Convert.ToBoolean(row["Activo"]);
                rol.CantidadComponentesAsignados = Convert.ToInt32(row["CantidadComponentes"]);
                rol.UsuariosActivos = Convert.ToInt32(row["UsuariosActivos"]);

                roles.Add(rol);
            }

            return roles;
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
    }
}
