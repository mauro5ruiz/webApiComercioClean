using Comercio.Application.Dtos.Sucursales;

namespace Comercio.Application.Interfaces
{
    public interface ISucursalesServicio
    {
        Task<IEnumerable<SucursalDto>> ObtenerTodas();
        Task<SucursalDto?> ObtenerPorId(int id);
        Task<int> Crear(CrearSucursalDto dto);
        Task<bool> Actualizar(int id, CrearSucursalDto dto);
        Task<bool> Eliminar(int id);
        Task<bool> DarDeBaja(int id);
        Task<bool> Restaurar(int id);
    }
}
