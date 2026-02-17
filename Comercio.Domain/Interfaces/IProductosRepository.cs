using Comercio.Domain.Entidades;

namespace Comercio.Domain.Interfaces
{
    public interface IProductosRepository
    {
        Task<IEnumerable<Producto>> ObtenerTodos(bool incluirEliminados = false);
        Task<Producto?> ObtenerPorId(int id);
        Task<bool> ExistePorCodigo(string? codigo, int? excluirId = null);
        Task<bool> ExistePorCodigoBarra(string? codigoBarra, int? excluirId = null);
        Task<int> Crear(Producto producto);
        Task<bool> Actualizar(Producto producto);
        Task<bool> DarDeBaja(int id);
        Task<bool> Restaurar(int id);
        Task<bool> EliminarPermanentemente(int id);
        Task<bool> ActualizarPrecioIndividual(int idProducto, decimal valor, string tipoOperacion);
        Task<int> ActualizarPrecios(decimal valor, string tipoOperacion, int? idCategoria = null, int? idMarca = null, bool soloActivos = true);
    }
}
