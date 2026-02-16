

namespace Comercio.Application.Dtos.Productos
{
    public class ProductoDto
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Categoria { get; set; } = string.Empty;

        public string Marca { get; set; } = string.Empty;

        public decimal PrecioVenta { get; set; }

        public int StockActual { get; set; }

        public bool ControlStock { get; set; }

        public bool Activo { get; set; }
    }
}
