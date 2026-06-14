using Comercio.Domain.Entidades;

namespace Comercio.Domain.Interfaces
{
    public interface IVentasPagosRepository
    {
        Task Insertar(VentaPago pago);
        Task<IEnumerable<VentaPago>> ObtenerPorVenta(int idVenta);
        Task RecalcularTotalPagado(int idVenta);
        Task RecalcularTotalPagado(int idVenta, decimal creditoAplicado);
        Task CambiarEstado(int idPago, string estado);
    }
}
