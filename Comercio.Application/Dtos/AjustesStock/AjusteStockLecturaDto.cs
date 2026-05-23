

using Comercio.Domain.Enums;

namespace Comercio.Application.Dtos.AjustesStock
{
    public class AjusteStockLecturaDto
    {
        public int IdProducto { get; set; }
        public string Producto { get; set; }
        public TipoMovimientoStock IdTipoMovimientoStock { get; set; }
        public int Cantidad { get; set; }
        public int IdReferencia { get; set; }
        public DateTime Fecha { get; set; }
        public string Observaciones { get; set; }
    }
}
