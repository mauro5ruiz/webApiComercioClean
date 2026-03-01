using Comercio.Application.Dtos.Ofertas;
using Comercio.Application.Interfaces;
using Comercio.Domain.Entidades;
using Microsoft.AspNetCore.Mvc;

namespace Comercio.Api.Controllers
{
    [ApiController]
    [Route("api/ofertas")]
    public class OfertasController : Controller
    {
        private readonly IOfertasServicio _ofertasServicio;

        public OfertasController(IOfertasServicio ofertasServicio)
        {
            _ofertasServicio = ofertasServicio;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTodas([FromQuery] bool incluirVencidas = false)
        {
            var ofertas = await _ofertasServicio.ObtenerTodas(incluirVencidas);

            return Ok(ofertas);
        }

        // GET: api/ofertas/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var oferta = await _ofertasServicio.ObtenerPorId(id);

            if (oferta == null) return NotFound();

            return Ok(oferta);
        }

        // GET: api/ofertas/producto/3
        [HttpGet("producto/{productoId:int}")]
        public async Task<IActionResult> ObtenerOfertaActiva(int productoId)
        {
            var oferta = await _ofertasServicio.ObtenerOfertaActivaVigente(productoId);

            if (oferta == null) return NotFound();

            return Ok(oferta);
        }

        // POST: api/ofertas
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearOfertaDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var oferta = new Oferta
            {
                IdProducto = dto.IdProducto,
                TipoDescuento = dto.TipoDescuento,
                ValorDescuento = dto.ValorDescuento,
                FechaInicio = dto.FechaInicio,
                FechaFin = dto.FechaFin,
                Activa = true
            };

            var id = await _ofertasServicio.CrearOferta(oferta);

            return CreatedAtAction(nameof(ObtenerPorId), new { id }, oferta);
        }

        // PUT: api/ofertas/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] CrearOfertaDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ofertaExistente = await _ofertasServicio.ObtenerPorId(id);

            if (ofertaExistente == null)
                return NotFound();

            ofertaExistente.IdProducto = dto.IdProducto;
            ofertaExistente.TipoDescuento = dto.TipoDescuento;
            ofertaExistente.ValorDescuento = dto.ValorDescuento;
            ofertaExistente.FechaInicio = dto.FechaInicio;
            ofertaExistente.FechaFin = dto.FechaFin;

            await _ofertasServicio.ActualizarOferta(ofertaExistente);

            return NoContent();
        }

        // PATCH: api/ofertas/desactivar/3
        [HttpPatch("desactivar/{productoId:int}")]
        public async Task<IActionResult> DesactivarPorProducto(int productoId)
        {
            await _ofertasServicio.DesactivarOfertasPorProducto(productoId);

            return NoContent();
        }
    }
}
