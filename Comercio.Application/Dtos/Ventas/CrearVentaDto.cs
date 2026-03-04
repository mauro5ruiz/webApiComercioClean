
namespace Comercio.Application.Dtos.Ventas
{
    public class CrearVentaDto
    {
        public string NumeroComprobante { get; set; } = string.Empty;
        public int IdCliente { get; set; }
        public int IdVendedor { get; set; }
        public int IdSucursal { get; set; }
        public string? Observaciones { get; set; }

        public List<DetalleVentaDto> Detalles { get; set; } = new();
        public List<VentaPagoDto>? Pagos { get; set; }
    }
}
