using Comercio.Application.Dtos.Proveedores;
using Comercio.Domain.Entidades;

namespace Comercio.Application.Interfaces
{
    public interface IProveedoresServicio
    {
        Task<IEnumerable<Proveedor>> ObtenerTodos(bool incluirEliminados = false);
        Task<Proveedor?> ObtenerPorId(int id);

        Task<ProveedorDto> Crear(CrearProveedorDto proveedor);
        Task<ProveedorDto> Actualizar(int id, ActualizarProveedorDto proveedor);

        Task<bool> DarDeBaja(int id);
        Task<bool> Restaurar(int id);

        Task EliminarPermanentemente(int id);
    }
}
