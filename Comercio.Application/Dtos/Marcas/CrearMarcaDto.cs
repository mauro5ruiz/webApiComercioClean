using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Comercio.Application.Dtos.Marcas
{
    public class CrearMarcaDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 50 caracteres.")]
        public string Nombre { get; set; }
        public IFormFile? Imagen { get; set; }
        public bool Activa { get; set; }
    }
}
