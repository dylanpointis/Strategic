using BE;
using DAL;
using Services;
using System;

namespace BLL
{
    // Se monitorea: se consulta su precio. Se puede pausar o desactivar
    public class PublicacionActiva : EstadoPublicacion
    {
        public override string Nombre
        {
            get { return "Activa"; }
        }

        public override bool Monitorear(PublicacionMonitoreada publicacion)
        {
            BEPublicacionCompetencia datos = publicacion.Datos;

            // Sin una consulta anterior se parte del precio propio del producto
            decimal ultimoPrecio = datos.UltimoPrecio ?? new DALProducto().TraerProductoPorId(datos.IdProducto).Precio;
            decimal precio = new ConsultorPreciosSimulado().ConsultarPrecio(datos.Url, ultimoPrecio);

            new DALCompetencia().RegistrarPrecioCompetencia(datos.IdProductoCompetencia, precio);
            return true;
        }

        public override void Pausar(PublicacionMonitoreada publicacion)
        {
            publicacion.SetEstado(new PublicacionPausada());
        }

        public override void Reactivar(PublicacionMonitoreada publicacion)
        {
            throw new Exception(Traductor.T("error.publicacionYaActiva"));
        }
    }
}
