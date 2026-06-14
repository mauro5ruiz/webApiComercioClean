

namespace Comercio.Application.Dtos.Proveedores
{
    public class ProveedorMovimientoDto
    {
        public string Tipo { get; set; } = string.Empty;
        public int? IdCompra { get; set; }
        public int? IdPago { get; set; }
        public int? IdDevolucionCompra { get; set; }
        public DateTime Fecha { get; set; }
        public string Comprobante { get; set; } = string.Empty;
        public string? FormaPago { get; set; }
        public string? Referencia { get; set; }
        public decimal Importe { get; set; }
        public decimal? SaldoCredito { get; set; }
        public decimal? TotalCompra { get; set; }
        public decimal? PagadoCompra { get; set; }
        public decimal? CreditoAplicadoCompra { get; set; }
        public decimal? SaldoPendienteCompra { get; set; }
    }
}
