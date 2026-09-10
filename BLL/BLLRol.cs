using BE;
using BE.Composite;
using DAL;
using Services;
using System;
using System.Collections.Generic;

namespace BLL
{
    /// <summary>
    /// Reglas de negocio de los roles (CU-005-021 a CU-005-024).
    /// El rol es el cliente del Composite: se le asignan componentes sin
    /// distinguir si son permisos simples o familias.
    /// </summary>
    public class BLLRol
    {
        private const string ModuloRoles = "Roles";
        private const int CriticidadCambio = 2;

        private readonly DALRol dalRol = new DALRol();
        private readonly DALPermiso dalPermiso = new DALPermiso();
        private readonly BLLEvento bllEvento = new BLLEvento();

        #region Consulta - CU-005-021

        /// <summary>
        /// Roles activos, para los desplegables de la gestión de usuarios.
        /// </summary>
        public List<BERol> TraerListaRoles()
        {
            return dalRol.TraerListaRoles();
        }

        public List<BERol> FiltrarRoles(string nombre, bool? activo)
        {
            return dalRol.FiltrarRoles(Normalizar(nombre), activo);
        }

        public BERol TraerRolPorId(int codRol)
        {
            ValidarCodigo(codRol);

            return dalRol.TraerRolPorId(codRol);
        }

        /// <summary>
        /// Rol con su árbol de permisos cargado, para el detalle y para el
        /// formulario de modificación.
        /// </summary>
        public BERol TraerRolConComponentes(int codRol)
        {
            ValidarCodigo(codRol);

            return dalRol.TraerRolConComponentes(codRol);
        }

        #endregion

        #region Alta - CU-005-022

        public int AltaRol(string nombre, List<int> componentes, string nombreUsuarioEnSesion)
        {
            ValidarDatos(nombre, componentes);

            // Alternativa 4.2 del CU [#ERR038]
            if (dalRol.ContarRolesConNombre(nombre.Trim(), null) > 0)
            {
                throw new Exception("Ya existe un rol con ese nombre");
            }

            ValidarComponentes(componentes);

            int codRol = dalRol.AltaRol(nombre.Trim(), componentes);

            RegistrarEvento(nombreUsuarioEnSesion, "Alta de rol", nombre.Trim());

            return codRol;
        }

        #endregion

        #region Modificacion - CU-005-024

        public void ModificarRol(int codRol, string nombre, List<int> componentes, string nombreUsuarioEnSesion)
        {
            ValidarCodigo(codRol);
            ValidarDatos(nombre, componentes);

            BERol actual = dalRol.TraerRolPorId(codRol);

            if (actual == null)
            {
                throw new Exception("No se encontro el rol seleccionado");
            }

            // Alternativa 4.2 del CU [#ERR038]
            if (dalRol.ContarRolesConNombre(nombre.Trim(), codRol) > 0)
            {
                throw new Exception("Ya existe un rol con ese nombre");
            }

            ValidarComponentes(componentes);

            dalRol.ModificarRol(codRol, nombre.Trim(), componentes);

            RegistrarEvento(nombreUsuarioEnSesion, "Modificacion de rol", nombre.Trim());
        }

        #endregion

        #region Baja - CU-005-023

        public void CambiarEstadoRol(int codRol, bool activo, string nombreUsuarioEnSesion)
        {
            ValidarCodigo(codRol);

            BERol rol = dalRol.TraerRolPorId(codRol);

            if (rol == null)
            {
                throw new Exception("No se encontro el rol seleccionado");
            }

            if (rol.Activo == activo)
            {
                throw new Exception(activo
                    ? "El rol ya se encuentra activo"
                    : "El rol ya se encuentra dado de baja");
            }

            // Alternativa 2.1 del CU [#ERR039]
            if (!activo && dalRol.ContarUsuariosActivosPorRol(codRol) > 0)
            {
                throw new Exception("No se puede dar de baja el rol porque hay usuarios activos que lo tienen asignado");
            }

            dalRol.ModificarEstadoRol(codRol, activo);

            RegistrarEvento(
                nombreUsuarioEnSesion,
                activo ? "Reactivacion de rol" : "Baja de rol",
                rol.Nombre);
        }

        #endregion

        #region Validaciones

        private void ValidarDatos(string nombre, List<int> componentes)
        {
            // Campos obligatorios incompletos [#ERR001]
            if (string.IsNullOrWhiteSpace(nombre))
            {
                throw new Exception("Debe completar el nombre del rol");
            }

            if (nombre.Trim().Length > 50)
            {
                throw new Exception("El nombre del rol supera los 50 caracteres");
            }

            if (componentes == null || componentes.Count == 0)
            {
                throw new Exception("El rol tiene que tener al menos un permiso o familia asignada");
            }
        }

        // Un rol no puede armarse con componentes que ya no existen o que
        // fueron dados de baja. No hace falta controlar ciclos: el rol nunca
        // es hijo de nadie en el arbol
        private void ValidarComponentes(List<int> componentes)
        {
            foreach (int codPermiso in componentes)
            {
                BEComponente componente = dalPermiso.TraerPermisoPorId(codPermiso);

                if (componente == null)
                {
                    throw new Exception("Alguno de los componentes seleccionados ya no existe");
                }

                if (!componente.Activo)
                {
                    throw new Exception(string.Format(
                        "No se puede asignar \"{0}\" porque esta dado de baja",
                        componente.Nombre));
                }
            }
        }

        private void ValidarCodigo(int codRol)
        {
            if (codRol <= 0)
            {
                throw new Exception("Debe seleccionar un rol");
            }
        }

        #endregion

        private void RegistrarEvento(string nombreUsuarioEnSesion, string accion, string nombre)
        {
            bllEvento.RegistrarEvento(new Evento(
                nombreUsuarioEnSesion,
                ModuloRoles,
                string.Format("{0}: {1}", accion, nombre),
                CriticidadCambio));
        }

        private string Normalizar(string valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }
    }
}
