using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Comercio.Application.Dtos.Proveedores
{
    public class CrearProveedorDto
    {
        [Required(ErrorMessage = "La razón social es obligatoria.")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "La razón social debe tener entre 3 y 150 caracteres.")]
        public required string RazonSocial { get; set; }

        [Required(ErrorMessage = "El CUIT es obligatorio.")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "El CUIT debe contener exactamente 11 dígitos numéricos.")]
        public required string Cuit { get; set; }

        [StringLength(50, ErrorMessage = "La condición IVA no puede superar los 50 caracteres.")]
        public string? CondicionIva { get; set; }

        [Phone(ErrorMessage = "El teléfono no es válido.")]
        [StringLength(30, ErrorMessage = "El teléfono no puede superar los 30 caracteres.")]
        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "El email no es válido.")]
        [StringLength(150, ErrorMessage = "El email no puede superar los 150 caracteres.")]
        public string? Email { get; set; }

        [StringLength(100, ErrorMessage = "La persona de contacto no puede superar los 100 caracteres.")]
        public string? PersonaContacto { get; set; }

        [StringLength(200, ErrorMessage = "La dirección no puede superar los 200 caracteres.")]
        public string? Direccion { get; set; }

        [StringLength(100, ErrorMessage = "La localidad no puede superar los 100 caracteres.")]
        public string? Localidad { get; set; }

        [StringLength(100, ErrorMessage = "La provincia no puede superar los 100 caracteres.")]
        public string? Provincia { get; set; }

        [StringLength(20, ErrorMessage = "El código postal no puede superar los 20 caracteres.")]
        public string? CodigoPostal { get; set; }

        [StringLength(500, ErrorMessage = "Las observaciones no pueden superar los 500 caracteres.")]
        public string? Observaciones { get; set; }

        public IFormFile? Imagen { get; set; }

        public bool Activo { get; set; } = true;
    }
}
