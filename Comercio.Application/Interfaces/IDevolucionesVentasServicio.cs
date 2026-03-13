using Comercio.Domain.Entidades;

namespace Comercio.Application.Interfaces
{
    public interface IDevolucionesVentasServicio
    {
        Task<int> RegistrarDevolucion(DevolucionVenta devolucion,IEnumerable<DetalleDevolucionVenta> detalles,IEnumerable<DevolucionVentaPago>? pagos = null);
        Task<DevolucionVenta?> ObtenerPorId(int id);
        Task<IEnumerable<DevolucionVenta>> ObtenerEntreFechas(DateTime desde, DateTime hasta);
    }
}
