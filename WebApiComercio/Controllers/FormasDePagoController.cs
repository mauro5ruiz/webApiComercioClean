using Comercio.Application.Dtos.Categorias;
using Comercio.Application.Dtos.FormasDePago;
using Comercio.Application.Interfaces;
using Comercio.Application.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace Comercio.Api.Controllers
{
    [ApiController]
    [Route("api/formas-de-pago")]
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
            var formaDePagos = await _formasDePagoServicio.ObtenerTodas();
            return Ok(formaDePagos);
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
                var formaDePago = await _formasDePagoServicio.Crear(dto);
                return CreatedAtAction(nameof(ObtenerPorId), new { id = formaDePago.Id }, formaDePago);
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
                var formaDePago = await _formasDePagoServicio.Actualizar(id, dto);

                return Ok(formaDePago);
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
            try
            {
                await _formasDePagoServicio.Eliminar(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
