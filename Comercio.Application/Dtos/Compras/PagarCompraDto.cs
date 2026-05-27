

namespace Comercio.Application.Dtos.Compras
{
    public class PagarCompraDto
    {
        public int IdCompra { get; set; }
        public decimal Importe { get; set; }
        public int IdFormaPago { get; set; }
    }
}
