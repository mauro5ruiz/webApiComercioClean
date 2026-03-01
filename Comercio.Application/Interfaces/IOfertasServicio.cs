using Comercio.Domain.Entidades;

namespace Comercio.Application.Interfaces
{
    public interface IOfertasServicio
    {
        Task<IEnumerable<Oferta>> ObtenerTodas(bool incluirVencidas = false);
        Task<Oferta?> ObtenerPorId(int id);
        Task<Oferta?> ObtenerOfertaActivaVigente(int productoId);
        Task<int> CrearOferta(Oferta oferta);
        Task ActualizarOferta(Oferta oferta);
        Task DesactivarOfertasPorProducto(int productoId);
        decimal CalcularPrecioFinal(decimal precioBase, Oferta? oferta);
    }
}
