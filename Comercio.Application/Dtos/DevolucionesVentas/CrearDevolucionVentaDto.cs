

namespace Comercio.Application.Dtos.DevolucionesVentas
{
    public class CrearDevolucionVentaDto
    {
        public int IdVenta { get; set; }

        public string? NumeroComprobante { get; set; }

        public int IdCliente { get; set; }

        public int IdSucursal { get; set; }

        public string? Observaciones { get; set; }

        public IEnumerable<DetalleDevolucionDto> Detalles { get; set; } = new List<DetalleDevolucionDto>();

        public IEnumerable<DevolucionPagoDto>? Pagos { get; set; }
    }
}
