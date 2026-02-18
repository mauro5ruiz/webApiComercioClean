using System.ComponentModel.DataAnnotations;

namespace Comercio.Application.Dtos.Usuarios
{
    public class CrearUsuarioDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 100 caracteres.")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "El email no es válido.")]
        [StringLength(150, ErrorMessage = "El email no puede superar los 150 caracteres.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El usuario es obligatorio.")]
        [StringLength(50, MinimumLength = 4, ErrorMessage = "El usuario debe tener entre 4 y 50 caracteres.")]
        [RegularExpression(@"^[a-zA-Z0-9._-]+$",ErrorMessage = "El usuario solo puede contener letras, números, punto, guión y guión bajo.")]
        public string UsuarioLogin { get; set; } = string.Empty;

        [Required(ErrorMessage = "La clave es obligatoria.")]
        [StringLength(100, MinimumLength = 6,ErrorMessage = "La clave debe tener entre 6 y 100 caracteres.")]
        public string Clave { get; set; } = string.Empty;

        [Required(ErrorMessage = "El rol es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un rol válido.")]
        public int RolId { get; set; }

        public int? SucursalId { get; set; }

        [Phone(ErrorMessage = "El teléfono no es válido.")]
        [StringLength(20, ErrorMessage = "El teléfono no puede superar los 20 caracteres.")]
        public string? Telefono { get; set; }

        public bool DebeCambiarClave { get; set; } = true;
        public bool Activo { get; set; } = true;
    }
}
