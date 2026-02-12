using Comercio.Domain.Entidades;

namespace Comercio.Domain.Interfaces
{
    public interface IMarcasRepository
    {
        Task<IEnumerable<Marca>> ObtenerTodas();
        Task<Marca?> ObtenerPorId(int id);
        Task<Marca?> ObtenerPorNombre(string nombre);
        Task<int> Crear(Marca marca);
        Task Actualizar(Marca marca);
        Task Eliminar(int id);
    }
}
