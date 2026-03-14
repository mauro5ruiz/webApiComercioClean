using Comercio.Application.Interfaces;
using Comercio.Domain.Entidades;
using Comercio.Domain.Enums;
using Comercio.Domain.Interfaces;

namespace Comercio.Application.Servicios
{
    public class AjusteStockServicio : IAjusteStockServicio
    {
        private readonly IMovimientosStockRepository _movimientosRepository;
        private readonly IProductosRepository _productosRepository;

        public AjusteStockServicio(IMovimientosStockRepository movimientosRepository,IProductosRepository productosRepository)
        {
            _movimientosRepository = movimientosRepository;
            _productosRepository = productosRepository;
        }

        public async Task AjustarStock(int idProducto, int stockReal, string motivo)
        {
            var producto = await _productosRepository.ObtenerPorId(idProducto);

            if (producto == null)
                throw new Exception("Producto no existe");

            var stockSistema = await _movimientosRepository.ObtenerStockActual(idProducto);

            var diferencia = stockReal - stockSistema;

            if (diferencia == 0)
                throw new Exception("No hay diferencia de stock");

            var movimiento = new MovimientoStock
            {
                IdProducto = idProducto,
                Cantidad = diferencia,
                IdTipoMovimientoStock = TipoMovimientoStock.AjusteStock,
                Fecha = DateTime.UtcNow,
                Observaciones = motivo
            };

            await _movimientosRepository.RegistrarMovimiento(movimiento);
        }
    }
}
