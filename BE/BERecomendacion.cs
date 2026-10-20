using System;

namespace BE
{
    // Recomendacion del CU-002-005. Se genera en el momento a partir de las
    // reglas de la BLL y solo se persiste si el usuario la guarda
    public class BERecomendacion
    {
        public const string TipoStock = "Stock";
        public const string TipoPrecio = "Precio";
        public const string TipoVentas = "Ventas";

        public const string PrioridadAlta = "Alta";
        public const string PrioridadMedia = "Media";
        public const string PrioridadBaja = "Baja";

        public int IdRecomendacion { get; set; }

        // Identifica la fila dentro de la lista generada mientras no se guarda
        public int Numero { get; set; }

        public int IdProducto { get; set; }
        public string CodigoProducto { get; set; }
        public string NombreProducto { get; set; }

        // Regla que la origino (SinStock, StockBajo, PrecioAlto...)
        public string Regla { get; set; }

        public string Tipo { get; set; }
        public string Prioridad { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string Justificacion { get; set; }
        public string AccionSugerida { get; set; }
        public DateTime FechaGeneracion { get; set; }

        // Usuario que la guardo
        public string NombreUsuario { get; set; }

        public bool Guardada { get; set; }

        // Clave de traduccion del tipo, para mostrarlo en pantalla
        public string TipoTexto
        {
            get
            {
                switch (Tipo)
                {
                    case TipoStock:
                        return "recomendaciones.tipo.stock";
                    case TipoPrecio:
                        return "recomendaciones.tipo.precio";
                    case TipoVentas:
                        return "recomendaciones.tipo.ventas";
                    default:
                        return Tipo;
                }
            }
        }

        // Clave de traduccion de la prioridad, para mostrarla en pantalla
        public string PrioridadTexto
        {
            get
            {
                switch (Prioridad)
                {
                    case PrioridadAlta:
                        return "recomendaciones.prioridad.alta";
                    case PrioridadMedia:
                        return "recomendaciones.prioridad.media";
                    case PrioridadBaja:
                        return "recomendaciones.prioridad.baja";
                    default:
                        return Prioridad;
                }
            }
        }

        // Para ordenar la lista: primero las de prioridad alta
        public int OrdenPrioridad
        {
            get
            {
                switch (Prioridad)
                {
                    case PrioridadAlta:
                        return 1;
                    case PrioridadMedia:
                        return 2;
                    default:
                        return 3;
                }
            }
        }

        public string GuardadaTexto
        {
            get { return Guardada ? "recomendaciones.guardada" : "recomendaciones.sinGuardar"; }
        }
    }
}
