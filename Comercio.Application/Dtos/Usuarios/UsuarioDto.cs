

namespace Comercio.Application.Dtos.Usuarios
{
    public class UsuarioDto
    {
        public int Id { get; set; }

        public string NombreCompleto { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string UsuarioLogin { get; set; } = string.Empty;

        public string Rol { get; set; } = string.Empty;

        public string? Sucursal { get; set; }

        public bool Activo { get; set; }

        public DateTime FechaCreacion { get; set; }

        public DateTime? UltimoAcceso { get; set; }
    }
}
