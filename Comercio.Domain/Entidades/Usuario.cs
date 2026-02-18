

namespace Comercio.Domain.Entidades
{
    public class Usuario
    {
        public int Id { get; set; }

        // Datos personales
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; } = string.Empty;

        // Credenciales
        public string UsuarioLogin { get; set; } = string.Empty;
        public string ClaveHash { get; set; } = string.Empty;

        // Seguridad
        public bool Activo { get; set; }
        public bool DebeCambiarClave { get; set; }

        // Relaciones
        public int RolId { get; set; }
        public int? SucursalId { get; set; }

        // Auditoría
        public DateTime FechaCreacion { get; set; }
        public DateTime? UltimoAcceso { get; set; }

        // Propiedades calculadas
        public string NombreCompleto => $"{Nombre} {Apellido}";
    }
}
