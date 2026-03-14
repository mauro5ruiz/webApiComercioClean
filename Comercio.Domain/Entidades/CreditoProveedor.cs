

namespace Comercio.Domain.Entidades
{
    public class CreditoProveedor
    {
        public int Id { get; set; }

        public int IdProveedor { get; set; }

        public int IdDevolucionCompra { get; set; }

        public decimal Importe { get; set; }

        public decimal Saldo { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        public Proveedor Proveedor { get; set; }

        public DevolucionCompra DevolucionCompra { get; set; }
    }
}
