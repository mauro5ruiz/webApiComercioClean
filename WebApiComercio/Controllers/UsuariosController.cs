using Comercio.Application.Dtos.Proveedores;
using Comercio.Application.Dtos.Usuarios;
using Comercio.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Comercio.Api.Controllers
{
    [ApiController]
    [Route("api/usuarios")]
    public class UsuariosController : Controller
    {
        private readonly IUsuariosServicio _usuariosServicio;

        public UsuariosController(IUsuariosServicio usuariosServicio)
        {
            _usuariosServicio = usuariosServicio;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsuarioDto>>> Obtener([FromQuery] bool incluirEliminados = false)
        {
            var usuarios = await _usuariosServicio.ObtenerTodos(incluirEliminados);
            return Ok(usuarios);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<UsuarioDto>> ObtenerPorId(int id)
        {
            var usuario = await _usuariosServicio.ObtenerPorId(id);

            if (usuario == null)
                return NotFound();

            return Ok(usuario);
        }

        [HttpGet("sucursal/{sucursalId:int}")]
        public async Task<ActionResult<IEnumerable<UsuarioDto>>> ObtenerPorSucursal(int sucursalId)
        {
            var usuarios = await _usuariosServicio.ObtenerPorSucursal(sucursalId);
            return Ok(usuarios);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Crear([FromBody] CrearUsuarioDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var id = await _usuariosServicio.Crear(dto);

            return CreatedAtAction(nameof(ObtenerPorId), new { id }, null);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarUsuarioDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _usuariosServicio.Actualizar(id, dto);

            return NoContent();
        }

        [HttpPut("activacion/{id:int}")]
        public async Task<IActionResult> Activar(int id)
        {
            var resultado = await _usuariosServicio.Activar(id);

            if (!resultado)
                return NotFound();

            return NoContent();
        }

        [HttpPut("desactivacion/{id:int}")]
        public async Task<IActionResult> Desactivar(int id)
        {
            var resultado = await _usuariosServicio.Desactivar(id);

            if (!resultado)
                return NotFound();

            return NoContent();
        }

        [HttpPut("cambio-clave")]
        public async Task<IActionResult> CambiarClave([FromBody] CambiarClaveDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _usuariosServicio.CambiarClave(dto);

            return NoContent();
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var usuario = await _usuariosServicio.Login(dto);

            if (usuario == null)
                return Unauthorized("Usuario o contraseña incorrectos.");

            return Ok(usuario);
        }
    }
}
