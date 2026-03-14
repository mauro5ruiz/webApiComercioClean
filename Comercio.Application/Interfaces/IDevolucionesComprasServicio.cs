using Comercio.Domain.Entidades;

namespace Comercio.Application.Interfaces
{
    public interface IDevolucionesComprasServicio
    {
        Task<int> RegistrarDevolucion(DevolucionCompra devolucion,IEnumerable<DevolucionCompraDetalle> detalles,IEnumerable<PagoDevolucionCompra>? pagos = null);

        Task<DevolucionCompra?> ObtenerPorId(int id);

        Task<IEnumerable<DevolucionCompra>> ObtenerEntreFechas(DateTime desde, DateTime hasta);
    }
}