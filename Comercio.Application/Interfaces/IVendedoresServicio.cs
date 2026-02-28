using Comercio.Application.Dtos.Vendedores;

namespace Comercio.Application.Interfaces
{
    public interface IVendedoresServicio
    {
        Task<VendedorDto?> ObtenerPorId(int id);
        Task<IEnumerable<VendedorDto>> ObtenerTodos(bool incluirEliminados = false);
        Task<int> Crear(CrearVendedorDto dto);
        Task<bool> Actualizar(int id, CrearVendedorDto dto);
        Task<bool> DarDeBaja(int id);
        Task<bool> Restaurar(int id);
        Task<bool> EliminarPermanentemente(int id);
    }
}
