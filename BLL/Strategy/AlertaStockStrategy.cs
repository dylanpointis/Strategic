using BE;
using DAL;
using Services;
using System;

namespace BLL
{
    // Si el stock llego al minimo mas el "margen" configurado, guarda una
    // recomendacion y avisa por mail al WebMaster
    public class AlertaStockStrategy : IAutomatizacionStrategy
    {
        private const string Regla = "AlertaStock";
        private const string RolWebMaster = "WebMaster";

        // Las guarda el sistema, no un usuario
        private const string UsuarioAutomatizacion = "Automatizaciones";

        private readonly DALRecomendacion dalRecomendacion = new DALRecomendacion();
        private readonly BLLUsuario bllUsuario = new BLLUsuario();

        public BEHistorialAutomatizacion Ejecutar(BEAutomatizacion automatizacion, BEProducto producto)
        {
            if (producto.Estado == "Inactivo" || !producto.StockMinimo.HasValue)
            {
                return null;
            }

            int margen = BLLAutomatizacion.LeerParametro(automatizacion, "margen");

            if (producto.Stock > producto.StockMinimo.Value + margen)
            {
                return null;
            }

            // Por producto se avisa una sola vez por dia
            if (dalRecomendacion.ContarRecomendacionesGuardadas(producto.IdProducto, Regla, DateTime.Today) > 0)
            {
                return null;
            }

            dalRecomendacion.RegistrarRecomendacion(CrearRecomendacion(producto));

            string valorNuevo = string.Format("Stock: {0} (mínimo: {1})", producto.Stock, producto.StockMinimo.Value);

            try
            {
                EnviarAviso(producto);
            }
            catch (Exception)
            {
                return new BEHistorialAutomatizacion
                {
                    AccionEjecutada = "Recomendación generada, pero no se pudo enviar el aviso por mail",
                    ValorNuevo = valorNuevo,
                    Estado = BEHistorialAutomatizacion.EstadoConError
                };
            }

            return new BEHistorialAutomatizacion
            {
                AccionEjecutada = "Recomendación generada y aviso enviado al WebMaster",
                ValorNuevo = valorNuevo,
                Estado = BEHistorialAutomatizacion.EstadoExitosa
            };
        }

        private BERecomendacion CrearRecomendacion(BEProducto producto)
        {
            return new BERecomendacion
            {
                IdProducto = producto.IdProducto,
                Regla = Regla,
                Tipo = BERecomendacion.TipoStock,
                Prioridad = BERecomendacion.PrioridadMedia,
                Titulo = Traductor.T("automatizacion.alerta.titulo"),
                Descripcion = Traductor.T("automatizacion.alerta.descripcion", producto.Nombre, producto.Stock, producto.StockMinimo.Value),
                Justificacion = Traductor.T("recomendaciones.stockBajo.justificacion", producto.Stock, producto.StockMinimo.Value),
                AccionSugerida = Traductor.T("recomendaciones.stockBajo.accionSinMaximo", producto.StockMinimo.Value),
                FechaGeneracion = DateTime.Now,
                NombreUsuario = UsuarioAutomatizacion
            };
        }

        private void EnviarAviso(BEProducto producto)
        {
            foreach (BEUsuario usuario in bllUsuario.TraerListaUsuarios())
            {
                if (usuario.NombreRol == RolWebMaster && usuario.Activo)
                {
                    MailService.EnviarMail(usuario.Email,
                        Traductor.T("mail.alertaStock.asunto", producto.Nombre),
                        Traductor.T("mail.alertaStock.cuerpo", producto.Nombre, producto.Codigo, producto.Stock, producto.StockMinimo.Value),
                        null,
                        null);
                }
            }
        }
    }
}
