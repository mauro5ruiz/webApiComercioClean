using Comercio.Domain.Entidades;

namespace Comercio.Domain.Interfaces
{
    public interface IDetalleComprasRepository
    {
        Task<IEnumerable<DetalleCompra>> ObtenerPorCompra(int idCompra);
        Task<int?> ObtenerIdDetalleCompra(int idCompra, int idProducto);
        Task Insertar(DetalleCompra detalle);
        Task AgregarCantidadDevuelto(int idDetalleCompra, int idProducto, int cantidad);
    }
}
