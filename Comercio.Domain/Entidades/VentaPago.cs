

namespace Comercio.Domain.Entidades
{
    public class VentaPago
    {
        public int Id { get; set; }
        public int IdVenta { get; set; }
        public int IdFormaPago { get; set; }

        public decimal Importe { get; set; }
        public int? Cuotas { get; set; }
        public string? Referencia { get; set; }
        public DateTime FechaPago { get; set; }

        public string Estado { get; set; } = "Activo";
    }
}
