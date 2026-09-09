using DAL;
using Services;
using System;
using System.Collections.Generic;
using System.Data;

namespace BLL
{
    public class BLLEvento
    {
        private readonly DALEvento dalEvento = new DALEvento();

        public void RegistrarEvento(Evento evento)
        {
            evento.Fecha = DateTime.Today.ToString("yyyy-MM-dd");
            evento.Hora = DateTime.Now.ToString("HH:mm");
            dalEvento.RegistrarEvento(evento);
        }

        public List<Evento> TraerListaEventos()
        {
            return Mapear(dalEvento.TraerListaEventos());
        }

        public List<Evento> FiltrarEventos(string nombreUsuario, string modulo, string descripcion, DateTime? fechaInicio, DateTime? fechaFin)
        {
            if (fechaInicio.HasValue && fechaFin.HasValue && fechaFin.Value < fechaInicio.Value)
            {
                throw new Exception("La fecha hasta no puede ser anterior a la fecha desde");
            }

            DataTable tabla = dalEvento.FiltrarEventos(
                Normalizar(nombreUsuario),
                Normalizar(modulo),
                Normalizar(descripcion),
                fechaInicio.HasValue ? fechaInicio.Value.ToString("yyyy-MM-dd") : null,
                fechaFin.HasValue ? fechaFin.Value.ToString("yyyy-MM-dd") : null);

            return Mapear(tabla);
        }

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

        private string Normalizar(string valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }
    }
}
