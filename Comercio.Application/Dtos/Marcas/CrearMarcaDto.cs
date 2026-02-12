using Microsoft.AspNetCore.Http;

namespace Comercio.Application.Dtos.Marcas
{
    public class CrearMarcaDto
    {
        public string Nombre { get; set; }
        public IFormFile? Imagen { get; set; }
        public bool Activa { get; set; }
    }
}
