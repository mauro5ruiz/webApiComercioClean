using Comercio.Application.Dtos.AjustesStock;
using Comercio.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Comercio.Api.Controllers
{
    [ApiController]
    [Route("api/ajustes-stock")]
    public class AjustesStockController : ControllerBase
    {
        private readonly IAjusteStockServicio _ajusteStockServicio;

        public AjustesStockController(IAjusteStockServicio ajusteStockServicio)
        {
            _ajusteStockServicio = ajusteStockServicio;
        }

        [HttpPost]
        public async Task<IActionResult> AjustarStock([FromBody] AjustesStockDto dto)
        {
            try
            {
                await _ajusteStockServicio.AjustarStock(dto.IdProducto,dto.StockReal,dto.Motivo);

                return Ok(new { mensaje = "Stock ajustado correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}