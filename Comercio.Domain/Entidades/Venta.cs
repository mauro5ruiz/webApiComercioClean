

namespace Comercio.Domain.Entidades
{
    public class Venta
    {
        public int Id { get; set; }
        public string NumeroComprobante { get; set; } = null!;
        public DateTime Fecha { get; set; }
        public int IdCliente { get; set; }
        public int IdVendedor { get; set; }
        public int IdSucursal { get; set; }
        public decimal Total { get; set; }
        public decimal TotalPagado { get; set; }
        public decimal SaldoPendiente { get; set; }
        public string Estado { get; set; } = "Activa";
        public string? Observaciones { get; set; }
        public DateTime? FechaAnulacion { get; set; }

        public List<DetalleVenta> Detalles { get; set; } = new();
        public List<VentaPago> Pagos { get; set; } = new();
    }
}
