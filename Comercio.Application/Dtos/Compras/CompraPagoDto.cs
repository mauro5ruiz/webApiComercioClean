

namespace Comercio.Application.Dtos.Compras
{
    public class CompraPagoDto
    {
        public int IdFormaPago { get; set; }
        public decimal Importe { get; set; }
        public string? Referencia { get; set; }
    }
}
