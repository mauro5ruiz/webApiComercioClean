using Comercio.Application.Dtos.FormasDePago;

namespace Comercio.Application.Interfaces
{
    public interface IFormasDePagoServicio
    {
        Task<IEnumerable<FormaDePagoDto>> ObtenerTodas();
        Task<FormaDePagoDto?> ObtenerPorId(int id);
        Task<FormaDePagoDto> Crear(CrearFormaDePagoDto dto);
        Task<FormaDePagoDto> Actualizar(int id, CrearFormaDePagoDto dto);
        Task Eliminar(int id);
    }
}
