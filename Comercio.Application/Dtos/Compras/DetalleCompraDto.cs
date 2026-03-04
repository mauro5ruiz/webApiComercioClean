

namespace Comercio.Application.Dtos.Compras
{
    public class DetalleCompraDto
    {
        public int IdProducto { get; set; }

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }
    }
}
