using Comercio.Domain.Entidades;

namespace Comercio.Domain.Interfaces
{
    public interface IComprasRepostory
    {
        Task<IEnumerable<Compra>> ObtenerPorFechas(DateTime desde, DateTime hasta);
        Task<Compra?> ObtenerPorId(int idCompra);
        Task<Compra?> ObtenerPorNroComprobante(string nroComprobante);
        Task<bool> Existe(int idVenta);
        Task<int> Insertar(Compra compra);
        Task CambiarEstado(int idCompra, int estado);
        Task ActualizarTotales(int idCompra, decimal total, decimal totalPagado, decimal saldoPendiente);
    }
}
