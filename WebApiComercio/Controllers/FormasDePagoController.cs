using Comercio.Application.Dtos.Categorias;
using Comercio.Application.Dtos.FormasDePago;
using Comercio.Application.Interfaces;
using Comercio.Application.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace Comercio.Api.Controllers
{
    [ApiController]
    [Route("api/fornas-de-pago")]
    public class FormasDePagoController : Controller
    {
        private readonly IFormasDePagoServicio _formasDePagoServicio;

        public FormasDePagoController(IFormasDePagoServicio formasDePagoServicio)
        {
            _formasDePagoServicio = formasDePagoServicio;
        }

        // GET: api/formas-de-pago
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FormaDePagoDto>>> Obtener()
        {
            var categorias = await _formasDePagoServicio.ObtenerTodas();
            return Ok(categorias);
        }

        // GET: api/formas-de-pago/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<FormaDePagoDto>> ObtenerPorId([FromRoute] int id)
        {
            var fdp = await _formasDePagoServicio.ObtenerPorId(id);
            if (fdp is null) return NotFound();

            return Ok(fdp);
        }

        // POST: api/categorias
        [HttpPost]
        public async Task<ActionResult<int>> Crear([FromBody] CrearFormaDePagoDto dto)
        {
            try
            {
                var id = await _formasDePagoServicio.Crear(dto);
                return CreatedAtAction(nameof(ObtenerPorId), new { id }, id);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // PUT: api/categorias/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar([FromRoute] int id, [FromBody] CrearFormaDePagoDto dto)
        {
            try
            {
                var actualizado = await _formasDePagoServicio.Actualizar(id, dto);
                if (!actualizado) return NotFound();

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // DELETE: api/categorias/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar([FromRoute] int id)
        {
            var eliminado = await _formasDePagoServicio.Eliminar(id);
            if (!eliminado) return NotFound();

            return NoContent();
        }
    }
}
