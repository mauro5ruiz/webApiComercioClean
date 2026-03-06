using Comercio.Domain.Entidades;

namespace Comercio.Domain.Interfaces
{
    public interface IDetallePerdidasRepository
    {
        Task<DetallePerdida?> ObtenerPorId(int idDetalle);
        Task Insertar(DetallePerdida detalle);
        Task<IEnumerable<DetallePerdida>> ObtenerPorPerdida(int idPerdida);
        Task Eliminar(int idDetalle);
        Task Actualizar(DetallePerdida detalle);
    }
}
