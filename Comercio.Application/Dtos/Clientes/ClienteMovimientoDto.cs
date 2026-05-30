namespace Comercio.Application.Dtos.Clientes
{
    public class ClienteMovimientoDto
    {
        public string Tipo { get; set; } = string.Empty;
        public int? IdVenta { get; set; }
        public int? IdPago { get; set; }
        public int? IdDevolucionVenta { get; set; }
        public DateTime Fecha { get; set; }
        public string Comprobante { get; set; } = string.Empty;
        public string? FormaPago { get; set; }
        public string? Referencia { get; set; }
        public decimal Importe { get; set; }
        public decimal? SaldoCredito { get; set; }
        public decimal? TotalVenta { get; set; }
        public decimal? CobradoVenta { get; set; }
        public decimal? SaldoPendienteVenta { get; set; }
    }
}
