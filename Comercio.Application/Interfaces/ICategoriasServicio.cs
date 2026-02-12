using Comercio.Application.Dtos.Categorias;

namespace Comercio.Application.Interfaces
{
    public interface ICategoriasServicio
    {
        Task<IEnumerable<CategoriaDto>> ObtenerTodas();
        Task<CategoriaDto?> ObtenerPorId(int id);
        Task<int> Crear(CrearCategoriaDto dto);
        Task<bool> Actualizar(int id, CrearCategoriaDto dto);
        Task<bool> Eliminar(int id);
    }
}
