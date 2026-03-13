using Comercio.Application.Dtos.DevolucionesVentas;
using Comercio.Application.Interfaces;
using Comercio.Domain.Entidades;
using Microsoft.AspNetCore.Mvc;

namespace Comercio.Api.Controllers
{
    [ApiController]
    [Route("api/devoluciones-ventas")]
    public class DevolucionesVentasController : Controller
    {
        private readonly IDevolucionesVentasServicio _devolucionesServicio;

        public DevolucionesVentasController(IDevolucionesVentasServicio devolucionesServicio)
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
        public async Task<IActionResult> CrearDevolucion([FromBody] CrearDevolucionVentaDto request)
        {
            try
            {
                var devolucion = new DevolucionVenta
                {
                    IdVenta = request.IdVenta,
                    NumeroComprobante = request.NumeroComprobante,
                    IdCliente = request.IdCliente,
                    IdSucursal = request.IdSucursal,
                    Observaciones = request.Observaciones
                };

                var detalles = request.Detalles.Select(d => new DetalleDevolucionVenta
                {
                    IdVentaDetalle = d.IdVentaDetalle,
                    IdProducto = d.IdProducto,
                    Cantidad = d.Cantidad
                });

                var pagos = request.Pagos?.Select(p => new DevolucionVentaPago
                {
                    IdFormaPago = p.IdFormaPago,
                    Importe = p.Importe,
                    Referencia = p.Referencia
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