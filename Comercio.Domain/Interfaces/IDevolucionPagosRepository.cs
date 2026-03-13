using Comercio.Domain.Entidades;

namespace Comercio.Domain.Interfaces
{
    public interface IDevolucionPagosRepository
    {
        Task<IEnumerable<DevolucionVentaPago>> ObtenerPorDevolucion(int idDevolucion);
        Task Insertar(DevolucionVentaPago pago);
        Task CambiarEstado(int idPago, string estado);
    }
}
