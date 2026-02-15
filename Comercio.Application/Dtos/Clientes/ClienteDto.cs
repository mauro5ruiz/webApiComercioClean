using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comercio.Application.Dtos.Clientes
{
    public class ClienteDto
    {
        public int Id { get; set; }

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

        public int? CondicionIva { get; set; }

        public string? Observaciones { get; set; }

        public bool Activo { get; set; }

        public string NombreCompleto => TipoCliente == 1 ? $"{Nombre} {Apellido}" : RazonSocial ?? string.Empty;
    }
}
