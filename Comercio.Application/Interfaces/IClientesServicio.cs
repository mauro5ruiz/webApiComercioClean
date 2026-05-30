using Comercio.Application.Dtos.Clientes;
using Comercio.Domain.Entidades;

namespace Comercio.Application.Interfaces
{
    public interface IClientesServicio
    {
        Task<IEnumerable<Cliente>> ObtenerTodos(bool incluirEliminados = false);
        Task<Cliente?> ObtenerPorId(int id);

        Task<int> Crear(CrearClienteDto proveedor);
        Task Actualizar(int id, ActualizarClienteDto proveedor);
        Task<ClienteCuentaCorrienteDto> ObtenerCuentaCorriente(int idCliente, DateTime? desde, DateTime? hasta);
        Task CobrarCliente(int idCliente, decimal importe, int idFormaPago, string referencia);

        Task<bool> DarDeBaja(int id);
        Task<bool> Restaurar(int id);

        Task<bool> EliminarPermanentemente(int id);
    }
}
