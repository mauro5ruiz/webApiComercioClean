using Comercio.Application.Dtos.Proveedores;
using Comercio.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Comercio.Api.Controllers
{
    [ApiController]
    [Route("api/proveedores")]
    public class ProveedoresController : Controller
    {
        private readonly IProveedoresServicio _proveedoresServicio;

        public ProveedoresController(IProveedoresServicio proveedoresServicio)
        {
            _proveedoresServicio = proveedoresServicio;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProveedorDto>>> Obtener([FromQuery] bool incluirEliminados = false)
        {
            var proveedores = await _proveedoresServicio.ObtenerTodos(incluirEliminados);
            return Ok(proveedores);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProveedorDto>> ObtenerPorId([FromRoute] int id)
        {
            var proveedor = await _proveedoresServicio.ObtenerPorId(id);

            if (proveedor is null)
                return NotFound();

            return Ok(proveedor);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Crear([FromForm] CrearProveedorDto dto)
        {
            try
            {
                var id = await _proveedoresServicio.Crear(dto);

                return CreatedAtAction(nameof(ObtenerPorId),new { id }, id);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Actualizar([FromRoute] int id, [FromForm] ActualizarProveedorDto dto)
        {
            try
            {
                await _proveedoresServicio.Actualizar(id, dto);

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
            var eliminado = await _proveedoresServicio.EliminarPermanentemente(id);

            if (!eliminado)
                return NotFound();

            return NoContent();
        }

        [HttpPatch("{id:int}/baja")]
        public async Task<IActionResult> DarDeBaja([FromRoute] int id)
        {
            var baja = await _proveedoresServicio.DarDeBaja(id);

            if (!baja)
                return NotFound();

            return NoContent();
        }

        [HttpPatch("{id:int}/restauracion")]
        public async Task<IActionResult> Restaurar([FromRoute] int id)
        {
            var restaurado = await _proveedoresServicio.Restaurar(id);

            if (!restaurado)
                return NotFound();

            return NoContent();
        }
    }
}
