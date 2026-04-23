using Comercio.Application.Dtos.Proveedores;
using Comercio.Application.Interfaces;
using Comercio.Domain.Entidades;
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
                var proveedor = await _proveedoresServicio.Crear(dto);

                return CreatedAtAction(nameof(ObtenerPorId),new { id = proveedor.Id }, proveedor);
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
                var proveedor = await _proveedoresServicio.Actualizar(id, dto);

                return Ok(proveedor);
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
            await _proveedoresServicio.EliminarPermanentemente(id);

            return NoContent();
        }

        [HttpPatch("{id:int}/desactivar")]
        public async Task<IActionResult> Desactivar(int id)
        {
            var ok = await _proveedoresServicio.DarDeBaja(id);

            if (!ok)
                return NotFound(new { error = "Proveedor no encontrado" });

            return NoContent();
        }

        [HttpPatch("{id:int}/activar")]
        public async Task<IActionResult> Activar(int id)
        {
            var ok = await _proveedoresServicio.Restaurar(id);

            if (!ok)
                return NotFound(new { error = "Proveedor no encontrado" });

            return NoContent();
        }
    }
}
