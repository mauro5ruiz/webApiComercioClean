using Comercio.Application.Dtos.Perdidas;
using Comercio.Application.Interfaces;
using Comercio.Domain.Entidades;
using Microsoft.AspNetCore.Mvc;

namespace Comercio.Api.Controllers
{
    [ApiController]
    [Route("api/perdidias")]
    public class PerdidasController : Controller
    {
        private readonly IPerdidasServicio _perdidasServicio;

        public PerdidasController(IPerdidasServicio perdidasServicio)
        {
            _perdidasServicio = perdidasServicio;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerEntreFechas([FromQuery] DateTime desde, [FromQuery] DateTime hasta)
        {
            try
            {
                if (desde > hasta)
                    return BadRequest(new { Error = "La fecha 'desde' no puede ser mayor que 'hasta'" });

                var perdidas = await _perdidasServicio.ObtenerPorFechas(desde, hasta);

                return Ok(perdidas);
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
                var perdida = await _perdidasServicio.ObtenerPerdidaCompleta(id);

                if (perdida is null)
                    return NotFound(new { Mensaje = "Pérdida no encontrada" });

                return Ok(perdida);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CrearPerdida([FromBody] CrearPerdidaDto request)
        {
            try
            {
                var perdida = new Perdida
                {
                    Fecha = request.Fecha,
                    Motivo = request.Motivo,
                    Observacion = request.Observacion,
                    IdUsuario = request.IdUsuario,
                    IdEstado = request.IdEstado
                };

                var detalles = request.Detalles.Select(d => new DetallePerdida
                {
                    IdProducto = d.IdProducto,
                    Cantidad = d.Cantidad
                }).ToList();

                var idPerdida = await _perdidasServicio.CrearPerdida(perdida, detalles);

                return Ok(new { Mensaje = "Pérdida creada correctamente", IdPerdida = idPerdida });
            }
            catch (Exception ex)
            {
                return BadRequest(new {  Error = ex.Message });
            }
        }

        [HttpPost("detalle")]
        public async Task<IActionResult> AgregarDetalle([FromBody] DetallePerdida detalle)
        {
            try
            {
                await _perdidasServicio.AgregarDetalle(detalle);

                return Ok(new { Mensaje = "Detalle agregado correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPut("detalle")]
        public async Task<IActionResult> ActualizarDetalle([FromBody] DetallePerdida detalle)
        {
            try
            {
                await _perdidasServicio.ActualizarDetalle(detalle);

                return Ok(new { Mensaje = "Detalle actualizado correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpDelete("detalle/{idDetalle}")]
        public async Task<IActionResult> EliminarDetalle(int idDetalle)
        {
            try
            {
                await _perdidasServicio.EliminarDetalle(idDetalle);

                return Ok(new { Mensaje = "Detalle eliminado correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarPerdida(int id, [FromBody] ActualizarPerdidaDto request)
        {
            try
            {
                await _perdidasServicio.ActualizarPerdida(id, request.Motivo, request.Observacion);

                return Ok(new { Mensaje = "Pérdida actualizada correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}
