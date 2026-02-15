using System.ComponentModel.DataAnnotations;

namespace Comercio.Application.Dtos.Categorias
{
    public class CrearCategoriaDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 50 caracteres.")]
        public string Nombre { get; set; }
    }
}
