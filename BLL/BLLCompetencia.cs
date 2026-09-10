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
    }
}
