namespace Comercio.Application.Dtos.Clientes
{
    public class ClienteVentaPendienteDto
    {
        public int IdVenta { get; set; }
        public DateTime Fecha { get; set; }
        public string Comprobante { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public decimal Cobrado { get; set; }
        public decimal SaldoPendiente { get; set; }
    }
}
