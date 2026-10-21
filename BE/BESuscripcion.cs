using System;

namespace BE
{
    // Un periodo pagado de la suscripcion del cliente a Strategic. La tabla
    // guarda el historial: cada renovacion es una fila nueva
    public class BESuscripcion
    {
        public int IdSuscripcion { get; set; }
        public DateTime FechaPago { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public string Estado { get; set; }
        public string Plan { get; set; }
        public decimal Monto { get; set; }

        // En la base va encriptado con AES; la BLL lo deja en claro
        public string NumeroOperacion { get; set; }

        public bool Activa
        {
            get { return Estado == "Activa"; }
        }

        public bool Cancelada
        {
            get { return Estado == "Cancelada"; }
        }

        public string Periodo
        {
            get { return string.Format("{0:dd/MM/yyyy} - {1:dd/MM/yyyy}", FechaInicio, FechaVencimiento); }
        }

        // Clave de traduccion del estado, para mostrarlo en pantalla
        public string EstadoTexto
        {
            get { return "suscripcion.estado." + (Estado ?? string.Empty).ToLowerInvariant(); }
        }

        // El cliente ve solo los ultimos 4 caracteres de sus comprobantes
        public string NumeroOperacionEnmascarado
        {
            get
            {
                if (string.IsNullOrEmpty(NumeroOperacion))
                {
                    return string.Empty;
                }

                if (NumeroOperacion.Length <= 4)
                {
                    return NumeroOperacion;
                }

                return "****" + NumeroOperacion.Substring(NumeroOperacion.Length - 4);
            }
        }
    }
}
