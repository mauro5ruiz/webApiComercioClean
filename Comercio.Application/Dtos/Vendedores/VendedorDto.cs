

namespace Comercio.Application.Dtos.Vendedores
{
    public class VendedorDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string NroDni { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaAlta { get; set; }
        public DateTime? FechaEliminado { get; set; }
        public string? Observaciones { get; set; }
        public int IdSucursal { get; set; }
    }
}
