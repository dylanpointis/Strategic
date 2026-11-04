using BE;
using DAL;

namespace BLL
{
    // Pausa los productos activos cuyo stock llego al limite configurado
    public class PausarVentaStrategy : IAutomatizacionStrategy
    {
        private readonly DALProducto dalProducto = new DALProducto();

        public BEHistorialAutomatizacion Ejecutar(BEAutomatizacion automatizacion, BEProducto producto)
        {
            int limite = BLLAutomatizacion.LeerParametro(automatizacion, "stock");

            if (producto.Estado != "Activo" || producto.Stock > limite)
            {
                return null;
            }

            dalProducto.PausarProducto(producto.IdProducto);

            return new BEHistorialAutomatizacion
            {
                AccionEjecutada = "Venta pausada por falta de stock",
                ValorAnterior = "Activo",
                ValorNuevo = "Pausado",
                Estado = BEHistorialAutomatizacion.EstadoExitosa
            };
        }
    }
}
