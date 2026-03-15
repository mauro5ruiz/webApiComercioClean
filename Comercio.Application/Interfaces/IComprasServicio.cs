using Comercio.Domain.Entidades;

namespace Comercio.Application.Interfaces
{
    public interface IComprasServicio
    {
        Task<IEnumerable<Compra>> ObtenerEntreFechas(DateTime desde, DateTime hasta);
        Task<Compra?> ObtenerPorId(int idCompra);
        Task<int> CrearCompra(Compra compra, IEnumerable<DetalleCompra> detalles, IEnumerable<CompraPago>? pagos = null);
        Task AnularCompra(int idCompra);
        Task PagarProveedor(int idProveedor, decimal importe, int idFormaPago);
    }
}
