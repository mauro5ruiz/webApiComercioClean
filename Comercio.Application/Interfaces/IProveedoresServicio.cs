using Comercio.Application.Dtos.Proveedores;
using Comercio.Domain.Entidades;
using Microsoft.AspNetCore.Mvc;

namespace Comercio.Application.Interfaces
{
    public interface IProveedoresServicio
    {
        Task<IEnumerable<Proveedor>> ObtenerTodos(bool incluirEliminados = false);
        Task<Proveedor?> ObtenerPorId(int id);
        Task<ProveedorDto> Crear(CrearProveedorDto proveedor);
        Task<ProveedorDto> Actualizar(int id, ActualizarProveedorDto proveedor);
        Task<bool> DarDeBaja(int id);
        Task<bool> Restaurar(int id);
        Task<ProveedorCuentaCorrienteDto> ObtenerCuentaCorriente(int idProveedor, DateTime? desde, DateTime? hasta);
        Task EliminarPermanentemente(int id);
        Task PagarProveedor(int idProveedor, decimal importe, int idFormaPago);
    }
}
