using Comercio.Domain.Entidades;

namespace Comercio.Domain.Interfaces
{
    public interface IVentasRepository
    {
        Task<IEnumerable<Venta>> ObtenerPorFechas(DateTime desde, DateTime hasta);
        Task<Venta?> ObtenerPorId(int id);
        Task<Venta?> ObtenerPorNroComprobante(string nroComprobante);
        Task<IEnumerable<Venta>> ObtenerPorCliente(int idCliente, bool soloPendientes = false);
        Task<IEnumerable<Venta>> ObtenerPendientes();
        Task<bool> Existe(int idVenta);

        Task<int> Insertar(Venta venta);
        Task Actualizar(Venta venta);
        Task CambiarEstado(int idVenta, string estado);
    }
}
