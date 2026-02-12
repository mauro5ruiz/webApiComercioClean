using Comercio.Application.Dtos.FormasDePago;

namespace Comercio.Application.Interfaces
{
    public interface IFormasDePagoServicio
    {
        Task<IEnumerable<FormaDePagoDto>> ObtenerTodas();
        Task<FormaDePagoDto?> ObtenerPorId(int id);
        Task<int> Crear(CrearFormaDePagoDto dto);
        Task<bool> Actualizar(int id, CrearFormaDePagoDto dto);
        Task<bool> Eliminar(int id);
    }
}
