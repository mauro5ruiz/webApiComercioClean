using Comercio.Domain.Entidades;

namespace Comercio.Application.Interfaces
{
    public interface IPerdidasServicio
    {
        Task<IEnumerable<Perdida>> ObtenerPorFechas(DateTime desde, DateTime hasta);
        Task<Perdida?> ObtenerPorId(int idPerdida);
        Task<Perdida> ObtenerPerdidaCompleta(int idPerdida);
        Task<int> CrearPerdida(Perdida perdida, List<DetallePerdida> detalles);
        Task AgregarDetalle(DetallePerdida detalle);
        Task ActualizarDetalle(DetallePerdida detalle);
        Task EliminarDetalle(int idDetalle);
        Task ActualizarPerdida(int idPerdida, string motivo, string observacion);
    }
}
