using Comercio.Domain.Entidades;

namespace Comercio.Domain.Interfaces
{
    public interface ISucursalesRepository
    {
        Task<IEnumerable<Sucursal>> ObtenerTodas();
        Task<Sucursal?> ObtenerPorId(int id);
        Task<Sucursal?> ObtenerPorNombre(string nombre);
        Task<bool> ExistePorCodigo(string codigo);
        Task<int> Crear(Sucursal sucursal);
        Task Actualizar(Sucursal sucursal);
        Task Eliminar(int id);
        Task<bool> DarDeBaja(int id);
        Task<bool> Restaurar(int id);
    }
}
