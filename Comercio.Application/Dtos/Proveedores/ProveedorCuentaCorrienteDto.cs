

namespace Comercio.Application.Dtos.Proveedores
{
    public class ProveedorCuentaCorrienteDto
    {
        public int IdProveedor { get; set; }
        public string Proveedor { get; set; } = string.Empty;
        public decimal SaldoTotalPendiente { get; set; }
        public decimal CreditoDisponible { get; set; }
        public decimal SaldoNeto { get; set; }
        public decimal TotalComprado { get; set; }
        public decimal TotalPagado { get; set; }
        public List<ProveedorCompraPendienteDto> ComprasPendientes { get; set; } = new();
        public List<ProveedorMovimientoDto> Movimientos { get; set; } = new();
    }
}
