using Comercio.Application.Dtos.Categorias;

namespace Comercio.Application.Interfaces
{
    public interface ICategoriasServicio
    {
        Task<IEnumerable<CategoriaDto>> ObtenerTodas();
        Task<CategoriaDto?> ObtenerPorId(int id);
        Task<CategoriaDto> Crear(CrearCategoriaDto dto);
        Task<CategoriaDto> Actualizar(int id, CrearCategoriaDto dto);
        Task Eliminar(int id);
    }
}
