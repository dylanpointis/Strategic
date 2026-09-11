using BE;
using DAL;
using Services;
using System;
using System.Collections.Generic;

namespace BLL
{
    public class BLLCompetencia
    {
        private const string ModuloCompetencia = "Competencia";
        private const int CriticidadCambio = 2;

        private readonly DALCompetencia dalCompetencia = new DALCompetencia();
        private readonly BLLEvento bllEvento = new BLLEvento();

        #region Consulta

        public List<BECompetencia> TraerListaCompetidores()
        {
            return dalCompetencia.TraerListaCompetidores();
        }

        public List<BECompetencia> FiltrarCompetidores(string texto, string marketplace, bool? activo)
        {
            return dalCompetencia.FiltrarCompetidores(Normalizar(texto), Normalizar(marketplace), activo);
        }

        public BECompetencia TraerCompetidorPorId(int idCompetencia)
        {
            ValidarCodigo(idCompetencia);

            return dalCompetencia.TraerCompetidorPorId(idCompetencia);
        }

        #endregion

        #region Alta

        public int AltaCompetidor(BECompetencia competidor, string nombreUsuarioEnSesion)
        {
            ValidarDatos(competidor);

            if (dalCompetencia.ContarCompetidoresConNombre(competidor.Nombre, null) > 0)
            {
                throw new Exception("Ya existe un competidor con ese nombre");
            }

            int idCompetencia = dalCompetencia.AltaCompetidor(competidor);

            RegistrarEvento(nombreUsuarioEnSesion, "Alta de competidor", competidor.Nombre);

            return idCompetencia;
        }

        #endregion

        #region Modificacion

        public void ModificarCompetidor(BECompetencia competidor, string nombreUsuarioEnSesion)
        {
            ValidarCodigo(competidor.IdCompetencia);
            ValidarDatos(competidor);

            BECompetencia actual = dalCompetencia.TraerCompetidorPorId(competidor.IdCompetencia);

            if (actual == null)
            {
                throw new Exception("No se encontro el competidor seleccionado");
            }

            if (dalCompetencia.ContarCompetidoresConNombre(competidor.Nombre, competidor.IdCompetencia) > 0)
            {
                throw new Exception("Ya existe un competidor con ese nombre");
            }

            dalCompetencia.ModificarCompetidor(competidor);

            RegistrarEvento(nombreUsuarioEnSesion, "Modificacion de competidor", competidor.Nombre);
        }

        #endregion

        #region Baja

        /// <summary>
        /// Publicaciones mapeadas activas que se pausarian si se diera de baja
        /// al competidor. Se usa para advertir antes de confirmar la baja.
        /// </summary>
        public int ContarPublicacionesActivasPorCompetidor(int idCompetencia)
        {
            ValidarCodigo(idCompetencia);

            return dalCompetencia.ContarPublicacionesActivasPorCompetidor(idCompetencia);
        }

        public void CambiarEstadoCompetidor(int idCompetencia, bool activo, string nombreUsuarioEnSesion)
        {
            ValidarCodigo(idCompetencia);

            BECompetencia competidor = dalCompetencia.TraerCompetidorPorId(idCompetencia);

            if (competidor == null)
            {
                throw new Exception("No se encontro el competidor seleccionado");
            }

            if (competidor.Activo == activo)
            {
                throw new Exception(activo
                    ? "El competidor ya se encuentra activo"
                    : "El competidor ya se encuentra dado de baja");
            }

            dalCompetencia.ModificarEstado(idCompetencia, activo);

            RegistrarEvento(
                nombreUsuarioEnSesion,
                activo ? "Reactivacion de competidor" : "Baja de competidor",
                competidor.Nombre);
        }

        #endregion

        #region Validaciones

        private void ValidarDatos(BECompetencia competidor)
        {
            if (competidor == null)
            {
                throw new Exception("No se recibieron los datos del competidor");
            }

            ExigirTexto(competidor.Nombre, "el nombre", 255);
            ExigirTexto(competidor.Marketplace, "el marketplace", 50);

            if (!string.IsNullOrEmpty(competidor.Descripcion) && competidor.Descripcion.Trim().Length > 500)
            {
                throw new Exception("La descripcion supera los 500 caracteres");
            }
        }

        private void ValidarCodigo(int idCompetencia)
        {
            if (idCompetencia <= 0)
            {
                throw new Exception("Debe seleccionar un competidor");
            }
        }

        private void ExigirTexto(string valor, string campo, int largoMaximo)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                throw new Exception(string.Format("Debe completar {0}", campo));
            }

            if (valor.Trim().Length > largoMaximo)
            {
                throw new Exception(string.Format("El valor de {0} supera los {1} caracteres", campo, largoMaximo));
            }
        }

        #endregion

        private void RegistrarEvento(string nombreUsuarioEnSesion, string accion, string nombreCompetidor)
        {
            bllEvento.RegistrarEvento(new Evento(
                nombreUsuarioEnSesion,
                ModuloCompetencia,
                string.Format("{0}: {1}", accion, nombreCompetidor),
                CriticidadCambio));
        }

        #region Mapeo de productos

        public int MapearProducto(int idProducto, int idCompetencia, string url, string nombreUsuarioEnSesion)
        {
            if (idProducto <= 0)
            {
                throw new Exception("Debe seleccionar un producto");
            }

            ValidarCodigo(idCompetencia);
            ValidarUrl(url);

            BECompetencia competidor = dalCompetencia.TraerCompetidorPorId(idCompetencia);

            if (competidor == null)
            {
                throw new Exception("No se encontro el competidor seleccionado");
            }

            if (dalCompetencia.ContarMapeosProductoCompetidor(idProducto, idCompetencia) > 0)
            {
                throw new Exception("Ya existe un mapeo para ese producto con ese competidor");
            }

            int idProductoCompetencia = dalCompetencia.MapearProducto(idProducto, idCompetencia, url.Trim());

            RegistrarEvento(nombreUsuarioEnSesion, "Mapeo de producto con competencia", competidor.Nombre);

            return idProductoCompetencia;
        }

        private void ValidarUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new Exception("Debe ingresar la URL de la publicacion");
            }

            Uri direccion;

            if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out direccion)
                || (direccion.Scheme != Uri.UriSchemeHttp && direccion.Scheme != Uri.UriSchemeHttps))
            {
                throw new Exception("La URL ingresada no tiene un formato valido");
            }

            if (url.Trim().Length > 500)
            {
                throw new Exception("La URL supera los 500 caracteres");
            }
        }

        #endregion

        public List<BEComparacionPrecio> TraerComparacionPrecios()
        {
            return dalCompetencia.TraerComparacionPrecios();
        }

        public List<BEComparacionPrecio> FiltrarComparacionPrecios(int? idProducto, string categoria, int? idCompetencia)
        {
            if (idProducto.HasValue && idProducto.Value <= 0)
            {
                throw new Exception("Debe seleccionar un producto valido");
            }

            if (idCompetencia.HasValue && idCompetencia.Value <= 0)
            {
                throw new Exception("Debe seleccionar un competidor valido");
            }

            return dalCompetencia.FiltrarComparacionPrecios(idProducto, Normalizar(categoria), idCompetencia);
        }

        public BEComparacionPrecio TraerComparacionPrecioPorId(int idProductoCompetencia)
        {
            ValidarMapeo(idProductoCompetencia);

            return dalCompetencia.TraerComparacionPrecioPorId(idProductoCompetencia);
        }

        public List<BEHistorialPrecioCompetencia> TraerHistorialPrecioCompetencia(int idProductoCompetencia)
        {
            ValidarMapeo(idProductoCompetencia);

            return dalCompetencia.TraerHistorialPrecioCompetencia(idProductoCompetencia);
        }

        /// <summary>
        /// Arma los indicadores de la comparacion que se esta viendo. Se calcula
        /// sobre la lista ya consultada para no repetir el filtro en la base.
        /// </summary>
        public BEResumenComparacion CalcularResumen(List<BEComparacionPrecio> comparaciones)
        {
            BEResumenComparacion resumen = new BEResumenComparacion();

            if (comparaciones == null)
            {
                return resumen;
            }

            resumen.PublicacionesMonitoreadas = comparaciones.Count;

            decimal sumaDiferencias = 0;
            int conPrecio = 0;

            foreach (BEComparacionPrecio comparacion in comparaciones)
            {
                if (!comparacion.DiferenciaPorcentaje.HasValue)
                {
                    resumen.SinPrecioRegistrado = resumen.SinPrecioRegistrado + 1;
                    continue;
                }

                decimal diferencia = comparacion.DiferenciaPorcentaje.Value;

                sumaDiferencias = sumaDiferencias + diferencia;
                conPrecio = conPrecio + 1;

                if (diferencia > 0)
                {
                    resumen.ProductosMasCaros = resumen.ProductosMasCaros + 1;
                }
                else if (diferencia < 0)
                {
                    resumen.ProductosMasBaratos = resumen.ProductosMasBaratos + 1;
                }
            }

            // Las publicaciones sin precio relevado no entran en el promedio:
            // no aportan una diferencia y bajarian el valor sin motivo
            resumen.DiferenciaPromedio = conPrecio > 0
                ? Math.Round(sumaDiferencias / conPrecio, 2)
                : 0;

            return resumen;
        }

        private void ValidarMapeo(int idProductoCompetencia)
        {
            if (idProductoCompetencia <= 0)
            {
                throw new Exception("Debe seleccionar una publicacion de la competencia");
            }
        }

        private string Normalizar(string valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }

        #region Monitoreo de publicaciones (CU-007-037)

        public List<BEPublicacionCompetencia> TraerListaPublicacionesMonitoreo()
        {
            return dalCompetencia.TraerListaPublicacionesMonitoreo();
        }

        public List<BEPublicacionCompetencia> FiltrarPublicacionesMonitoreo(int? idProducto, int? idCompetencia, string estado)
        {
            if (idProducto.HasValue && idProducto.Value <= 0)
            {
                throw new Exception("Debe seleccionar un producto valido");
            }

            if (idCompetencia.HasValue && idCompetencia.Value <= 0)
            {
                throw new Exception("Debe seleccionar un competidor valido");
            }

            return dalCompetencia.FiltrarPublicacionesMonitoreo(idProducto, idCompetencia, Normalizar(estado));
        }

        public BEPublicacionCompetencia TraerPublicacionMonitoreoPorId(int idProductoCompetencia)
        {
            ValidarMapeo(idProductoCompetencia);

            return dalCompetencia.TraerPublicacionMonitoreoPorId(idProductoCompetencia);
        }

        /// <summary>
        /// Aplica la accion elegida en el panel de monitoreo (Pausar, Reactivar o
        /// Desactivar) validando que tenga sentido contra el estado actual de la
        /// publicacion. Desactivar es terminal: una vez Finalizada no se puede
        /// volver a mover.
        /// </summary>
        public void CambiarEstadoPublicacion(int idProductoCompetencia, string accion, string nombreUsuarioEnSesion)
        {
            ValidarMapeo(idProductoCompetencia);

            BEPublicacionCompetencia publicacion = dalCompetencia.TraerPublicacionMonitoreoPorId(idProductoCompetencia);

            if (publicacion == null)
            {
                throw new Exception("No se encontro la publicacion seleccionada");
            }

            string nuevoEstado = CalcularNuevoEstado(publicacion.Estado, accion);

            dalCompetencia.ModificarEstadoPublicacion(idProductoCompetencia, nuevoEstado);

            RegistrarEvento(
                nombreUsuarioEnSesion,
                string.Format("{0} de publicacion monitoreada", accion),
                string.Format("{0} - {1}", publicacion.NombreProducto, publicacion.NombreCompetidor));
        }

        private string CalcularNuevoEstado(string estadoActual, string accion)
        {
            switch (accion)
            {
                case "Pausar":
                    if (estadoActual != "Activa")
                    {
                        throw new Exception("Solo se puede pausar una publicacion activa");
                    }

                    return "Pausada";

                case "Reactivar":
                    if (estadoActual == "Activa")
                    {
                        throw new Exception("La publicacion ya esta activa");
                    }

                    if (estadoActual == "Finalizada")
                    {
                        throw new Exception("No se puede reactivar una publicacion desactivada");
                    }

                    return "Activa";

                case "Desactivar":
                    if (estadoActual == "Finalizada")
                    {
                        throw new Exception("La publicacion ya esta desactivada");
                    }

                    return "Finalizada";

                default:
                    throw new Exception("Accion no reconocida");
            }
        }

        #endregion
    }
}
