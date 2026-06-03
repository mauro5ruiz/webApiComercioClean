using Comercio.Domain.Entidades;

namespace Comercio.Domain.Interfaces
{
    public interface IDetalleVentasRepostory
    {
        Task<IEnumerable<DetalleVenta>> ObtenerPorVenta(int idVenta);
        Task<int?> ObtenerIdDetalleVenta(int idVenta, int idProducto);
        Task Insertar(DetalleVenta detalle);
        Task AgregarCantidadDevuelto(int idDetalleVenta, int idProducto, int cantidad);
    }
}
