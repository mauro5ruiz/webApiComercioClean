using Comercio.Application.Dtos.Categorias;
using Comercio.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Comercio.Api.Controllers
{
    [ApiController]
    [Route("api/categorias")]
    public class CategoriasController : ControllerBase
    {
        private readonly ICategoriasServicio _categoriasServicio;

        public CategoriasController(ICategoriasServicio categoriasServicio)
        {
            _categoriasServicio = categoriasServicio;
        }

        // GET: api/categorias
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoriaDto>>> Obtener()
        {
            var categorias = await _categoriasServicio.ObtenerTodas();
            return Ok(categorias);
        }

        // GET: api/categorias/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<CategoriaDto>> ObtenerPorId([FromRoute] int id)
        {
            var categoria = await _categoriasServicio.ObtenerPorId(id);
            if (categoria is null) return NotFound();

            return Ok(categoria);
        }

        // POST: api/categorias
        [HttpPost]
        public async Task<ActionResult<int>> Crear([FromBody] CrearCategoriaDto dto)
        {
            try
            {
                var categoria = await _categoriasServicio.Crear(dto);
                return CreatedAtAction(nameof(ObtenerPorId), new { id = categoria.Id }, categoria);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // PUT: api/categorias/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar([FromRoute] int id, [FromBody] CrearCategoriaDto dto)
        {
            try
            {
                var categoria = await _categoriasServicio.Actualizar(id, dto);

                return Ok(categoria);
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
                await _categoriasServicio.Eliminar(id);
                return NoContent(); 
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}