using BE;
using BE.Composite;
using DAL;
using Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace BLL
{
    
    /// Reglas de negocio de los roles (CU-005-021 a CU-005-024).
    /// El rol es el cliente del Composite: se le asignan componentes sin
    /// distinguir si son permisos simples o familias.
    
    public class BLLRol
    {
        private const string ModuloRoles = "Roles";
        private const int CriticidadCambio = 2;

        private readonly DALRol dalRol = new DALRol();
        private readonly DALPermiso dalPermiso = new DALPermiso();
        private readonly BLLEvento bllEvento = new BLLEvento();
        private readonly BLLDigitoVerificador bllDigitoVerificador = new BLLDigitoVerificador();

        #region Consulta - CU-005-021

        
        /// Roles activos, para los desplegables de la gestión de usuarios.
        
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

        
        /// Rol con su árbol de permisos cargado, para el detalle y para el
        /// formulario de modificación.
        
        public BERol TraerRolConComponentes(int codRol)
        {
            ValidarCodigo(codRol);

            return dalRol.TraerRolConComponentes(codRol);
        }

        // Para ConsultarArbolPermisos: cada rol activo con su arbol de
        // familias y permisos sueltos
        public List<BERol> TraerRolesConArbol()
        {
            List<BERol> roles = new List<BERol>();

            foreach (BERol rol in dalRol.FiltrarRoles(null, true))
            {
                BERol conComponentes = dalRol.TraerRolConComponentes(rol.CodRol);

                if (conComponentes != null)
                {
                    roles.Add(conComponentes);
                }
            }

            return roles;
        }

        #endregion

        #region Exportacion XML - CU-005-021

        // Los roles que cumplen los filtros de la consulta, cada uno con su
        // arbol completo. El XML queda anidado igual que el Composite: una
        // familia contiene permisos y otras familias
        public byte[] ExportarRolesXml(string nombre, bool? activo, string nombreUsuarioEnSesion)
        {
            List<BERol> roles = new List<BERol>();

            foreach (BERol rol in FiltrarRoles(nombre, activo))
            {
                BERol conComponentes = dalRol.TraerRolConComponentes(rol.CodRol);

                if (conComponentes != null)
                {
                    roles.Add(conComponentes);
                }
            }

            // Alternativa 10.1 [#ERR013]: no se exporta un archivo vacio
            if (roles.Count == 0)
            {
                throw new Exception(Traductor.T("comun.sinResultadosFiltros"));
            }

            // Alternativa 10.2 [#ERR020]
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    // Se escribe en forma secuencial: encabezado y cada rol con su arbol
                    XmlTextWriter escritor = new XmlTextWriter(stream, new UTF8Encoding(false));
                    escritor.Formatting = Formatting.Indented;

                    escritor.WriteStartDocument();
                    escritor.WriteStartElement("RolesYPermisos");

                    escritor.WriteStartElement("Encabezado");
                    escritor.WriteElementString("FechaGeneracion", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"));
                    escritor.WriteElementString("GeneradoPor", nombreUsuarioEnSesion);
                    escritor.WriteElementString("CantidadRoles", roles.Count.ToString());

                    // Los filtros vacios quedan como nodos vacios: significan "todos"
                    escritor.WriteStartElement("Filtros");
                    escritor.WriteElementString("Nombre", string.IsNullOrWhiteSpace(nombre) ? string.Empty : nombre.Trim());
                    escritor.WriteElementString("Estado", activo.HasValue ? (activo.Value ? "Activos" : "Inactivos") : string.Empty);
                    escritor.WriteEndElement();

                    escritor.WriteEndElement();

                    escritor.WriteStartElement("Roles");

                    foreach (BERol rol in roles)
                    {
                        escritor.WriteStartElement("Rol");
                        escritor.WriteAttributeString("Codigo", rol.CodRol.ToString());
                        escritor.WriteAttributeString("Nombre", rol.Nombre);
                        escritor.WriteAttributeString("Activo", rol.Activo ? "true" : "false");
                        escritor.WriteAttributeString("UsuariosActivos", rol.UsuariosActivos.ToString());
                        escritor.WriteAttributeString("PantallasHabilitadas", rol.ObtenerPermisosSimples().Count.ToString());

                        foreach (BEComponente componente in rol.Componentes.ObtenerHijos())
                        {
                            EscribirComponente(escritor, componente);
                        }

                        escritor.WriteEndElement();
                    }

                    escritor.WriteEndElement();
                    escritor.WriteEndElement();
                    escritor.WriteEndDocument();

                    escritor.Flush();

                    return stream.ToArray();
                }
            }
            catch (Exception)
            {
                throw new Exception(Traductor.T("error.exportarReporte"));
            }
        }

        public string NombreArchivoExportacion()
        {
            return "Roles-" + DateTime.Now.ToString("yyyyMMdd-HHmm") + ".xml";
        }

        // Mismo recorrido para hojas y familias: una hoja no tiene hijos y la
        // recursion corta sola
        private void EscribirComponente(XmlTextWriter escritor, BEComponente componente)
        {
            escritor.WriteStartElement(componente.EsFamilia ? "Familia" : "Permiso");
            escritor.WriteAttributeString("Codigo", componente.CodPermiso.ToString());
            escritor.WriteAttributeString("Nombre", componente.Nombre);
            escritor.WriteAttributeString("Activo", componente.Activo ? "true" : "false");

            if (componente.EsFamilia)
            {
                escritor.WriteAttributeString("PantallasHabilitadas", componente.CantidadPermisosSimples.ToString());
            }

            foreach (BEComponente hijo in componente.ObtenerHijos())
            {
                EscribirComponente(escritor, hijo);
            }

            escritor.WriteEndElement();
        }

        #endregion

        #region Alta - CU-005-022

        public int AltaRol(string nombre, List<int> componentes, string nombreUsuarioEnSesion)
        {
            ValidarDatos(nombre, componentes);

            // Alternativa 4.2 del CU [#ERR038]
            if (dalRol.ContarRolesConNombre(nombre.Trim(), null) > 0)
            {
                throw new Exception(Traductor.T("error.rolDuplicado"));
            }

            ValidarComponentes(componentes);

            int codRol = dalRol.AltaRol(nombre.Trim(), componentes);
            bllDigitoVerificador.PersistirDV("Rol");
            bllDigitoVerificador.PersistirDV("Rol_Permiso");

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
                throw new Exception(Traductor.T("error.rolNoEncontrado"));
            }

            // Alternativa 4.2 del CU [#ERR038]
            if (dalRol.ContarRolesConNombre(nombre.Trim(), codRol) > 0)
            {
                throw new Exception(Traductor.T("error.rolDuplicado"));
            }

            ValidarComponentes(componentes);

            dalRol.ModificarRol(codRol, nombre.Trim(), componentes);
            bllDigitoVerificador.PersistirDV("Rol");
            bllDigitoVerificador.PersistirDV("Rol_Permiso");

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
                throw new Exception(Traductor.T("error.rolNoEncontrado"));
            }

            if (rol.Activo == activo)
            {
                throw new Exception(activo
                    ? Traductor.T("error.rolYaActivo")
                    : Traductor.T("error.rolYaDadoDeBaja"));
            }

            // Alternativa 2.1 del CU [#ERR039]
            if (!activo && dalRol.ContarUsuariosActivosPorRol(codRol) > 0)
            {
                throw new Exception(Traductor.T("error.rolConUsuarios"));
            }

            dalRol.ModificarEstadoRol(codRol, activo);
            bllDigitoVerificador.PersistirDV("Rol");

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
                throw new Exception(Traductor.T("error.nombreRolRequerido"));
            }

            if (nombre.Trim().Length > 50)
            {
                throw new Exception(Traductor.T("error.nombreRolLargo"));
            }

            if (componentes == null || componentes.Count == 0)
            {
                throw new Exception(Traductor.T("error.rolVacio"));
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
                    throw new Exception(Traductor.T("error.componenteInexistente"));
                }

                if (!componente.Activo)
                {
                    throw new Exception(Traductor.T("error.componenteDadoDeBaja", componente.Nombre));
                }
            }
        }

        private void ValidarCodigo(int codRol)
        {
            if (codRol <= 0)
            {
                throw new Exception(Traductor.T("error.seleccioneRol"));
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
