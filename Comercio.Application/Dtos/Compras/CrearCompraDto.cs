

namespace Comercio.Application.Dtos.Compras
{
    public class CrearCompraDto
    {
        public string NumeroComprobante { get; set; } = string.Empty;
        public int IdProveedor { get; set; }
        public int IdSucursal { get; set; }
        public string? Observaciones { get; set; }
        public List<DetalleCompraDto> Detalles { get; set; } = new();
        public List<CompraPagoDto>? Pagos { get; set; }
    }
}
