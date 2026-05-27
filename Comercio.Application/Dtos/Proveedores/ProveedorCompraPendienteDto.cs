

namespace Comercio.Application.Dtos.Proveedores
{
    public class ProveedorCompraPendienteDto
    {
        public int IdCompra { get; set; }
        public DateTime Fecha { get; set; }
        public string Comprobante { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public decimal Pagado { get; set; }
        public decimal SaldoPendiente { get; set; }
    }
}
