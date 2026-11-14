using DAL;
using Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace BLL
{
    public class BLLEvento
    {
        private const string ModuloAuditoria = "Auditoria";

        // Criticidad de los eventos: 1 informativo, 2 cambio de datos, 3 grave
        private const int CriticidadGrave = 3;

        // La bitacora mantiene los eventos de los ultimos 30 dias: los
        // anteriores se pueden dar de baja en forma logica
        public const int DiasDeMantenimiento = 30;

        private const string FormatoFecha = "yyyy-MM-dd";

        // Largo de la columna Eventos.Evento
        private const int LargoDescripcion = 100;

        private readonly DALEvento dalEvento = new DALEvento();

        public void RegistrarEvento(Evento evento)
        {
            // Eventos.NombreUsuario es FK de Usuario, y el usuario de emergencia no
            // esta en esa tabla: va sin usuario y se aclara en la descripcion
            if (UsuarioEmergencia.EsUsuarioEmergencia(evento.NombreUsuario))
            {
                string descripcion = "Emergencia: " + evento.Descripcion;

                evento.NombreUsuario = null;
                evento.Descripcion = descripcion.Length > LargoDescripcion ? descripcion.Substring(0, LargoDescripcion) : descripcion;
            }

            evento.Fecha = DateTime.Today.ToString(FormatoFecha);
            evento.Hora = DateTime.Now.ToString("HH:mm");
            dalEvento.RegistrarEvento(evento);
        }

        // Solo los eventos que no fueron dados de baja
        public List<Evento> TraerListaEventos()
        {
            return Mapear(dalEvento.TraerListaEventos());
        }

        #region Exportacion a XML - CU-008-038

        // Descarga en XML los eventos que cumplen los filtros de la pantalla.
        // Es una consulta: no deja evento en la bitacora
        public byte[] ExportarEventosXml(string nombreUsuario, string modulo, string descripcion, int? criticidad, DateTime? fechaInicio, DateTime? fechaFin, string nombreUsuarioEnSesion)
        {
            List<Evento> eventos = FiltrarEventos(nombreUsuario, modulo, descripcion, criticidad, fechaInicio, fechaFin);

            // Alternativa [#ERR013]: no se exporta un archivo vacio
            if (eventos.Count == 0)
            {
                throw new Exception(Traductor.T("comun.sinResultadosFiltros"));
            }

            // Alternativa [#ERR020]
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    XmlWriterSettings configuracion = new XmlWriterSettings
                    {
                        Indent = true,
                        Encoding = new UTF8Encoding(false)
                    };

                    using (XmlWriter escritor = XmlWriter.Create(stream, configuracion))
                    {
                        escritor.WriteStartDocument();
                        escritor.WriteStartElement("Bitacora");

                        escritor.WriteStartElement("Encabezado");
                        escritor.WriteElementString("FechaGeneracion", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"));
                        escritor.WriteElementString("GeneradoPor", nombreUsuarioEnSesion);
                        escritor.WriteElementString("CantidadEventos", eventos.Count.ToString());

                        // Los filtros vacios quedan como nodos vacios: significan "todos"
                        escritor.WriteStartElement("Filtros");
                        escritor.WriteElementString("Usuario", Texto(nombreUsuario));
                        escritor.WriteElementString("Modulo", Texto(modulo));
                        escritor.WriteElementString("Evento", Texto(descripcion));
                        escritor.WriteElementString("Criticidad", criticidad.HasValue ? criticidad.Value.ToString() : string.Empty);
                        escritor.WriteElementString("FechaDesde", fechaInicio.HasValue ? fechaInicio.Value.ToString(FormatoFecha) : string.Empty);
                        escritor.WriteElementString("FechaHasta", fechaFin.HasValue ? fechaFin.Value.ToString(FormatoFecha) : string.Empty);
                        escritor.WriteEndElement();

                        escritor.WriteEndElement();

                        EscribirEventos(escritor, eventos);

                        escritor.WriteEndElement();
                        escritor.WriteEndDocument();
                    }

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
            return "Bitacora-" + DateTime.Now.ToString("yyyyMMdd-HHmm") + ".xml";
        }

        private string Texto(string valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? string.Empty : valor.Trim();
        }

        #endregion

        #region Filtrado con XPath - CU-008-038

        // La bitacora se arma como un documento XML y los filtros se aplican
        // con una expresion XPath sobre ese documento
        public List<Evento> FiltrarEventos(string nombreUsuario, string modulo, string descripcion, int? criticidad, DateTime? fechaInicio, DateTime? fechaFin)
        {
            if (criticidad.HasValue && (criticidad.Value < 1 || criticidad.Value > CriticidadGrave))
            {
                throw new Exception(Traductor.T("error.criticidadInvalida"));
            }

            if (fechaInicio.HasValue && fechaFin.HasValue && fechaFin.Value < fechaInicio.Value)
            {
                throw new Exception(Traductor.T("filtroFechas.errorRango"));
            }

            XPathDocument documento = GenerarXml(TraerListaEventos());
            XPathNavigator navegador = documento.CreateNavigator();
            XPathNodeIterator iterador = navegador.Select(ArmarConsulta(nombreUsuario, modulo, descripcion, criticidad, fechaInicio, fechaFin));

            List<Evento> lista = new List<Evento>();

            while (iterador.MoveNext())
            {
                lista.Add(LeerEvento(iterador.Current));
            }

            return lista;
        }

        // <Eventos><Evento><CodEvento>..</CodEvento>...</Evento></Eventos>
        private XPathDocument GenerarXml(List<Evento> eventos)
        {
            StringBuilder xml = new StringBuilder();

            using (XmlWriter escritor = XmlWriter.Create(xml))
            {
                EscribirEventos(escritor, eventos);
            }

            using (StringReader lector = new StringReader(xml.ToString()))
            {
                return new XPathDocument(lector);
            }
        }

        // El nodo <Eventos> es el mismo para el filtrado y para la exportacion
        private void EscribirEventos(XmlWriter escritor, List<Evento> eventos)
        {
            escritor.WriteStartElement("Eventos");

            foreach (Evento evento in eventos)
            {
                escritor.WriteStartElement("Evento");
                escritor.WriteElementString("CodEvento", evento.CodEvento.ToString());
                escritor.WriteElementString("NombreUsuario", evento.NombreUsuario);
                escritor.WriteElementString("Modulo", evento.Modulo);
                escritor.WriteElementString("Descripcion", evento.Descripcion);
                escritor.WriteElementString("Criticidad", evento.Criticidad.ToString());
                escritor.WriteElementString("Fecha", evento.Fecha);
                escritor.WriteElementString("Hora", evento.Hora);
                escritor.WriteEndElement();
            }

            escritor.WriteEndElement();
        }

        // Por ejemplo: /Eventos/Evento[Modulo = 'Sesiones' and Criticidad = 3 and number(translate(Fecha, '-', '')) >= 20261001]
        private string ArmarConsulta(string nombreUsuario, string modulo, string descripcion, int? criticidad, DateTime? fechaInicio, DateTime? fechaFin)
        {
            List<string> condiciones = new List<string>();

            // Usuario y evento buscan texto contenido sin distinguir mayusculas
            if (!string.IsNullOrWhiteSpace(nombreUsuario))
            {
                condiciones.Add("contains(" + Minusculas("NombreUsuario") + ", " + Literal(nombreUsuario.Trim().ToLowerInvariant()) + ")");
            }

            if (!string.IsNullOrWhiteSpace(modulo))
            {
                condiciones.Add("Modulo = " + Literal(modulo.Trim()));
            }

            if (!string.IsNullOrWhiteSpace(descripcion))
            {
                condiciones.Add("contains(" + Minusculas("Descripcion") + ", " + Literal(descripcion.Trim().ToLowerInvariant()) + ")");
            }

            // Ya validada como 1, 2 o 3: entra a la expresion como numero
            if (criticidad.HasValue)
            {
                condiciones.Add("Criticidad = " + criticidad.Value);
            }

            // XPath 1.0 no compara fechas: yyyy-MM-dd sin guiones queda como un numero
            if (fechaInicio.HasValue)
            {
                condiciones.Add("number(translate(Fecha, '-', '')) >= " + fechaInicio.Value.ToString("yyyyMMdd"));
            }

            if (fechaFin.HasValue)
            {
                condiciones.Add("number(translate(Fecha, '-', '')) <= " + fechaFin.Value.ToString("yyyyMMdd"));
            }

            string consulta = "/Eventos/Evento";

            return condiciones.Count == 0
                ? consulta
                : consulta + "[" + string.Join(" and ", condiciones) + "]";
        }

        private string Minusculas(string campo)
        {
            return "translate(" + campo + ", 'ABCDEFGHIJKLMNOPQRSTUVWXYZÁÉÍÓÚÑ', 'abcdefghijklmnopqrstuvwxyzáéíóúñ')";
        }

        // Arma un literal XPath con el texto que escribio el usuario. XPath 1.0
        // no tiene escape de comillas: si trae los dos tipos se arma con concat()
        private string Literal(string valor)
        {
            if (!valor.Contains("'"))
            {
                return "'" + valor + "'";
            }

            if (!valor.Contains("\""))
            {
                return "\"" + valor + "\"";
            }

            return "concat('" + valor.Replace("'", "', \"'\", '") + "')";
        }

        private Evento LeerEvento(XPathNavigator nodo)
        {
            Evento evento = new Evento(
                Valor(nodo, "NombreUsuario"),
                Valor(nodo, "Modulo"),
                Valor(nodo, "Descripcion"),
                Convert.ToInt32(Valor(nodo, "Criticidad")));

            evento.CodEvento = Convert.ToInt64(Valor(nodo, "CodEvento"));
            evento.Fecha = Valor(nodo, "Fecha");
            evento.Hora = Valor(nodo, "Hora");

            return evento;
        }

        private string Valor(XPathNavigator nodo, string campo)
        {
            XPathNavigator hijo = nodo.SelectSingleNode(campo);
            return hijo == null ? string.Empty : hijo.Value;
        }

        #endregion

        #region Baja logica

        // Primer dia que se mantiene en la bitacora
        public DateTime FechaLimiteMantenimiento()
        {
            return DateTime.Today.AddDays(-DiasDeMantenimiento);
        }

        // Cuantos eventos quedarian dados de baja, para pedir la confirmacion
        public int ContarEventosParaBaja()
        {
            string limite = FechaLimiteMantenimiento().ToString(FormatoFecha);
            return TraerListaEventos().Count(evento => string.CompareOrdinal(evento.Fecha, limite) < 0);
        }

        // Da de baja logica los eventos anteriores a los ultimos 30 dias.
        // Quedan en la base (y en los backups) pero no se muestran mas
        public int BajaEventos(string nombreUsuario)
        {
            string limite = FechaLimiteMantenimiento().ToString(FormatoFecha);
            DataTable tabla = dalEvento.BajaEventos(limite);
            int cantidad = tabla.Rows.Count > 0 ? Convert.ToInt32(tabla.Rows[0]["Cantidad"]) : 0;

            if (cantidad > 0)
            {
                RegistrarEvento(new Evento(nombreUsuario, ModuloAuditoria,
                    "Baja de " + cantidad + " eventos anteriores al " + limite, CriticidadGrave));
            }

            return cantidad;
        }

        #endregion

        private List<Evento> Mapear(DataTable tabla)
        {
            List<Evento> lista = new List<Evento>();

            foreach (DataRow row in tabla.Rows)
            {
                Evento evento = new Evento(
                    row["NombreUsuario"] == DBNull.Value ? string.Empty : row["NombreUsuario"].ToString(),
                    row["Modulo"].ToString(),
                    row["Evento"].ToString(),
                    Convert.ToInt32(row["Criticidad"]));

                evento.CodEvento = Convert.ToInt64(row["CodEvento"]);
                evento.Fecha = row["Fecha"].ToString();
                evento.Hora = row["Hora"].ToString();

                lista.Add(evento);
            }

            return lista;
        }
    }
}
