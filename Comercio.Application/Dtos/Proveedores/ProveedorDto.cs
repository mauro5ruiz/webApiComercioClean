

namespace Comercio.Application.Dtos.Proveedores
{
    public class ProveedorDto
    {
        public int Id { get; set; }
        public required string RazonSocial { get; set; }
        public required string Cuit { get; set; }
        public string? CondicionIva { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? PersonaContacto { get; set; }
        public string? Direccion { get; set; }
        public string? Localidad { get; set; }
        public string? Provincia { get; set; }
        public string? CodigoPostal { get; set; }
        public string? Observaciones { get; set; }
        public string? UrlImagen { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaAlta { get; set; }
        public DateTime? FechaBaja { get; set; }
    }
}
