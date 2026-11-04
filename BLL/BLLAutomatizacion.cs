using BE;
using DAL;
using Newtonsoft.Json;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BLL
{
    // CU-003-009 a CU-003-012: consulta, pausa, parametros e historial. La
    // ejecucion de las automatizaciones esta en BLLEjecucionAutomatizaciones
    public class BLLAutomatizacion
    {
        private const string ModuloAutomatizaciones = "Automatizaciones";
        private const int CriticidadCambio = 2;

        // Parametros de cada tipo con su rango permitido
        private static readonly Dictionary<string, BEParametroAutomatizacion[]> ParametrosPorTipo = new Dictionary<string, BEParametroAutomatizacion[]>
        {
            {
                BEAutomatizacion.TipoAjustarPrecio, new[]
                {
                    new BEParametroAutomatizacion { Clave = "umbral", Minimo = 1, Maximo = 100 },
                    new BEParametroAutomatizacion { Clave = "ajuste", Minimo = 1, Maximo = 50 }
                }
            },
            {
                BEAutomatizacion.TipoPausarVenta, new[]
                {
                    new BEParametroAutomatizacion { Clave = "stock", Minimo = 0, Maximo = 1000 }
                }
            },
            {
                BEAutomatizacion.TipoAlertaStock, new[]
                {
                    new BEParametroAutomatizacion { Clave = "margen", Minimo = 0, Maximo = 1000 }
                }
            }
        };

        private readonly DALAutomatizacion dalAutomatizacion = new DALAutomatizacion();
        private readonly BLLEvento bllEvento = new BLLEvento();
        private readonly BLLDigitoVerificador bllDigitoVerificador = new BLLDigitoVerificador();

        // Las estrategias leen sus parametros con este metodo
        public static int LeerParametro(BEAutomatizacion automatizacion, string clave)
        {
            return LeerParametros(automatizacion)[clave];
        }

        private static Dictionary<string, int> LeerParametros(BEAutomatizacion automatizacion)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, int>>(automatizacion.Parametros);
        }

        #region Consulta

        public List<BEAutomatizacion> TraerListaAutomatizaciones()
        {
            List<BEAutomatizacion> automatizaciones = dalAutomatizacion.TraerListaAutomatizaciones();

            foreach (BEAutomatizacion automatizacion in automatizaciones)
            {
                object[] valores = TraerParametros(automatizacion).Select(p => (object)p.Valor).ToArray();

                automatizacion.Condicion = Traductor.T("automatizacion.condicion." + automatizacion.Tipo, valores);
            }

            return automatizaciones;
        }

        public List<BEAutomatizacion> FiltrarAutomatizaciones(List<BEAutomatizacion> automatizaciones, string tipo, string estado)
        {
            return automatizaciones
                .Where(a => string.IsNullOrEmpty(tipo) || a.Tipo == tipo)
                .Where(a => string.IsNullOrEmpty(estado) || a.Estado == estado)
                .ToList();
        }

        public BEAutomatizacion TraerAutomatizacionPorId(int idAutomatizacion)
        {
            return TraerListaAutomatizaciones().Find(a => a.IdAutomatizacion == idAutomatizacion);
        }

        public List<BEParametroAutomatizacion> TraerParametros(BEAutomatizacion automatizacion)
        {
            Dictionary<string, int> valores = LeerParametros(automatizacion);
            List<BEParametroAutomatizacion> parametros = new List<BEParametroAutomatizacion>();

            foreach (BEParametroAutomatizacion parametro in ParametrosPorTipo[automatizacion.Tipo])
            {
                parametros.Add(new BEParametroAutomatizacion
                {
                    Clave = parametro.Clave,
                    Minimo = parametro.Minimo,
                    Maximo = parametro.Maximo,
                    Valor = valores[parametro.Clave]
                });
            }

            return parametros;
        }

        #endregion

        #region Pausar o reactivar - CU-003-010

        public void CambiarEstado(int idAutomatizacion, string nombreUsuarioEnSesion)
        {
            BEAutomatizacion automatizacion = TraerAutomatizacionPorId(idAutomatizacion);

            if (automatizacion == null)
            {
                throw new Exception(Traductor.T("automatizacion.noEncontrada"));
            }

            string estadoNuevo = automatizacion.Activa ? BEAutomatizacion.EstadoPausada : BEAutomatizacion.EstadoActiva;

            try
            {
                dalAutomatizacion.ModificarEstado(idAutomatizacion, estadoNuevo);
                bllDigitoVerificador.PersistirDV("Automatizacion");
            }
            catch (Exception)
            {
                // Alternativa 4.1 del CU [#ERR023]
                throw new Exception(Traductor.T("error.cambioEstadoAutomatizacion"));
            }

            bllEvento.RegistrarEvento(new Evento(
                nombreUsuarioEnSesion,
                ModuloAutomatizaciones,
                string.Format("Automatizacion {0}: {1}", estadoNuevo, automatizacion.Nombre),
                CriticidadCambio));
        }

        #endregion

        #region Modificar parametros - CU-003-011

        public void ValidarParametros(BEAutomatizacion automatizacion, Dictionary<string, int> valores)
        {
            foreach (BEParametroAutomatizacion parametro in ParametrosPorTipo[automatizacion.Tipo])
            {
                if (!valores.ContainsKey(parametro.Clave)
                    || valores[parametro.Clave] < parametro.Minimo
                    || valores[parametro.Clave] > parametro.Maximo)
                {
                    throw new Exception(Traductor.T("automatizacion.parametroFueraDeRango",
                        Traductor.T("automatizacion.parametro." + parametro.Clave),
                        parametro.Minimo,
                        parametro.Maximo));
                }
            }
        }

        public void ModificarParametros(int idAutomatizacion, Dictionary<string, int> valores, string nombreUsuarioEnSesion)
        {
            BEAutomatizacion automatizacion = TraerAutomatizacionPorId(idAutomatizacion);

            if (automatizacion == null)
            {
                throw new Exception(Traductor.T("automatizacion.noEncontrada"));
            }

            ValidarParametros(automatizacion, valores);

            string parametrosJson = JsonConvert.SerializeObject(valores);

            try
            {
                dalAutomatizacion.ModificarParametros(idAutomatizacion, parametrosJson);
                bllDigitoVerificador.PersistirDV("Automatizacion");
            }
            catch (Exception)
            {
                // Alternativa 4.1 del CU [#ERR023]
                throw new Exception(Traductor.T("error.cambioEstadoAutomatizacion"));
            }

            bllEvento.RegistrarEvento(new Evento(
                nombreUsuarioEnSesion,
                ModuloAutomatizaciones,
                string.Format("Parametros modificados: {0} {1}", automatizacion.Nombre, parametrosJson),
                CriticidadCambio));
        }

        #endregion

        #region Historial - CU-003-012

        public List<BEHistorialAutomatizacion> TraerHistorial()
        {
            return dalAutomatizacion.TraerHistorial();
        }

        public List<BEHistorialAutomatizacion> FiltrarHistorial(List<BEHistorialAutomatizacion> historial, int? idAutomatizacion, int? idProducto, string estado, DateTime? fechaDesde, DateTime? fechaHasta)
        {
            if (fechaDesde.HasValue && fechaHasta.HasValue && fechaHasta.Value < fechaDesde.Value)
            {
                throw new Exception(Traductor.T("filtroFechas.errorRango"));
            }

            return historial
                .Where(h => !idAutomatizacion.HasValue || h.IdAutomatizacion == idAutomatizacion.Value)
                .Where(h => !idProducto.HasValue || h.IdProducto == idProducto.Value)
                .Where(h => string.IsNullOrEmpty(estado) || h.Estado == estado)
                .Where(h => !fechaDesde.HasValue || h.FechaEjecucion >= fechaDesde.Value.Date)
                .Where(h => !fechaHasta.HasValue || h.FechaEjecucion < fechaHasta.Value.Date.AddDays(1))
                .ToList();
        }

        #endregion
    }
}
