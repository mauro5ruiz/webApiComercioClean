using Comercio.Domain.Entidades;

namespace Comercio.Domain.Interfaces
{
    public interface IDetalleDevolucionesVentasRepository
    {
        Task<IEnumerable<DetalleDevolucionVenta>> ObtenerPorDevolucion(int idDevolucion);
        Task Insertar(DetalleDevolucionVenta detalle);
    }
}
