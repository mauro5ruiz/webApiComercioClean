

using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Comercio.Application.Dtos.Productos
{
    public class CrearProductoDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MaxLength(90, ErrorMessage = "El nombre no puede superar los 90 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(250, ErrorMessage = "La descripción no puede superar los 250 caracteres.")]
        public string? Descripcion { get; set; }

        [MaxLength(30, ErrorMessage = "El código no puede superar los 30 caracteres.")]
        public string? Codigo { get; set; }

        [MaxLength(80, ErrorMessage = "El código de barra no puede superar los 80 caracteres.")]
        public string? CodigoBarra { get; set; }

        [Required(ErrorMessage = "La categoría es obligatoria.")]
        public int IdCategoria { get; set; }

        [Required(ErrorMessage = "La marca es obligatoria.")]
        public int IdMarca { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El precio de compra no puede ser negativo.")]
        public decimal PrecioCompra { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El precio de venta no puede ser negativo.")]
        public decimal PrecioVenta { get; set; }

        public bool ControlStock { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo no puede ser negativo.")]
        public int StockMinimo { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El stock actual no puede ser negativo.")]
        public int StockActual { get; set; }

        public IFormFile? Imagen { get; set; }
    }
}
