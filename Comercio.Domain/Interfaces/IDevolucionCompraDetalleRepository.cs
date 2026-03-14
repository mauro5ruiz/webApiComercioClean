using Comercio.Domain.Entidades;

namespace Comercio.Domain.Interfaces
{
    public interface IDevolucionCompraDetalleRepository
    {
        Task<IEnumerable<DevolucionCompraDetalle>> ObtenerPorDevolucion(int idDevolucion);

        Task Insertar(DevolucionCompraDetalle detalle);
    }
}