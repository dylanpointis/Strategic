using BE;
using DAL;
using Services;
using System;

namespace BLL
{
    public class BLLUsuario
    {
        private readonly DALUsuario dalUsuario = new DALUsuario();
        private readonly BLLEvento bllEvento = new BLLEvento();

        public BEUsuario Login(string nombreUsuario, string clave)
        {
            BEUsuario usuario = ValidarUsuario(nombreUsuario, string.Empty);

            if (usuario == null)
            {
                throw new Exception("El usuario ingresado no existe");
            }

            if (usuario.Bloqueado || !usuario.Activo)
            {
                throw new Exception("El usuario se encuentra bloqueado o desactivado, comuniquese con el administrador del sistema");
            }

            if (Encriptacion.EncriptarSHA256(clave) != usuario.Clave)
            {
                int intentosFallidos = usuario.ContFallidos + 1;
                ModificarContFallido(usuario.NombreUsuario, intentosFallidos);

                if (intentosFallidos >= 3)
                {
                    ModificarBloqueo(usuario.NombreUsuario, true);
                    bllEvento.RegistrarEvento(new Evento(usuario.NombreUsuario, "Sesiones", "Usuario bloqueado", 1));
                    throw new Exception("Se ha bloqueado al usuario por superar el limite de intentos fallidos, por favor comuniquese con el administrador del sistema");
                }

                throw new Exception("Las credenciales no coinciden");
            }

            ModificarContFallido(usuario.NombreUsuario, 0);
            bllEvento.RegistrarEvento(new Evento(usuario.NombreUsuario, "Sesiones", "Inicio sesion", 1));

            return usuario;
        }

        public BEUsuario ValidarUsuario(string nombreUsuario, string email)
        {
            return dalUsuario.ValidarUsuario(nombreUsuario, email);
        }

        public void ModificarContFallido(string nombreUsuario, int contFallidos)
        {
            dalUsuario.ModificarContFallido(nombreUsuario, contFallidos);
        }

        public void ModificarBloqueo(string nombreUsuario, bool bloqueo)
        {
            dalUsuario.ModificarBloqueo(nombreUsuario, bloqueo);
        }
    }
}
