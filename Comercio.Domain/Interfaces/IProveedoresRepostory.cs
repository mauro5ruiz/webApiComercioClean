using Comercio.Domain.Entidades;

namespace Comercio.Domain.Interfaces
{
    public interface IProveedoresRepostory
    {
        Task<IEnumerable<Proveedor>> ObtenerTodos(bool incluirEliminados = false);
        Task<Proveedor?> ObtenerPorId(int id);
        Task<bool> ExistePorCuit(string cuit, int? idProveedor = null);
        Task<int> Crear(Proveedor proveedor);
        Task Actualizar(Proveedor proveedor);
        Task EliminarPermanentemente(int id);
        Task<bool> DarDeBaja(int id);
        Task<bool> Restaurar(int id);
    }
}

