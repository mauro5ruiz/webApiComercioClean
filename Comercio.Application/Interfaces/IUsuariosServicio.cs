using Comercio.Application.Dtos.Usuarios;
using Comercio.Domain.Entidades;

namespace Comercio.Application.Interfaces
{
    public interface IUsuariosServicio
    {
        Task<Usuario?> ObtenerPorId(int id);
        Task<IEnumerable<Usuario>> ObtenerTodos(bool incluirEliminados = false);
        Task<IEnumerable<Usuario>> ObtenerPorSucursal(int sucursalId);
        Task<Usuario?> Login(LoginDto dto);
        Task<int> Crear(CrearUsuarioDto dto);
        Task Actualizar(int id, ActualizarUsuarioDto dto);
        Task CambiarClave(CambiarClaveDto dto);
        Task<bool> Activar(int id);
        Task<bool> Desactivar(int id);
    }
}
