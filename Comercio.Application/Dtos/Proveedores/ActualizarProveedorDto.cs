using System.ComponentModel.DataAnnotations;

namespace Comercio.Application.Dtos.Proveedores
{
    public class ActualizarProveedorDto: CrearProveedorDto
    {
        public bool EliminarImagen { get; set; }
    }
}
