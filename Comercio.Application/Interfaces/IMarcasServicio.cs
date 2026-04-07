using Comercio.Application.Dtos.Categorias;
using Comercio.Application.Dtos.Marcas;

namespace Comercio.Application.Interfaces
{
    public interface IMarcasServicio
    {
        Task<IEnumerable<MarcaDto>> ObtenerTodas();
        Task<MarcaDto?> ObtenerPorId(int id);
        Task<MarcaDto> Crear(CrearMarcaDto dto);
        Task<MarcaDto> Actualizar(int id, CrearMarcaDto dto);
        Task Eliminar(int id);
    }
}
