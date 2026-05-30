using Comercio.Domain.Entidades;

namespace Comercio.Domain.Interfaces
{
    public interface ICreditoClienteRepository
    {
        Task<IEnumerable<CreditoCliente>> ObtenerPorCliente(int idCliente);
        Task Insertar(CreditoCliente credito);
        Task ConsumirCredito(int idCredito, decimal importe);
    }
}
