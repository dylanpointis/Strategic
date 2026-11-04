using System;

namespace BE
{
    // Automatizacion configurada: un tipo del catalogo con sus parametros
    public class BEAutomatizacion
    {
        public const string EstadoActiva = "Activa";
        public const string EstadoPausada = "Pausada";

        public const string TipoAjustarPrecio = "AjustarPrecio";
        public const string TipoPausarVenta = "PausarVenta";
        public const string TipoAlertaStock = "AlertaStock";

        public int IdAutomatizacion { get; set; }
        public int IdCatalogoAutomatizacion { get; set; }
        public string Nombre { get; set; }
        public string Tipo { get; set; }
        public string Descripcion { get; set; }

        // JSON con los valores que eligio el usuario, por ejemplo {"umbral":15,"ajuste":5}
        public string Parametros { get; set; }

        public string Estado { get; set; }
        public DateTime FechaAlta { get; set; }
        public DateTime? FechaUltimaEjecucion { get; set; }

        // Texto de la condicion con los parametros actuales. Lo arma la BLL
        public string Condicion { get; set; }

        public bool Activa
        {
            get { return Estado == EstadoActiva; }
        }

        // Clave de traduccion del estado, para mostrarlo en pantalla
        public string EstadoTexto
        {
            get { return "automatizacion.estado." + Estado; }
        }

        // Clave de traduccion de la accion disponible segun el estado
        public string AccionEstado
        {
            get { return Activa ? "comun.accion.pausar" : "comun.accion.reactivar"; }
        }
    }
}
