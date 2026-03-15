using Comercio.Application.Dtos.Compras;
using Comercio.Application.Interfaces;
using Comercio.Domain.Entidades;
using Microsoft.AspNetCore.Mvc;

namespace Comercio.Api.Controllers
{
    [ApiController]
    [Route("api/compras")]
    public class ComprasController : Controller
    {
        private readonly IComprasServicio _comprasServicio;

        public ComprasController(IComprasServicio comprasServicio)
        {
            _comprasServicio = comprasServicio;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerEntreFechas([FromQuery] DateTime desde, [FromQuery] DateTime hasta)
        {
            try
            {
                if (desde > hasta)
                    return BadRequest(new { Error = "La fecha 'desde' no puede ser mayor que 'hasta'" });

                var compras = await _comprasServicio.ObtenerEntreFechas(desde, hasta);

                return Ok(compras);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            try
            {
                var compra = await _comprasServicio.ObtenerPorId(id);

                if (compra is null)
                    return NotFound(new { Mensaje = "Compra no encontrada" });

                return Ok(compra);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CrearCompra([FromBody] CrearCompraDto request)
        {
            try
            {
                var compra = new Compra
                {
                    NumeroComprobante = request.NumeroComprobante,
                    IdProveedor = request.IdProveedor,
                    IdSucursal = request.IdSucursal,
                    Observaciones = request.Observaciones
                };

                var detalles = request.Detalles.Select(d => new DetalleCompra
                {
                    IdProducto = d.IdProducto,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario
                });

                var pagos = request.Pagos?.Select(p => new CompraPago
                {
                    IdFormaPago = p.IdFormaPago,
                    Importe = p.Importe,
                    Referencia = p.Referencia
                });

                var idCompra = await _comprasServicio.CrearCompra(compra, detalles, pagos);

                return Ok(new { Mensaje = "Compra creada correctamente", IdCompra = idCompra });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Error = ex.Message
                });
            }
        }

        [HttpPut("anular/{id}")]
        public async Task<IActionResult> AnularCompra(int id)
        {
            try
            {
                await _comprasServicio.AnularCompra(id);

                return Ok(new { Mensaje = "Compra anulada correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("pagar-proveedor")]
        public async Task<IActionResult> PagarProveedor([FromBody] PagarProveedorDto dto)
        {
            try
            {
                await _comprasServicio.PagarProveedor(dto.IdProveedor, dto.Importe, dto.IdFormaPago);
                
                return Ok(new { mensaje = "Pago registrado correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}