using Comercio.Domain.Entidades;

namespace Comercio.Domain.Interfaces
{
    public interface IPerdidasRepository
    {
        Task<IEnumerable<Perdida>> ObtenerPorFechas(DateTime desde, DateTime hasta);

        Task<Perdida?> ObtenerPorId(int idPerdida);

        Task<int> Insertar(Perdida perdida);

        Task Actualizar(int idPerdida, string motivo, string observacion);
    }
}
