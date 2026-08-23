using BE;
using System;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class DALUsuario
    {
        private readonly DALConexion dalCon = new DALConexion();

        public BEUsuario ValidarUsuario(string nombreUsuario, string email)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@NombreUsuario", nombreUsuario),
                new SqlParameter("@Email", email)
            };

            DataTable tabla = dalCon.ConsultaProcAlmacenado("ValidarUsuario", parametros);

            foreach (DataRow row in tabla.Rows)
            {
                BEUsuario usuario = new BEUsuario(
                    row["NombreUsuario"].ToString(),
                    row["Nombre"].ToString(),
                    row["Apellido"].ToString(),
                    row["Email"].ToString(),
                    row["Clave"].ToString(),
                    Convert.ToInt32(row["CodRol"]),
                    Convert.ToBoolean(row["Bloqueado"]),
                    Convert.ToBoolean(row["Activo"]));

                usuario.ContFallidos = Convert.ToInt32(row["ContFallidos"]);
                usuario.Rol = new BERol(Convert.ToInt32(row["CodRol"]), row["NombreRol"].ToString());
                usuario.Rol.Permisos = ObtenerPermisosPorRol(usuario.CodRol);

                return usuario;
            }

            return null;
        }

        public void ModificarBloqueo(string nombreUsuario, bool bloqueo)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@NombreUsuario", nombreUsuario),
                new SqlParameter("@Bloqueado", bloqueo)
            };

            dalCon.EjecutarProcAlmacenado("ModificarBloquearUsuario", parametros);
        }

        public void ModificarContFallido(string nombreUsuario, int contFallidos)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@NombreUsuario", nombreUsuario),
                new SqlParameter("@ContFallidos", contFallidos)
            };

            dalCon.EjecutarProcAlmacenado("ModificarContFallido", parametros);
        }

        private System.Collections.Generic.List<BEPermiso> ObtenerPermisosPorRol(int codRol)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@CodRol", codRol)
            };

            DataTable tabla = dalCon.ConsultaProcAlmacenado("TraerPermisosPorRol", parametros);
            System.Collections.Generic.List<BEPermiso> permisos = new System.Collections.Generic.List<BEPermiso>();

            foreach (DataRow row in tabla.Rows)
            {
                permisos.Add(new BEPermiso
                {
                    CodPermiso = Convert.ToInt32(row["CodPermiso"]),
                    Nombre = row["Nombre"].ToString(),
                    Tipo = row["Tipo"].ToString()
                });
            }

            return permisos;
        }
    }
}
