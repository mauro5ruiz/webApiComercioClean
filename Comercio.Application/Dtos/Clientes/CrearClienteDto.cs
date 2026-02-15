

using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Comercio.Application.Dtos.Clientes
{
    public class CrearClienteDto
    {
        [Required(ErrorMessage = "Debe seleccionar el tipo de cliente")]
        public int TipoCliente { get; set; }

        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
        public string? Nombre { get; set; }

        [StringLength(100, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 100 caracteres.")]
        public string? Apellido { get; set; }

        [StringLength(20, MinimumLength = 5, ErrorMessage = "El número de documento debe tener entre 5 y 20 caracteres.")]
        public string? NroDocumento { get; set; }

        [RegularExpression(@"^\d{11}$", ErrorMessage = "El CUIT debe contener exactamente 11 dígitos numéricos.")]
        public string? Cuit { get; set; }

        [StringLength(100, MinimumLength = 2, ErrorMessage = "La razón soical debe tener entre 2 y 100 caracteres.")]
        public string? RazonSocial { get; set; }

        [Phone(ErrorMessage = "El teléfono no es válido.")]
        [StringLength(30, ErrorMessage = "El teléfono no puede superar los 30 caracteres.")]
        public string? NroTelefono { get; set; }

        [EmailAddress(ErrorMessage = "El email no es válido.")]
        [StringLength(150, ErrorMessage = "El email no puede superar los 150 caracteres.")]
        public string? Email { get; set; }

        public IFormFile? Imagen { get; set; }

        [StringLength(200, ErrorMessage = "La dirección no puede superar los 200 caracteres.")]
        public string? Direccion { get; set; }

        [StringLength(100, ErrorMessage = "La localidad no puede superar los 100 caracteres.")]
        public string? Localidad { get; set; }

        [StringLength(100, ErrorMessage = "La provincia no puede superar los 100 caracteres.")]
        public string? Provincia { get; set; }

        [StringLength(20, ErrorMessage = "El código postal no puede superar los 20 caracteres.")]
        public string? CodigoPostal { get; set; }

        [StringLength(50, ErrorMessage = "La caondición Iva no puede superar los 50 caracteres.")]
        public string? CondicionIva { get; set; }

        [StringLength(500, ErrorMessage = "Las observaciones no pueden superar los 500 caracteres.")]
        public string? Observaciones { get; set; }

        public bool Activo { get; set; } = true;
    }
}
