using Comercio.Application.Enums;

namespace Comercio.Application.Dtos.Productos
{
    public class ActualizacionPrecioIndividualDto
    {
        public decimal Valor { get; set; }
        public TipoOperacionPrecio TipoOperacion { get; set; }
    }
}
