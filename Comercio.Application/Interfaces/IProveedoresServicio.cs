using Comercio.Application.Dtos.Proveedores;
using Comercio.Domain.Entidades;

namespace Comercio.Application.Interfaces
{
    public interface IProveedoresServicio
    {
        Task<IEnumerable<Proveedor>> ObtenerTodos(bool incluirEliminados = false);
        Task<Proveedor?> ObtenerPorId(int id);

        Task<int> Crear(CrearProveedorDto proveedor);
        Task Actualizar(int id, ActualizarProveedorDto proveedor);

        Task<bool> DarDeBaja(int id);
        Task<bool> Restaurar(int id);

        Task<bool> EliminarPermanentemente(int id);
    }
}
