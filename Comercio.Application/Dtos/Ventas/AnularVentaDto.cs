using Comercio.Application.Dtos.DevolucionesVentas;

namespace Comercio.Application.Dtos.Ventas
{
    public class AnularVentaDto
    {
        public IEnumerable<DevolucionPagoDto>? Pagos { get; set; }
    }
}
