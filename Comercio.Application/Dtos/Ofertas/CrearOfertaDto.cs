using Comercio.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Comercio.Application.Dtos.Ofertas
{
    public class CrearOfertaDto
    {
        [Required]
        public int IdProducto { get; set; }

        [Required]
        public TipoDescuento TipoDescuento { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal ValorDescuento { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaFin { get; set; }
    }
}
