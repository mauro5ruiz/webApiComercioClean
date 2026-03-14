
using System.ComponentModel.DataAnnotations;

namespace Comercio.Application.Dtos.AjustesStock
{
    public class AjustesStockDto
    {
        [Required]
        public int IdProducto { get; set; }

        [Range(0, 1000000, ErrorMessage = "El stock debe estar entre 0 y 1.000.000")]
        public int StockReal { get; set; }

        [MaxLength(200)]
        public string Motivo { get; set; } = string.Empty;
    }
}
