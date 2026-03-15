using Comercio.Application.Dtos.Ventas;
using Comercio.Application.Interfaces;
using Comercio.Domain.Entidades;
using Microsoft.AspNetCore.Mvc;

namespace Comercio.Api.Controllers
{
    [ApiController]
    [Route("api/ventas")]
    public class VentasController : Controller
    {
        private readonly IVentasServicio _ventasServicio;

        public VentasController(IVentasServicio ventasServicio)
        {
            _ventasServicio = ventasServicio;
        }

        [HttpGet()]
        public async Task<IActionResult> ObtenerEntreFechas([FromQuery] DateTime desde, [FromQuery] DateTime hasta)
        {
            try
            {
                if (desde > hasta)
                    return BadRequest(new { Error = "La fecha 'desde' no puede ser mayor que 'hasta'" });

                var ventas = await _ventasServicio.ObtenerEntreFechas(desde, hasta);

                return Ok(ventas);
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
                var venta = await _ventasServicio.ObtenerPorId(id);

                if (venta is null)
                    return NotFound(new { Mensaje = "Venta no encontrada" });

                return Ok(venta);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CrearVenta([FromBody] CrearVentaDto request)
        {
            try
            {
                var venta = new Venta
                {
                    NumeroComprobante = request.NumeroComprobante,
                    IdCliente = request.IdCliente,
                    IdVendedor = request.IdVendedor,
                    IdSucursal = request.IdSucursal,
                    Observaciones = request.Observaciones
                };

                var detalles = request.Detalles.Select(d => new DetalleVenta
                {
                    IdProducto = d.IdProducto,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario
                });

                var pagos = request.Pagos?.Select(p => new VentaPago
                {
                    IdFormaPago = p.IdFormaPago,
                    Importe = p.Importe,
                    Cuotas = p.Cuotas,
                    Referencia = p.Referencia
                });

                var idVenta = await _ventasServicio.CrearVenta(venta, detalles, pagos);

                return Ok(new { Mensaje = "Venta creada correctamente", IdVenta = idVenta });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPut("anular/{id}")]
        public async Task<IActionResult> AnularVenta(int id)
        {
            try
            {
                await _ventasServicio.AnularVenta(id);

                return Ok(new { Mensaje = "Venta anulada correctamente"  });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("pendientes/cliente/{idCliente}")]
        public async Task<IActionResult> ObtenerPendientesPorCliente(int idCliente)
        {
            try
            {
                var ventas = await _ventasServicio.ObtenerPendientesPorCliente(idCliente);

                return Ok(ventas);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("cobrar")]
        public async Task<IActionResult> CobrarCliente([FromBody] CobrarDto dto)
        {
            try
            {
                await _ventasServicio.CobrarCliente(dto.IdCliente,dto.Importe,dto.IdFormaPago,dto.Referencia);

                return Ok(new { Mensaje = "Cobro registrado correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}
