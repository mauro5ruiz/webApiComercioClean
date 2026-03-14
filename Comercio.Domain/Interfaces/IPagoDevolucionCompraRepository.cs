using Comercio.Domain.Entidades;

namespace Comercio.Domain.Interfaces
{
    public interface IPagoDevolucionCompraRepository
    {
        Task<IEnumerable<PagoDevolucionCompra>> ObtenerPorDevolucion(int idDevolucion);

        Task Insertar(PagoDevolucionCompra pago);
    }
}