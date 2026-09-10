using BE.Composite;
using DAL;
using Services;
using System;
using System.Collections.Generic;

namespace BLL
{
    /// <summary>
    /// Reglas de negocio de los permisos y las familias.
    /// Los permisos simples no tienen ABM: vienen precargados en la base.
    /// Las familias sí (CU-005-025 a CU-005-028).
    /// </summary>
    public class BLLPermiso
    {
        private const string ModuloFamilias = "Familias";
        private const int CriticidadCambio = 2;

        private readonly DALPermiso dalPermiso = new DALPermiso();
        private readonly BLLEvento bllEvento = new BLLEvento();

        #region Consulta - CU-005-025

        /// <summary>
        /// Componentes que se pueden elegir al armar un rol o una familia:
        /// permisos simples y familias, todos activos. El selector los trata
        /// por igual porque los dos son BEComponente.
        /// </summary>
        public List<BEComponente> TraerComponentesDisponibles()
        {
            return dalPermiso.TraerListaPermisos(null, true);
        }

        /// <summary>
        /// Lo mismo, pero con el subárbol de cada componente ya cargado. Es lo
        /// que necesita el selector para avisar cuando un permiso ya viene
        /// incluido dentro de una familia elegida.
        /// </summary>
        public List<BEComponente> TraerComponentesDisponiblesConArbol()
        {
            List<int> codigos = new List<int>();

            foreach (BEComponente componente in dalPermiso.TraerListaPermisos(null, true))
            {
                codigos.Add(componente.CodPermiso);
            }

            return dalPermiso.TraerArbolesDeComponentes(codigos);
        }

        public List<BEComponente> TraerPermisosSimples()
        {
            return dalPermiso.TraerListaPermisos(BEPermiso.TipoSimple, true);
        }

        public List<BEFamilia> FiltrarFamilias(string nombre, bool? activo)
        {
            return dalPermiso.FiltrarFamilias(Normalizar(nombre), activo);
        }

        public BEFamilia TraerFamiliaConHijos(int codPermiso)
        {
            ValidarCodigo(codPermiso, "familia");

            return dalPermiso.TraerArbolDeFamilia(codPermiso);
        }

        public List<BEComponente> TraerHijosDeFamilia(int codPermiso)
        {
            ValidarCodigo(codPermiso, "familia");

            return dalPermiso.TraerHijosDeFamilia(codPermiso);
        }

        #endregion

        #region Alta - CU-005-026

        public int AltaFamilia(string nombre, string descripcion, List<int> componentes, string nombreUsuarioEnSesion)
        {
            ValidarDatos(nombre, componentes);

            // Alternativa 4.3 del CU [#ERR040]
            if (dalPermiso.ContarPermisosConNombre(nombre.Trim(), null) > 0)
            {
                throw new Exception("Ya existe un permiso o una familia con ese nombre");
            }

            // En el alta todavia no hay codigo asignado, asi que la familia no
            // puede estar contenida en ninguna otra: la validacion solo puede
            // fallar por un componente que no exista
            ValidarComponentes(componentes, 0);

            int codPermiso = dalPermiso.AltaFamilia(nombre.Trim(), Normalizar(descripcion), componentes);

            RegistrarEvento(nombreUsuarioEnSesion, "Alta de familia", nombre.Trim());

            return codPermiso;
        }

        #endregion

        #region Modificacion - CU-005-028

        public void ModificarFamilia(int codPermiso, string nombre, string descripcion, List<int> componentes, string nombreUsuarioEnSesion)
        {
            ValidarCodigo(codPermiso, "familia");
            ValidarDatos(nombre, componentes);

            BEComponente actual = dalPermiso.TraerPermisoPorId(codPermiso);

            if (actual == null || !actual.EsFamilia)
            {
                throw new Exception("No se encontro la familia seleccionada");
            }

            // Alternativa 4.3 del CU [#ERR040]
            if (dalPermiso.ContarPermisosConNombre(nombre.Trim(), codPermiso) > 0)
            {
                throw new Exception("Ya existe un permiso o una familia con ese nombre");
            }

            // Alternativa 4.2 del CU [#ERR041]
            ValidarComponentes(componentes, codPermiso);

            dalPermiso.ModificarFamilia(codPermiso, nombre.Trim(), Normalizar(descripcion), componentes);

            RegistrarEvento(nombreUsuarioEnSesion, "Modificacion de familia", nombre.Trim());
        }

        #endregion

        #region Baja - CU-005-027

        public void CambiarEstadoFamilia(int codPermiso, bool activo, string nombreUsuarioEnSesion)
        {
            ValidarCodigo(codPermiso, "familia");

            BEComponente familia = dalPermiso.TraerPermisoPorId(codPermiso);

            if (familia == null || !familia.EsFamilia)
            {
                throw new Exception("No se encontro la familia seleccionada");
            }

            if (familia.Activo == activo)
            {
                throw new Exception(activo
                    ? "La familia ya se encuentra activa"
                    : "La familia ya se encuentra dada de baja");
            }

            if (!activo)
            {
                // Alternativa 2.1 del CU [#ERR042]
                if (dalPermiso.ContarRolesConComponente(codPermiso) > 0)
                {
                    throw new Exception("No se puede dar de baja la familia porque esta asignada a uno o mas roles");
                }

                // Alternativa 2.2 del CU [#ERR043]
                if (dalPermiso.ContarFamiliasQueContienen(codPermiso) > 0)
                {
                    throw new Exception("No se puede dar de baja la familia porque esta contenida en otra familia");
                }
            }

            dalPermiso.ModificarEstadoFamilia(codPermiso, activo);

            RegistrarEvento(
                nombreUsuarioEnSesion,
                activo ? "Reactivacion de familia" : "Baja de familia",
                familia.Nombre);
        }

        #endregion

        #region Validaciones

        private void ValidarDatos(string nombre, List<int> componentes)
        {
            // Campos obligatorios incompletos [#ERR001]
            if (string.IsNullOrWhiteSpace(nombre))
            {
                throw new Exception("Debe completar el nombre de la familia");
            }

            if (nombre.Trim().Length > 80)
            {
                throw new Exception("El nombre de la familia supera los 80 caracteres");
            }

            if (componentes == null || componentes.Count == 0)
            {
                throw new Exception("La familia tiene que contener al menos un permiso o familia");
            }
        }

        /// <summary>
        /// Controla que todos los componentes elegidos existan y que ninguno
        /// genere una referencia circular.
        ///
        /// La detección se apoya en el propio Composite: se arma el subárbol
        /// de cada componente elegido y se le pregunta si contiene a la
        /// familia que se está editando. Si la contiene, meterla adentro
        /// cerraría el ciclo. Al recorrer todo el subárbol también quedan
        /// cubiertos los ciclos indirectos, no solo el caso de elegirse a
        /// sí misma.
        /// </summary>
        private void ValidarComponentes(List<int> componentes, int codPermisoEditado)
        {
            List<BEComponente> armados = dalPermiso.TraerArbolesDeComponentes(componentes);

            if (armados.Count != componentes.Count)
            {
                throw new Exception("Alguno de los componentes seleccionados ya no existe");
            }

            foreach (BEComponente componente in armados)
            {
                if (codPermisoEditado > 0 && componente.Contiene(codPermisoEditado))
                {
                    throw new Exception(string.Format(
                        "No se puede agregar \"{0}\": generaria una referencia circular",
                        componente.Nombre));
                }
            }
        }

        private void ValidarCodigo(int codigo, string entidad)
        {
            if (codigo <= 0)
            {
                throw new Exception(string.Format("Debe seleccionar una {0}", entidad));
            }
        }

        #endregion

        private void RegistrarEvento(string nombreUsuarioEnSesion, string accion, string nombre)
        {
            bllEvento.RegistrarEvento(new Evento(
                nombreUsuarioEnSesion,
                ModuloFamilias,
                string.Format("{0}: {1}", accion, nombre),
                CriticidadCambio));
        }

        private string Normalizar(string valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }
    }
}
