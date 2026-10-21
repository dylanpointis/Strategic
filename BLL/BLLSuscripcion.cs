using BE;
using DAL;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BLL
{
    public class BLLSuscripcion
    {
        private const string ModuloSuscripciones = "Suscripciones";
        private const int CriticidadCambio = 2;
        private const int CriticidadGrave = 3;

        // Cada renovacion paga un mes de servicio
        private const int MesesPorRenovacion = 1;

        // Datos de la cuenta de Strategic donde el cliente transfiere. Son
        // ficticios: el cobro real se verifica fuera del sistema
        public const string CbuStrategic = "1234567890123456789012";
        public const string AliasStrategic = "STRATEGIC.PAGO.DEMO";
        public const string TitularStrategic = "Strategic S.A.";

        public const string AgrupacionSemana = "Semana";
        public const string AgrupacionMes = "Mes";

        private readonly DALSuscripcion dalSuscripcion = new DALSuscripcion();
        private readonly BLLUsuario bllUsuario = new BLLUsuario();
        private readonly BLLEvento bllEvento = new BLLEvento();
        private readonly BLLDigitoVerificador bllDigitoVerificador = new BLLDigitoVerificador();

        #region Consulta

        // Historial completo, del periodo mas reciente al mas viejo, con los
        // numeros de operacion desencriptados
        public List<BESuscripcion> TraerListaSuscripciones()
        {
            ActualizarVencidas();

            return Desencriptar(dalSuscripcion.TraerListaSuscripciones());
        }

        // El periodo mas reciente define el estado actual. Null si nunca se contrato
        public BESuscripcion TraerSuscripcionActual(List<BESuscripcion> suscripciones)
        {
            return suscripciones.Count > 0 ? suscripciones[0] : null;
        }

        // Hay servicio mientras el ultimo periodo pagado no haya terminado. Una
        // suscripcion cancelada sigue andando hasta su vencimiento (CU-009-043)
        public bool TieneServicioActivo()
        {
            BESuscripcion actual = TraerSuscripcionActual(TraerListaSuscripciones());

            return actual != null && actual.FechaVencimiento > DateTime.Now;
        }

        #endregion

        #region Renovacion - CU-009-042

        // Arma el periodo que se pagaria al renovar, sin guardarlo: mismo plan
        // y monto que el actual. Si todavia no vencio, el periodo nuevo arranca
        // cuando termina el actual para no perder dias ya pagados
        public BESuscripcion ArmarRenovacion(List<BESuscripcion> suscripciones)
        {
            BESuscripcion actual = TraerSuscripcionActual(suscripciones);

            // Alternativa 2.1 [#ERR045]
            if (actual == null)
            {
                throw new Exception(Traductor.T("error.sinSuscripcion"));
            }

            DateTime ahora = DateTime.Now;
            DateTime inicio = actual.FechaVencimiento > ahora ? actual.FechaVencimiento : ahora;

            return new BESuscripcion
            {
                FechaPago = ahora,
                FechaInicio = inicio,
                FechaVencimiento = inicio.AddMonths(MesesPorRenovacion),
                Estado = "Activa",
                Plan = actual.Plan,
                Monto = actual.Monto
            };
        }

        // Registra el pago y extiende el vencimiento en el momento. Strategic
        // verifica despues, por fuera del sistema, que la transferencia llego
        public BESuscripcion RenovarSuscripcion(string numeroOperacion, string nombreUsuarioEnSesion)
        {
            numeroOperacion = ValidarNumeroOperacion(numeroOperacion);

            List<BESuscripcion> suscripciones = TraerListaSuscripciones();
            BESuscripcion renovacion = ArmarRenovacion(suscripciones);

            // Alternativa 6.2 [#ERR050]. Se compara contra los numeros ya
            // desencriptados: con AES el mismo numero no se encripta igual dos veces
            if (suscripciones.Any(s => string.Equals(s.NumeroOperacion, numeroOperacion, StringComparison.OrdinalIgnoreCase)))
            {
                throw new Exception(Traductor.T("error.operacionDuplicada"));
            }

            BESuscripcion aGuardar = new BESuscripcion
            {
                FechaPago = renovacion.FechaPago,
                FechaInicio = renovacion.FechaInicio,
                FechaVencimiento = renovacion.FechaVencimiento,
                Plan = renovacion.Plan,
                Monto = renovacion.Monto,
                NumeroOperacion = Encriptacion.EncriptarAES(numeroOperacion)
            };

            // Alternativa 6.3 [#ERR046]: el SP corre en una sola sentencia, si
            // falla no queda nada guardado
            try
            {
                renovacion.IdSuscripcion = dalSuscripcion.RegistrarSuscripcion(aGuardar);
            }
            catch (Exception)
            {
                throw new Exception(Traductor.T("error.renovacionNoRegistrada"));
            }

            bllDigitoVerificador.PersistirDV("Suscripcion");

            bllEvento.RegistrarEvento(new Evento(
                nombreUsuarioEnSesion,
                ModuloSuscripciones,
                string.Format("Renovacion de suscripcion: plan {0} hasta {1:dd/MM/yyyy}", renovacion.Plan, renovacion.FechaVencimiento),
                CriticidadCambio));

            // Pendiente: enviar mail al usuario y al equipo de Strategic avisando
            // la renovacion (CU-009-042, paso 6; MailService.EnviarMailRenovacionSuscripcion)

            renovacion.NumeroOperacion = numeroOperacion;
            return renovacion;
        }

        // Numero de operacion del comprobante del banco o de MercadoPago:
        // letras y numeros, sin espacios
        private string ValidarNumeroOperacion(string numeroOperacion)
        {
            // Alternativa 6.1 [#ERR001]
            if (string.IsNullOrWhiteSpace(numeroOperacion))
            {
                throw new Exception(Traductor.T("error.debeCompletar", Traductor.T("campo.elNumeroOperacion")));
            }

            string numero = numeroOperacion.Trim();

            if (!Regex.IsMatch(numero, "^[A-Za-z0-9-]{4,30}$"))
            {
                throw new Exception(Traductor.T("error.numeroOperacionInvalido"));
            }

            return numero;
        }

        #endregion

        #region Baja - CU-009-043

        // Fecha hasta la que el cliente sigue teniendo acceso si se da de baja:
        // el ultimo vencimiento entre los periodos activos
        public DateTime? CalcularFinDeAcceso(List<BESuscripcion> suscripciones)
        {
            List<BESuscripcion> activas = suscripciones.Where(s => s.Activa).ToList();

            if (activas.Count == 0)
            {
                return null;
            }

            return activas.Max(s => s.FechaVencimiento);
        }

        // Valida que se pueda dar de baja la suscripcion actual
        public void ValidarBaja(List<BESuscripcion> suscripciones)
        {
            BESuscripcion actual = TraerSuscripcionActual(suscripciones);

            if (actual == null)
            {
                throw new Exception(Traductor.T("error.sinSuscripcion"));
            }

            // Alternativa 2.1 [#ERR047]
            if (actual.Cancelada)
            {
                throw new Exception(Traductor.T("error.suscripcionYaCancelada"));
            }

            if (!actual.Activa)
            {
                throw new Exception(Traductor.T("error.suscripcionSinPeriodoActivo"));
            }
        }

        // Devuelve la fecha hasta la que se mantiene el acceso
        public DateTime CancelarSuscripcion(string clave, string nombreUsuarioEnSesion)
        {
            // Alternativa 4.1 [#ERR048]
            if (string.IsNullOrEmpty(clave) || !bllUsuario.ValidarClave(nombreUsuarioEnSesion, clave))
            {
                throw new Exception(Traductor.T("error.claveVerificacionIncorrecta"));
            }

            List<BESuscripcion> suscripciones = TraerListaSuscripciones();
            ValidarBaja(suscripciones);

            DateTime finDeAcceso = CalcularFinDeAcceso(suscripciones).Value;

            // Alternativa 4.2 [#ERR049]
            try
            {
                dalSuscripcion.CancelarSuscripcion();
            }
            catch (Exception)
            {
                throw new Exception(Traductor.T("error.bajaNoRegistrada"));
            }

            bllDigitoVerificador.PersistirDV("Suscripcion");

            // Grave: el cliente deja de tener servicio al vencimiento
            bllEvento.RegistrarEvento(new Evento(
                nombreUsuarioEnSesion,
                ModuloSuscripciones,
                string.Format("Baja de suscripcion: acceso hasta {0:dd/MM/yyyy}", finDeAcceso),
                CriticidadGrave));

            // Pendiente: enviar mail al usuario y al equipo de Strategic avisando
            // la baja (CU-009-043, paso 4; MailService.EnviarMailBajaSuscripcion)

            return finDeAcceso;
        }

        #endregion

        #region Reporte de facturacion

        public List<BESuscripcion> FiltrarPagos(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            if (fechaDesde.HasValue && fechaHasta.HasValue && fechaHasta.Value < fechaDesde.Value)
            {
                throw new Exception(Traductor.T("filtroFechas.errorRango"));
            }

            ActualizarVencidas();

            return Desencriptar(dalSuscripcion.FiltrarSuscripcionesPorFechaPago(
                fechaDesde.HasValue ? fechaDesde.Value.Date : (DateTime?)null,
                fechaHasta.HasValue ? fechaHasta.Value.Date : (DateTime?)null));
        }

        public decimal CalcularFacturacionTotal(List<BESuscripcion> pagos)
        {
            decimal total = 0;

            foreach (BESuscripcion pago in pagos)
            {
                total = total + pago.Monto;
            }

            return total;
        }

        public decimal CalcularPagoPromedio(List<BESuscripcion> pagos)
        {
            if (pagos.Count == 0)
            {
                return 0;
            }

            return CalcularFacturacionTotal(pagos) / pagos.Count;
        }

        // Suma lo cobrado por semana (de lunes a domingo) o por mes. La clave
        // es el primer dia del periodo. Los periodos sin cobros entre el primero
        // y el ultimo van en cero para que el grafico muestre los huecos
        public List<KeyValuePair<DateTime, decimal>> AgruparFacturacion(List<BESuscripcion> pagos, string agrupacion)
        {
            bool porSemana = agrupacion == AgrupacionSemana;
            Dictionary<DateTime, decimal> acumulado = new Dictionary<DateTime, decimal>();

            foreach (BESuscripcion pago in pagos)
            {
                DateTime periodo = InicioDePeriodo(pago.FechaPago, porSemana);

                if (acumulado.ContainsKey(periodo))
                {
                    acumulado[periodo] = acumulado[periodo] + pago.Monto;
                }
                else
                {
                    acumulado.Add(periodo, pago.Monto);
                }
            }

            List<KeyValuePair<DateTime, decimal>> facturacion = new List<KeyValuePair<DateTime, decimal>>();

            if (acumulado.Count == 0)
            {
                return facturacion;
            }

            DateTime desde = acumulado.Keys.Min();
            DateTime hasta = acumulado.Keys.Max();

            for (DateTime periodo = desde; periodo <= hasta; periodo = porSemana ? periodo.AddDays(7) : periodo.AddMonths(1))
            {
                decimal monto;
                acumulado.TryGetValue(periodo, out monto);

                facturacion.Add(new KeyValuePair<DateTime, decimal>(periodo, monto));
            }

            return facturacion;
        }

        private DateTime InicioDePeriodo(DateTime fecha, bool porSemana)
        {
            if (porSemana)
            {
                int diasDesdeElLunes = ((int)fecha.DayOfWeek + 6) % 7;
                return fecha.Date.AddDays(-diasDesdeElLunes);
            }

            return new DateTime(fecha.Year, fecha.Month, 1);
        }

        #endregion

        // Los periodos activos que ya terminaron pasan a Vencida. Lo hace el
        // sistema al consultar, por eso no deja evento en la bitacora
        private void ActualizarVencidas()
        {
            if (dalSuscripcion.ActualizarSuscripcionesVencidas() > 0)
            {
                bllDigitoVerificador.PersistirDV("Suscripcion");
            }
        }

        private List<BESuscripcion> Desencriptar(List<BESuscripcion> suscripciones)
        {
            foreach (BESuscripcion suscripcion in suscripciones)
            {
                if (!string.IsNullOrEmpty(suscripcion.NumeroOperacion))
                {
                    suscripcion.NumeroOperacion = Encriptacion.DesencriptarAES(suscripcion.NumeroOperacion);
                }
            }

            return suscripciones;
        }
    }
}
