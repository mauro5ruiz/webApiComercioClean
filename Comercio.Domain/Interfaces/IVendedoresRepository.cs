using Comercio.Domain.Entidades;

namespace Comercio.Domain.Interfaces
{
    public interface IVendedoresRepository
    {
        Task<IEnumerable<Vendedor>> ObtenerTodos(bool incluirEliminados = false);
        Task<Vendedor?> ObtenerPorId(int id);
        Task<bool> ExistePorNroDni(string nroDni, int? idVendedor = null);
        Task<int> Crear(Vendedor vendedor);
        Task Actualizar(Vendedor vendedor);
        Task EliminarPermanentemente(int id);
        Task<bool> DarDeBaja(int id);
        Task<bool> Restaurar(int id);
    }
}
