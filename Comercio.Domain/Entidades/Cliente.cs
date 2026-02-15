

namespace Comercio.Domain.Entidades
{
    public class Cliente
    {
        public int Id { get; set; }

        // 1 = Persona | 2 = Empresa
        public int TipoCliente { get; set; }

        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? RazonSocial { get; set; }

        public string? NroDocumento { get; set; }
        public string? Cuit { get; set; }
        public string? NroTelefono { get; set; }
        public string? Email { get; set; }

        public string? UrlImagen { get; set; }

        public DateTime FechaAlta { get; set; }
        public DateTime? FechaBaja { get; set; }

        public string? Direccion { get; set; }
        public string? Localidad { get; set; }
        public string? Provincia { get; set; }
        public string? CodigoPostal { get; set; }

        public string? CondicionIva { get; set; }
        public string? Observaciones { get; set; }

        public bool Activo { get; set; }

        // Propiedad calculada
        public string NombreCompleto
        {
            get
            {
                if (TipoCliente == 1)
                    return $"{Nombre} {Apellido}";

                return RazonSocial ?? "";
            }
        }
    }
}
