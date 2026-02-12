

namespace Comercio.Domain.Entidades
{
    public class Proveedor
    {
        public int Id { get; set; }

        public string RazonSocial { get; set; } = string.Empty;

        public string CUIT { get; set; } = string.Empty;

        public string? CondicionIVA { get; set; }

        public string? Telefono { get; set; }

        public string? Email { get; set; }

        public string? PersonaContacto { get; set; }

        public string? Direccion { get; set; }

        public string? Localidad { get; set; }

        public string? Provincia { get; set; }

        public string? CodigoPostal { get; set; }

        public string? Observaciones { get; set; }

        public bool Activo { get; set; }

        public string? UrlImagen { get; set; }

        public DateTime FechaCreacion { get; set; }
    }
}
