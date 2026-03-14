using Comercio.Domain.Entidades;

namespace Comercio.Domain.Interfaces
{
    public interface ICreditoProveedorRepository
    {
        Task<IEnumerable<CreditoProveedor>> ObtenerPorProveedor(int idProveedor);

        Task Insertar(CreditoProveedor credito);

        Task ConsumirCredito(int idCredito, decimal importe);
    }
}