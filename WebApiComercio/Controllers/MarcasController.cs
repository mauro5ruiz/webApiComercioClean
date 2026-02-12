using Comercio.Application.Dtos.Marcas;
using Comercio.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Comercio.Api.Controllers
{
    [ApiController]
    [Route("api/marcas")]
    public class MarcasController : Controller
    {
        private readonly IMarcasServicio _marcasServicio;

        public MarcasController(IMarcasServicio marcasServicio)
        {
            _marcasServicio = marcasServicio;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MarcaDto>>> Obtener()
        {
            var marcas = await _marcasServicio.ObtenerTodas();
            return Ok(marcas);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<MarcaDto>> ObtenerPorId([FromRoute] int id)
        {
            var marca = await _marcasServicio.ObtenerPorId(id);
            if (marca is null)
                return NotFound();

            return Ok(marca);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Crear([FromForm] CrearMarcaDto dto)
        {
            try
            {
                var id = await _marcasServicio.Crear(dto);
                return CreatedAtAction(nameof(ObtenerPorId), new { id }, id);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar([FromRoute] int id,[FromForm] CrearMarcaDto dto)
        {
            try
            {
                var actualizado = await _marcasServicio.Actualizar(id, dto);
                if (!actualizado)
                    return NotFound();

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar([FromRoute] int id)
        {
            var eliminado = await _marcasServicio.Eliminar(id);
            if (!eliminado)
                return NotFound();

            return NoContent();
        }
    }
}
