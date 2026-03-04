using Comercio.Domain.Entidades;

namespace Comercio.Domain.Interfaces
{
    public interface IDetalleComprasRepository
    {
        Task Insertar(DetalleCompra detalle);
        Task<IEnumerable<DetalleCompra>> ObtenerPorCompra(int idCompra);
    }
}
