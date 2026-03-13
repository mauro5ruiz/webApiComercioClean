using Comercio.Domain.Entidades;

namespace Comercio.Domain.Interfaces
{
    public interface IDevolucionesVentasRepository
    {
        Task<IEnumerable<DevolucionVenta>> ObtenerPorFechas(DateTime desde, DateTime hasta);
        Task<DevolucionVenta?> ObtenerPorId(int id);
        Task<IEnumerable<DevolucionVenta>> ObtenerPorVenta(int idVenta);
        Task<int> Insertar(DevolucionVenta devolucion);
        Task CambiarEstado(int idDevolucion, string estado);
    }
}
