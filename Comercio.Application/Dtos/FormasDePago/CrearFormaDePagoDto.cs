

using System.ComponentModel.DataAnnotations;

namespace Comercio.Application.Dtos.FormasDePago
{
    public class CrearFormaDePagoDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 20 caracteres.")]
        public string Nombre { get; set; }
    }
}

