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

        public System.Collections.Generic.List<BEUsuario> TraerListaUsuarios()
        {
            return MapearUsuarios(dalCon.ConsultaProcAlmacenado("TraerListaUsuarios", null));
        }

        public System.Collections.Generic.List<BEUsuario> FiltrarUsuarios(string texto, int? codRol, bool? activo)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@Texto", texto),
                new SqlParameter("@CodRol", codRol),
                new SqlParameter("@Activo", activo)
            };

            return MapearUsuarios(dalCon.ConsultaProcAlmacenado("FiltrarUsuarios", parametros));
        }

        public BEUsuario TraerUsuarioPorNombre(string nombreUsuario)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@NombreUsuario", nombreUsuario)
            };

            System.Collections.Generic.List<BEUsuario> usuarios = MapearUsuarios(
                dalCon.ConsultaProcAlmacenado("TraerUsuarioPorNombre", parametros));

            return usuarios.Count > 0 ? usuarios[0] : null;
        }

        /// <summary>
        /// Cantidad de usuarios que ya usan ese nombre de usuario o ese email.
        /// nombreUsuarioExcluido deja fuera al usuario que se esta editando.
        /// </summary>
        public int ContarUsuariosConDatos(string nombreUsuario, string email, string nombreUsuarioExcluido)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@NombreUsuario", nombreUsuario),
                new SqlParameter("@Email", email),
                new SqlParameter("@NombreUsuarioExcluido", nombreUsuarioExcluido)
            };

            DataTable tabla = dalCon.ConsultaProcAlmacenado("ExisteUsuario", parametros);

            foreach (DataRow row in tabla.Rows)
            {
                return Convert.ToInt32(row["Cantidad"]);
            }

            return 0;
        }

        public void AltaUsuario(BEUsuario usuario)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@NombreUsuario", usuario.NombreUsuario),
                new SqlParameter("@Nombre", usuario.Nombre),
                new SqlParameter("@Apellido", usuario.Apellido),
                new SqlParameter("@Email", usuario.Email),
                new SqlParameter("@Clave", usuario.Clave),
                new SqlParameter("@CodRol", usuario.CodRol)
            };

            dalCon.EjecutarProcAlmacenado("AltaUsuario", parametros);
        }

        public void ModificarUsuario(BEUsuario usuario)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@NombreUsuario", usuario.NombreUsuario),
                new SqlParameter("@Nombre", usuario.Nombre),
                new SqlParameter("@Apellido", usuario.Apellido),
                new SqlParameter("@Email", usuario.Email),
                new SqlParameter("@CodRol", usuario.CodRol),
                new SqlParameter("@Bloqueado", usuario.Bloqueado)
            };

            dalCon.EjecutarProcAlmacenado("ModificarUsuario", parametros);
        }

        public void ModificarEstado(string nombreUsuario, bool activo)
        {
            SqlParameter[] parametros = new SqlParameter[]
            {
                new SqlParameter("@NombreUsuario", nombreUsuario),
                new SqlParameter("@Activo", activo)
            };

            dalCon.EjecutarProcAlmacenado("ModificarEstadoUsuario", parametros);
        }

        // Los procedimientos de listado no devuelven la clave encriptada,
        // asi que el mapeo la deja vacia
        private System.Collections.Generic.List<BEUsuario> MapearUsuarios(DataTable tabla)
        {
            System.Collections.Generic.List<BEUsuario> usuarios = new System.Collections.Generic.List<BEUsuario>();

            foreach (DataRow row in tabla.Rows)
            {
                BEUsuario usuario = new BEUsuario(
                    row["NombreUsuario"].ToString(),
                    row["Nombre"].ToString(),
                    row["Apellido"].ToString(),
                    row["Email"].ToString(),
                    string.Empty,
                    Convert.ToInt32(row["CodRol"]),
                    Convert.ToBoolean(row["Bloqueado"]),
                    Convert.ToBoolean(row["Activo"]));

                usuario.ContFallidos = Convert.ToInt32(row["ContFallidos"]);
                usuario.Rol = new BERol(usuario.CodRol, row["NombreRol"].ToString());

                usuarios.Add(usuario);
            }

            return usuarios;
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
