using Comercio.Domain.Entidades;

namespace Comercio.Domain.Interfaces
{
    public interface IOfertasRepository
    {
        Task<IEnumerable<Oferta>> ObtenerTodas(bool incluirVencidas = false);
        Task<Oferta?> ObtenerPorId(int id);
        Task<Oferta?> ObtenerOfertaActivaVigente(int productoId);
        Task<IEnumerable<Oferta>> ObtenerPorProducto(int productoId, bool incluirVencidas = false);
        Task<int> Crear(Oferta oferta);
        Task Actualizar(Oferta oferta);
        Task DesactivarOfertasPorProducto(int productoId);
    }
}
