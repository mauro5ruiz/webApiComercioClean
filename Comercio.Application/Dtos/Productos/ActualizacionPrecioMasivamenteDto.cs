using Comercio.Application.Enums;

namespace Comercio.Application.Dtos.Productos
{
    public class ActualizacionPrecioMasivamenteDto
    {
        public decimal Valor { get; set; }
        public TipoOperacionPrecio TipoOperacion { get; set; }
        public int? IdCategoria { get; set; }
        public int? IdMarca { get; set; }
        public bool SoloActivos { get; set; } = true;
    }
}
