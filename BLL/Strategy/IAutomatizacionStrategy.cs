using BE;

namespace BLL
{
    // Patron Strategy: cada tipo de automatizacion implementa esta interfaz
    public interface IAutomatizacionStrategy
    {
        // Si el producto cumple la condicion ejecuta la accion y devuelve el
        // resultado; si no la cumple devuelve null
        BEHistorialAutomatizacion Ejecutar(BEAutomatizacion automatizacion, BEProducto producto);
    }
}
