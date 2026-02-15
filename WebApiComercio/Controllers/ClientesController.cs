using Comercio.Application.Dtos.Clientes;
using Comercio.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Comercio.Api.Controllers
{
    [ApiController]
    [Route("api/clientes")]
    public class ClientesController : Controller
    {
        private readonly IClientesServicio _clientesServicio;

        public ClientesController(IClientesServicio clientesServicio)
        {
            _clientesServicio = clientesServicio;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClienteDto>>> Obtener([FromQuery] bool incluirEliminados = false)
        {
            var clientes = await _clientesServicio.ObtenerTodos(incluirEliminados);
            return Ok(clientes);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ClienteDto>> ObtenerPorId([FromRoute] int id)
        {
            var cliente = await _clientesServicio.ObtenerPorId(id);

            if (cliente is null)
                return NotFound();

            return Ok(cliente);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Crear([FromForm] CrearClienteDto dto)
        {
            try
            {
                var id = await _clientesServicio.Crear(dto);

                return CreatedAtAction(nameof(ObtenerPorId), new { id }, id);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Actualizar([FromRoute] int id, [FromForm] ActualizarClienteDto dto)
        {
            try
            {
                await _clientesServicio.Actualizar(id, dto);

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
            var eliminado = await _clientesServicio.EliminarPermanentemente(id);

            if (!eliminado)
                return NotFound();

            return NoContent();
        }

        [HttpPatch("{id:int}/baja")]
        public async Task<IActionResult> DarDeBaja([FromRoute] int id)
        {
            var baja = await _clientesServicio.DarDeBaja(id);

            if (!baja)
                return NotFound();

            return NoContent();
        }

        [HttpPatch("{id:int}/restauracion")]
        public async Task<IActionResult> Restaurar([FromRoute] int id)
        {
            var restaurado = await _clientesServicio.Restaurar(id);

            if (!restaurado)
                return NotFound();

            return NoContent();
        }
    }
}
