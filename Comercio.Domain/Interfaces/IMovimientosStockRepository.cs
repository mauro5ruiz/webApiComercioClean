using Comercio.Domain.Entidades;

namespace Comercio.Domain.Interfaces
{
    public interface IMovimientosStockRepository
    {
        Task<IEnumerable<MovimientoStock>> ObtenerPorProducto(int idProducto);
        Task RegistrarMovimiento(MovimientoStock movimiento);
        Task<int> ObtenerStockActual(int idProducto);
        Task Actualizar(MovimientoStock movimiento);
        Task Eliminar(int idMovimiento);
    }
}
