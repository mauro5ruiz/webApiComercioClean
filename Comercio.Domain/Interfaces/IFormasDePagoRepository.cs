using Comercio.Domain.Entidades;

namespace Comercio.Domain.Interfaces
{
    public interface IFormasDePagoRepository
    {
        Task<IEnumerable<FormaDePago>> ObtenerTodas();
        Task<FormaDePago?> ObtenerPorId(int id);
        Task<FormaDePago?> ObtenerPorNombre(string nombre);
        Task<int> Crear(FormaDePago categoria);
        Task Actualizar(FormaDePago categoria);
        Task Eliminar(int id);
    }
}
