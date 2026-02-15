

using System.ComponentModel.DataAnnotations;

namespace Comercio.Application.Dtos.Sucursales
{
    public class CrearSucursalDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres.")]
        public required string Nombre { get; set; }

        [StringLength(30, MinimumLength = 2, ErrorMessage = "El código debe tener entre 2 y 30 caracteres.")]
        public string? Codigo { get; set; }

        [StringLength(90, ErrorMessage = "La dirección no puede superar los 90 caracteres.")]
        public string? Direccion { get; set; }

        [StringLength(100, ErrorMessage = "La dirección no puede superar los 100 caracteres.")]
        public string? Localidad { get; set; }

        [Phone(ErrorMessage = "El número de teléfono no es válido.")]
        [StringLength(20, ErrorMessage = "La número de teléfono no puede superar los 20 caracteres.")]
        public string? NroTelefono { get; set; }

        [EmailAddress(ErrorMessage = "El email no es válido.")]
        [StringLength(150, ErrorMessage = "El email no puede superar los 150 caracteres.")]
        public string? Email { get; set; }

        public bool Activa { get; set; }
    }
}
