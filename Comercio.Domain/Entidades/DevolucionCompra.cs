

namespace Comercio.Domain.Entidades
{
    public class DevolucionCompra
    {
        public int Id { get; set; }

        public int IdCompra { get; set; }

        public int IdProveedor { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        public string? Motivo { get; set; }

        public decimal Total { get; set; }

        public int Estado { get; set; } = 1;

        public ICollection<DevolucionCompraDetalle> Detalles { get; set; }

        public ICollection<PagoDevolucionCompra> Pagos { get; set; }
    }
}
