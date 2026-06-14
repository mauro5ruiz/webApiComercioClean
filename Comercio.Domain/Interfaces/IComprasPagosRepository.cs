using Comercio.Domain.Entidades;

namespace Comercio.Domain.Interfaces
{
    public interface IComprasPagosRepository
    {
        Task Insertar(CompraPago pago);
        Task<IEnumerable<CompraPago>> ObtenerPorCompra(int idCompra);
        Task RecalcularTotalPagado(int idCompra);
        Task RecalcularTotalPagado(int idCompra, decimal creditoAplicado);
        Task CambiarEstado(int idPago, int estado);
    }
}
