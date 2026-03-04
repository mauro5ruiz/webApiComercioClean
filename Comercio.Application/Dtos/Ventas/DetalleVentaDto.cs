

namespace Comercio.Application.Dtos.Ventas
{
    public class DetalleVentaDto
    {
        public int IdProducto { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }
}
