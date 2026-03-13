

namespace Comercio.Domain.Entidades
{
    public class DetalleDevolucionVenta
    {
        public int Id { get; set; }

        public int IdDevolucionVenta { get; set; }

        public int IdVentaDetalle { get; set; }

        public int IdProducto { get; set; }

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }

        public decimal Subtotal { get; set; }


        /* Relaciones */

        public DevolucionVenta DevolucionVenta { get; set; } = null!;

        public DetalleVenta VentaDetalle { get; set; } = null!;
    }
}
