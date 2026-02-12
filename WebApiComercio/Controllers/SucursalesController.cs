using Comercio.Application.Dtos.Marcas;
using Comercio.Application.Dtos.Sucursales;
using Comercio.Application.Interfaces;
using Comercio.Application.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace Comercio.Api.Controllers
{
    [ApiController]
    [Route("api/sucursales")]
    public class SucursalesController : Controller
    {
        private readonly ISucursalesServicio _sucursalesServicio;

        public SucursalesController(ISucursalesServicio sucursalesServicio)
        {
            _sucursalesServicio = sucursalesServicio;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SucursalDto>>> Obtener()
        {
            var sucursales = await _sucursalesServicio.ObtenerTodas();
            return Ok(sucursales);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<SucursalDto>> ObtenerPorId([FromRoute] int id)
        {
            var sucursal = await _sucursalesServicio.ObtenerPorId(id);
            if (sucursal is null)
                return NotFound();

            return Ok(sucursal);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Crear([FromForm] CrearSucursalDto dto)
        {
            try
            {
                var id = await _sucursalesServicio.Crear(dto);
                return CreatedAtAction(nameof(ObtenerPorId), new { id }, id);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar([FromRoute] int id, [FromForm] CrearSucursalDto dto)
        {
            try
            {
                var actualizado = await _sucursalesServicio.Actualizar(id, dto);
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
            var eliminado = await _sucursalesServicio.Eliminar(id);
            if (!eliminado)
                return NotFound();

            return NoContent();
        }

        [HttpPatch("{id:int}/baja")]
        public async Task<IActionResult> DarDeBaja([FromRoute] int id)
        {
            var baja = await _sucursalesServicio.DarDeBaja(id);
            if (!baja)
                return NotFound();

            return NoContent();
        }

        [HttpPatch("{id:int}/restauracion")]
        public async Task<IActionResult> Restaurar([FromRoute] int id)
        {
            var restauracion = await _sucursalesServicio.Restaurar(id);
            if (!restauracion)
                return NotFound();

            return NoContent();
        }
    }
}
