using Comercio.Application.Dtos.Categorias;
using Comercio.Application.Dtos.Marcas;

namespace Comercio.Application.Interfaces
{
    public interface IMarcasServicio
    {
        Task<IEnumerable<MarcaDto>> ObtenerTodas();
        Task<MarcaDto?> ObtenerPorId(int id);
        Task<int> Crear(CrearMarcaDto dto);
        Task<bool> Actualizar(int id, CrearMarcaDto dto);
        Task<bool> Eliminar(int id);
    }
}
