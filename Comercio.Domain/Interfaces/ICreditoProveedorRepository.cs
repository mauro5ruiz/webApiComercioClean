using Comercio.Domain.Entidades;

namespace Comercio.Domain.Interfaces
{
    public interface ICreditoProveedorRepository
    {
        Task<IEnumerable<CreditoProveedor>> ObtenerPorProveedor(int idProveedor);

        Task Insertar(CreditoProveedor credito);

        Task<bool> ConsumirCredito(int idCredito, decimal importe);
    }
}
