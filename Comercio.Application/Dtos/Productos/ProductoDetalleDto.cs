

namespace Comercio.Application.Dtos.Productos
{
    public class ProductoDetalleDto
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public string? Codigo { get; set; }

        public string? CodigoBarra { get; set; }

        public string Categoria { get; set; } = string.Empty;

        public string Marca { get; set; } = string.Empty;

        public decimal PrecioCompra { get; set; }

        public decimal PrecioVenta { get; set; }

        public int StockMinimo { get; set; }

        public int StockActual { get; set; }

        public bool ControlStock { get; set; }

        public string? UrlImagen { get; set; }

        public bool Activo { get; set; }

        public DateTime FechaAlta { get; set; }
    }
}
