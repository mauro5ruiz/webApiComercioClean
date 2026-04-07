using Comercio.Application.Dtos.Marcas;
using Comercio.Application.Interfaces;
using Comercio.Application.Servicios;
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
                var marca = await _marcasServicio.Crear(dto);
                return CreatedAtAction(nameof(ObtenerPorId), new { id = marca.Id }, marca);
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
                var marca = await _marcasServicio.Actualizar(id, dto);

                return Ok(marca);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar([FromRoute] int id)
        {
            try
            {
                await _marcasServicio.Eliminar(id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
