using Comercio.Domain.Entidades;

namespace Comercio.Domain.Interfaces
{
    public interface IDetalleVentasRepostory
    {
        Task Insertar(DetalleVenta detalle);
        Task<IEnumerable<DetalleVenta>> ObtenerPorVenta(int idVenta);
    }
}
