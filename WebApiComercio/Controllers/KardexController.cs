using Comercio.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Comercio.Api.Controllers
{
    [ApiController]
    [Route("api/kardex")]
    public class KardexController : ControllerBase
    {
        private readonly IKardexServicio _kardexServicio;

        public KardexController(IKardexServicio kardexServicio)
        {
            _kardexServicio = kardexServicio;
        }

        [HttpGet("{idProducto}")]
        public async Task<IActionResult> ObtenerKardex(int idProducto)
        {
            try
            {
                var kardex = await _kardexServicio.ObtenerKardex(idProducto);

                return Ok(kardex);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
        }
    }
}