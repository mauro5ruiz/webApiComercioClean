

namespace Comercio.Application.Dtos.Vendedores
{
    public class CrearVendedorDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string NroDni { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string? Observaciones { get; set; }
        public string PinHash { get; set; } = string.Empty;
        public int IdSucursal { get; set; }
    }
}
