

namespace Comercio.Application.Dtos.DevolucionesVentas
{
    public class DevolucionPagoDto
    {
        public int IdFormaPago { get; set; }

        public decimal Importe { get; set; }

        public string? Referencia { get; set; }
    }
}
