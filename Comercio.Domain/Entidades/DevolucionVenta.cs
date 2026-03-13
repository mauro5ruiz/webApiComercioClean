

namespace Comercio.Domain.Entidades
{
    public class DevolucionVenta
    {
        public int Id { get; set; }

        public int IdVenta { get; set; }

        public string? NumeroComprobante { get; set; }

        public DateTime Fecha { get; set; }

        public int? IdCliente { get; set; }

        public int? IdSucursal { get; set; }

        public decimal Total { get; set; }

        public string? Observaciones { get; set; }

        public string? Estado { get; set; }

        public Venta Venta { get; set; } = null!;

        public ICollection<DetalleDevolucionVenta> Detalles { get; set; } = new List<DetalleDevolucionVenta>();

        public ICollection<DevolucionVentaPago> Pagos { get; set; } = new List<DevolucionVentaPago>();
    }
}
