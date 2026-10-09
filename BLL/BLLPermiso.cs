using BE.Composite;
using DAL;
using Services;
using System;
using System.Collections.Generic;

namespace BLL
{
    
    /// Reglas de negocio de los permisos y las familias.
    /// Los permisos simples no tienen ABM: vienen precargados en la base.
    /// Las familias sí (CU-005-025 a CU-005-028).
    
    public class BLLPermiso
    {
        private const string ModuloFamilias = "Familias";
        private const int CriticidadCambio = 2;

        private readonly DALPermiso dalPermiso = new DALPermiso();
        private readonly BLLEvento bllEvento = new BLLEvento();
        private readonly BLLDigitoVerificador bllDigitoVerificador = new BLLDigitoVerificador();

        #region Consulta - CU-005-025

        
        /// Componentes que se pueden elegir al armar un rol o una familia:
        /// permisos simples y familias, todos activos. El selector los trata
        /// por igual porque los dos son BEComponente.
        
        public List<BEComponente> TraerComponentesDisponibles()
        {
            return dalPermiso.TraerListaPermisos(null, true);
        }

        
        /// Lo mismo, pero con el subárbol de cada componente ya cargado. Es lo
        /// que necesita el selector para avisar cuando un permiso ya viene
        /// incluido dentro de una familia elegida.
        
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
            ValidarCodigo(codPermiso);

            return dalPermiso.TraerArbolDeFamilia(codPermiso);
        }

        public List<BEComponente> TraerHijosDeFamilia(int codPermiso)
        {
            ValidarCodigo(codPermiso);

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
                throw new Exception(Traductor.T("error.permisoDuplicado"));
            }

            // En el alta todavia no hay codigo asignado, asi que la familia no
            // puede estar contenida en ninguna otra: la validacion solo puede
            // fallar por un componente que no exista
            ValidarComponentes(componentes, 0);

            int codPermiso = dalPermiso.AltaFamilia(nombre.Trim(), Normalizar(descripcion), componentes);
            bllDigitoVerificador.PersistirDV("Permiso");
            bllDigitoVerificador.PersistirDV("Permiso_Componente");

            RegistrarEvento(nombreUsuarioEnSesion, "Alta de familia", nombre.Trim());

            return codPermiso;
        }

        #endregion

        #region Modificacion - CU-005-028

        public void ModificarFamilia(int codPermiso, string nombre, string descripcion, List<int> componentes, string nombreUsuarioEnSesion)
        {
            ValidarCodigo(codPermiso);
            ValidarDatos(nombre, componentes);

            BEComponente actual = dalPermiso.TraerPermisoPorId(codPermiso);

            if (actual == null || !actual.EsFamilia)
            {
                throw new Exception(Traductor.T("error.familiaNoEncontrada"));
            }

            // Alternativa 4.3 del CU [#ERR040]
            if (dalPermiso.ContarPermisosConNombre(nombre.Trim(), codPermiso) > 0)
            {
                throw new Exception(Traductor.T("error.permisoDuplicado"));
            }

            // Alternativa 4.2 del CU [#ERR041]
            ValidarComponentes(componentes, codPermiso);

            dalPermiso.ModificarFamilia(codPermiso, nombre.Trim(), Normalizar(descripcion), componentes);
            bllDigitoVerificador.PersistirDV("Permiso");
            bllDigitoVerificador.PersistirDV("Permiso_Componente");

            RegistrarEvento(nombreUsuarioEnSesion, "Modificacion de familia", nombre.Trim());
        }

        #endregion

        #region Baja - CU-005-027

        public void CambiarEstadoFamilia(int codPermiso, bool activo, string nombreUsuarioEnSesion)
        {
            ValidarCodigo(codPermiso);

            BEComponente familia = dalPermiso.TraerPermisoPorId(codPermiso);

            if (familia == null || !familia.EsFamilia)
            {
                throw new Exception(Traductor.T("error.familiaNoEncontrada"));
            }

            if (familia.Activo == activo)
            {
                throw new Exception(activo
                    ? Traductor.T("error.familiaYaActiva")
                    : Traductor.T("error.familiaYaDadaDeBaja"));
            }

            if (!activo)
            {
                // Alternativa 2.1 del CU [#ERR042]
                if (dalPermiso.ContarRolesConComponente(codPermiso) > 0)
                {
                    throw new Exception(Traductor.T("error.familiaEnRoles"));
                }

                // Alternativa 2.2 del CU [#ERR043]
                if (dalPermiso.ContarFamiliasQueContienen(codPermiso) > 0)
                {
                    throw new Exception(Traductor.T("error.familiaEnFamilia"));
                }
            }

            dalPermiso.ModificarEstadoFamilia(codPermiso, activo);
            bllDigitoVerificador.PersistirDV("Permiso");

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
                throw new Exception(Traductor.T("error.nombreFamiliaRequerido"));
            }

            if (nombre.Trim().Length > 80)
            {
                throw new Exception(Traductor.T("error.nombreFamiliaLargo"));
            }

            if (componentes == null || componentes.Count == 0)
            {
                throw new Exception(Traductor.T("error.familiaVacia"));
            }
        }

        
        /// Controla que todos los componentes elegidos existan y que ninguno
        /// genere una referencia circular.
        ///
        /// La detección se apoya en el propio Composite: se arma el subárbol
        /// de cada componente elegido y se le pregunta si contiene a la
        /// familia que se está editando. Si la contiene, meterla adentro
        /// cerraría el ciclo. Al recorrer todo el subárbol también quedan
        /// cubiertos los ciclos indirectos, no solo el caso de elegirse a
        /// sí misma.
        
        private void ValidarComponentes(List<int> componentes, int codPermisoEditado)
        {
            List<BEComponente> armados = dalPermiso.TraerArbolesDeComponentes(componentes);

            if (armados.Count != componentes.Count)
            {
                throw new Exception(Traductor.T("error.componenteInexistente"));
            }

            foreach (BEComponente componente in armados)
            {
                if (codPermisoEditado > 0 && componente.Contiene(codPermisoEditado))
                {
                    throw new Exception(Traductor.T("error.referenciaCircular", componente.Nombre));
                }
            }
        }

        private void ValidarCodigo(int codigo)
        {
            if (codigo <= 0)
            {
                throw new Exception(Traductor.T("error.seleccioneFamilia"));
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
