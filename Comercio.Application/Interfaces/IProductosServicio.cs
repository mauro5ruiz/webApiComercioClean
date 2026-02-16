using Comercio.Application.Dtos.Productos;
using Comercio.Domain.Entidades;

namespace Comercio.Application.Interfaces
{
    public interface IProductosServicio
    {
        Task<IEnumerable<Producto>> ObtenerTodos(bool incluirEliminados = false);
        Task<Producto?> ObtenerPorId(int id);

        Task<int> Crear(CrearProductoDto producto);
        Task Actualizar(int id, ActualizarProductoDto producto);

        Task<bool> DarDeBaja(int id);
        Task<bool> Restaurar(int id);

        Task<bool> EliminarPermanentemente(int id);
    }
}
