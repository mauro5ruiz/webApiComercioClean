using Comercio.Domain.Entidades;

namespace Comercio.Domain.Interfaces
{
    public interface ICategoriasRepository
    {
        Task<IEnumerable<Categoria>> ObtenerTodas();
        Task<Categoria?> ObtenerPorId(int id);
        Task<Categoria?> ObtenerPorNombre(string nombre);
        Task<int> Crear(Categoria categoria);
        Task Actualizar(Categoria categoria);
        Task Eliminar(int id);
    }
}
