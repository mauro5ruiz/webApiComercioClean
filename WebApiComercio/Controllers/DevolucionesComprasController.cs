using Comercio.Application.Dtos.DevolucionesCompras;
using Comercio.Application.Interfaces;
using Comercio.Domain.Entidades;
using Microsoft.AspNetCore.Mvc;

namespace Comercio.Api.Controllers
{
    [ApiController]
    [Route("api/devoluciones-compras")]
    public class DevolucionesComprasController : Controller
    {
        private readonly IDevolucionesComprasServicio _devolucionesServicio;

        public DevolucionesComprasController(IDevolucionesComprasServicio devolucionesServicio)
        {
            _devolucionesServicio = devolucionesServicio;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerEntreFechas([FromQuery] DateTime desde, [FromQuery] DateTime hasta)
        {
            try
            {
                if (desde > hasta)
                    return BadRequest(new { Error = "La fecha 'desde' no puede ser mayor que 'hasta'" });

                var devoluciones = await _devolucionesServicio.ObtenerEntreFechas(desde, hasta);

                return Ok(devoluciones);
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
                var devolucion = await _devolucionesServicio.ObtenerPorId(id);

                if (devolucion is null)
                    return NotFound(new { Mensaje = "Devolución no encontrada" });

                return Ok(devolucion);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CrearDevolucion([FromBody] CrearDevolucionCompraDto request)
        {
            try
            {
                var devolucion = new DevolucionCompra
                {
                    IdCompra = request.IdCompra,
                    IdProveedor = request.IdProveedor,
                    Motivo = request.Motivo
                };

                var detalles = request.Detalles.Select(d => new DevolucionCompraDetalle
                {
                    IdProducto = d.IdProducto,
                    Cantidad = d.Cantidad
                });

                var pagos = request.Pagos?.Select(p => new PagoDevolucionCompra
                {
                    IdFormaPago = p.IdFormaPago,
                    Importe = p.Importe
                });

                var idDevolucion = await _devolucionesServicio.RegistrarDevolucion(devolucion, detalles, pagos);

                return Ok(new
                {
                    Mensaje = "Devolución registrada correctamente",
                    IdDevolucion = idDevolucion
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Error = ex.Message
                });
            }
        }
    }
}