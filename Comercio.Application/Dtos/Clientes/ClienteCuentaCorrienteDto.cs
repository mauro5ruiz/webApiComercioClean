namespace Comercio.Application.Dtos.Clientes
{
    public class ClienteCuentaCorrienteDto
    {
        public int IdCliente { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public decimal SaldoTotalPendiente { get; set; }
        public decimal CreditoDisponible { get; set; }
        public decimal SaldoNeto { get; set; }
        public decimal TotalVendido { get; set; }
        public decimal TotalCobrado { get; set; }
        public List<ClienteVentaPendienteDto> VentasPendientes { get; set; } = new();
        public List<ClienteMovimientoDto> Movimientos { get; set; } = new();
    }
}
