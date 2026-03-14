using Comercio.Domain.Entidades;

namespace Comercio.Domain.Interfaces
{
    public interface IDevolucionComprasRepository
    {
        Task<IEnumerable<DevolucionCompra>> ObtenerPorFechas(DateTime desde, DateTime hasta);

        Task<DevolucionCompra?> ObtenerPorId(int idDevolucion);

        Task<bool> Existe(int idDevolucion);

        Task<int> Insertar(DevolucionCompra devolucion);

        Task CambiarEstado(int idDevolucion, int estado);

        Task ActualizarTotal(int idDevolucion, decimal total);
    }
}