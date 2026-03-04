

namespace Comercio.Application.Dtos.Ventas
{
    public class VentaPagoDto
    {
        public int IdFormaPago { get; set; }
        public decimal Importe { get; set; }
        public int? Cuotas { get; set; }
        public string? Referencia { get; set; }
    }
}
