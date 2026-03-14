

namespace Comercio.Domain.Entidades
{
    public class PagoDevolucionCompra
    {
        public int Id { get; set; }

        public int IdDevolucionCompra { get; set; }

        public int IdFormaPago { get; set; }

        public decimal Importe { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        public DevolucionCompra DevolucionCompra { get; set; }
    }
}
