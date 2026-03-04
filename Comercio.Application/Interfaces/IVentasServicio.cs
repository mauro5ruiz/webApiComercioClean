using Comercio.Domain.Entidades;

namespace Comercio.Application.Interfaces
{
    public interface IVentasServicio
    {
        Task<IEnumerable<Venta>> ObtenerEntreFechas(DateTime desde, DateTime hasta);
        Task<Venta?> ObtenerPorId(int idVenta);
        Task<int> CrearVenta(Venta venta, IEnumerable<DetalleVenta> detalles, IEnumerable<VentaPago>? pagos = null);
        Task AnularVenta(int idVenta);
    }
}
