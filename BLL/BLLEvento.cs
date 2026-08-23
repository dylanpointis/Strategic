using DAL;
using Services;
using System;

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
    }
}
