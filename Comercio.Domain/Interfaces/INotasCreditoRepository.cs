using Comercio.Domain.Entidades;

namespace Comercio.Domain.Interfaces
{
    public interface INotasCreditoRepository
    {
        Task<NotaCredito?> ObtenerPorDevolucion(int idDevolucionVenta);
        Task<string> ObtenerSiguienteCodigo();
        Task<int> Insertar(NotaCredito notaCredito);
    }
}
