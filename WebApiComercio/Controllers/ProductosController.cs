using Comercio.Application.Dtos.Productos;
using Comercio.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Comercio.Api.Controllers
{
    [ApiController]
    [Route("api/productos")]
    public class ProductosController : Controller
    {
        private readonly IProductosServicio _productosServicio;

        public ProductosController(IProductosServicio productosServicio)
        {
            _productosServicio = productosServicio;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductoDto>>> Obtener([FromQuery] bool incluirEliminados = false)
        {
            var productos = await _productosServicio.ObtenerTodos(incluirEliminados);
            return Ok(productos);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductoDto>> ObtenerPorId([FromRoute] int id)
        {
            var producto = await _productosServicio.ObtenerPorId(id);

            if (producto is null)
                return NotFound();

            return Ok(producto);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Crear([FromForm] CrearProductoDto dto)
        {
            try
            {
                var id = await _productosServicio.Crear(dto);

                return CreatedAtAction(nameof(ObtenerPorId), new { id }, id);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
        }

        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Actualizar([FromRoute] int id,[FromForm] ActualizarProductoDto dto)
        {
            try
            {
                await _productosServicio.Actualizar(id, dto);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar([FromRoute] int id)
        {
            var eliminado = await _productosServicio.EliminarPermanentemente(id);

            if (!eliminado)
                return NotFound();

            return NoContent();
        }

        [HttpPatch("{id:int}/baja")]
        public async Task<IActionResult> DarDeBaja([FromRoute] int id)
        {
            var baja = await _productosServicio.DarDeBaja(id);

            if (!baja)
                return NotFound();

            return NoContent();
        }

        [HttpPatch("{id:int}/restauracion")]
        public async Task<IActionResult> Restaurar([FromRoute] int id)
        {
            var restaurado = await _productosServicio.Restaurar(id);

            if (!restaurado)
                return NotFound();

            return NoContent();
        }
    }
}
