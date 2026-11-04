using BE;
using DAL;
using Services;
using System;
using System.Collections.Generic;

namespace BLL
{
    // Patron Strategy: es el Context. Recorre las automatizaciones activas y
    // ejecuta cada una sobre los productos sin conocer la logica de cada tipo.
    // Por ahora los cambios quedan en la base de Strategic (ver Limitaciones
    // de la solucion en la documentacion)
    public class BLLEjecucionAutomatizaciones
    {
        private const string ModuloAutomatizaciones = "Automatizaciones";
        private const int CriticidadCambio = 2;

        // Tablas que escribe una ejecucion. El DV se persiste una sola vez al final
        private static readonly string[] TablasConDV =
        {
            "Producto",
            "HistorialPrecioProducto",
            "Recomendacion",
            "Automatizacion",
            "HistorialAutomatizacion"
        };

        private IAutomatizacionStrategy estrategia;

        private readonly DALAutomatizacion dalAutomatizacion = new DALAutomatizacion();
        private readonly DALProducto dalProducto = new DALProducto();
        private readonly BLLEvento bllEvento = new BLLEvento();
        private readonly BLLDigitoVerificador bllDigitoVerificador = new BLLDigitoVerificador();

        // Cambia la estrategia que utiliza el Context
        public void SetStrategy(IAutomatizacionStrategy nuevaEstrategia)
        {
            estrategia = nuevaEstrategia;
        }

        // Devuelve lo que se ejecuto; los productos que no cumplen la condicion no figuran
        public List<BEHistorialAutomatizacion> EjecutarAutomatizaciones(string nombreUsuarioEnSesion)
        {
            List<BEHistorialAutomatizacion> resultados = new List<BEHistorialAutomatizacion>();

            try
            {
                foreach (BEAutomatizacion automatizacion in dalAutomatizacion.TraerListaAutomatizaciones())
                {
                    if (!automatizacion.Activa)
                    {
                        continue;
                    }

                    SetStrategy(ObtenerStrategy(automatizacion.Tipo));

                    // Se traen en cada vuelta para ver los cambios de la automatizacion anterior
                    foreach (BEProducto producto in dalProducto.TraerListaProductos())
                    {
                        BEHistorialAutomatizacion resultado = EjecutarSobreProducto(automatizacion, producto);

                        if (resultado != null)
                        {
                            resultado.IdAutomatizacion = automatizacion.IdAutomatizacion;
                            resultado.IdProducto = producto.IdProducto;

                            dalAutomatizacion.RegistrarHistorial(resultado);
                            resultados.Add(resultado);
                        }
                    }

                    dalAutomatizacion.ActualizarFechaUltimaEjecucion(automatizacion.IdAutomatizacion);
                }
            }
            finally
            {
                foreach (string tabla in TablasConDV)
                {
                    bllDigitoVerificador.PersistirDV(tabla);
                }
            }

            bllEvento.RegistrarEvento(new Evento(
                nombreUsuarioEnSesion,
                ModuloAutomatizaciones,
                string.Format("Automatizaciones ejecutadas: {0} acciones", resultados.Count),
                CriticidadCambio));

            return resultados;
        }

        // Un error en un producto queda en el historial y no corta la ejecucion
        private BEHistorialAutomatizacion EjecutarSobreProducto(BEAutomatizacion automatizacion, BEProducto producto)
        {
            try
            {
                return estrategia.Ejecutar(automatizacion, producto);
            }
            catch (Exception ex)
            {
                return new BEHistorialAutomatizacion
                {
                    AccionEjecutada = ex.Message,
                    Estado = BEHistorialAutomatizacion.EstadoConError
                };
            }
        }

        private IAutomatizacionStrategy ObtenerStrategy(string tipo)
        {
            switch (tipo)
            {
                case BEAutomatizacion.TipoAjustarPrecio:
                    return new AjustarPrecioStrategy();
                case BEAutomatizacion.TipoPausarVenta:
                    return new PausarVentaStrategy();
                case BEAutomatizacion.TipoAlertaStock:
                    return new AlertaStockStrategy();
                default:
                    throw new Exception(Traductor.T("automatizacion.tipoDesconocido", tipo));
            }
        }
    }
}
