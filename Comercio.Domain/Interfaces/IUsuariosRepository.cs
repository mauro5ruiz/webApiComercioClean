using Comercio.Domain.Entidades;

namespace Comercio.Domain.Interfaces
{
    public interface IUsuariosRepository
    {
        Task<Usuario?> ObtenerPorId(int id);
        Task<Usuario?> ObtenerPorUsuario(string usuarioLogin);
        Task<IEnumerable<Usuario>> ObtenerTodos(bool incluirEliminados = false);
        Task<IEnumerable<Usuario>> ObtenerPorSucursal(int sucursalId);
        Task<bool> ExisteUsuario(string usuarioLogin);
        Task<bool> ExisteEmail(string email);
        Task<int> Crear(Usuario usuario);
        Task<bool> Actualizar(Usuario usuario);
        Task<bool> CambiarClave(int usuarioId, string nuevoHash);
        Task<bool> ActualizarUltimoAcceso(int usuarioId);
        Task<bool> Activar(int usuarioId);
        Task<bool> Desactivar(int usuarioId);
    }
}
