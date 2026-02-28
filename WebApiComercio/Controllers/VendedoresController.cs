using Comercio.Application.Dtos.Vendedores;
using Comercio.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Comercio.Api.Controllers
{
    [ApiController]
    [Route("api/vendedores")]
    public class VendedoresController : Controller
    {
        private readonly IVendedoresServicio _vendedoresServicio;

        public VendedoresController(IVendedoresServicio vendedoresServicio)
        {
            _vendedoresServicio = vendedoresServicio;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<VendedorDto>>> Obtener([FromQuery] bool incluirEliminados = false)
        {
            var vendedores = await _vendedoresServicio.ObtenerTodos(incluirEliminados);
            return Ok(vendedores);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<VendedorDto>> ObtenerPorId([FromRoute] int id)
        {
            var vendedor = await _vendedoresServicio.ObtenerPorId(id);
            if (vendedor is null) return NotFound();

            return Ok(vendedor);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Crear([FromBody] CrearVendedorDto dto)
        {
            try
            {
                var id = await _vendedoresServicio.Crear(dto);
                return CreatedAtAction(nameof(ObtenerPorId), new { id }, id);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar([FromRoute] int id, [FromBody] CrearVendedorDto dto)
        {
            try
            {
                var actualizado = await _vendedoresServicio.Actualizar(id, dto);
                if (!actualizado) return NotFound();

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPatch("{id:int}/baja")]
        public async Task<IActionResult> DarDeBaja([FromRoute] int id)
        {
            var eliminado = await _vendedoresServicio.DarDeBaja(id);
            if (!eliminado) return NotFound();

            return NoContent();
        }

        [HttpPatch("{id:int}/restaurar")]
        public async Task<IActionResult> Restaurar([FromRoute] int id)
        {
            var restaurado = await _vendedoresServicio.Restaurar(id);
            if (!restaurado) return NotFound();

            return NoContent();
        }

        [HttpDelete("{id:int}/permanente")]
        public async Task<IActionResult> EliminarPermanentemente([FromRoute] int id)
        {
            var eliminado = await _vendedoresServicio.EliminarPermanentemente(id);
            if (!eliminado) return NotFound();

            return NoContent();
        }
    }
}
