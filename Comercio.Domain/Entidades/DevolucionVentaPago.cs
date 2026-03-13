

namespace Comercio.Domain.Entidades
{
    public class DevolucionVentaPago
    {
        public int Id { get; set; }

        public int IdDevolucionVenta { get; set; }

        public int IdFormaPago { get; set; }

        public decimal Importe { get; set; }

        public string? Referencia { get; set; }

        public DateTime FechaPago { get; set; }

        public string? Estado { get; set; }

        public DevolucionVenta DevolucionVenta { get; set; } = null!;
    }
}
