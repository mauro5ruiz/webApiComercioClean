namespace Comercio.Application.Dtos.DevolucionesCompras
{
    public class CrearDevolucionCompraDto
    {
        public int IdCompra { get; set; }

        public int IdProveedor { get; set; }

        public string? Motivo { get; set; }

        public IEnumerable<CrearDevolucionCompraDetalleDto> Detalles { get; set; }

        public IEnumerable<CrearPagoDevolucionCompraDto>? Pagos { get; set; }
    }
}