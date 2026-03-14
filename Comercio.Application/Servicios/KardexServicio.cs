using Comercio.Application.Interfaces;
using Comercio.Domain.Entidades;
using Comercio.Domain.Interfaces;

namespace Comercio.Application.Servicios
{
    public class KardexServicio: IKardexServicio
    {
        private readonly IMovimientosStockRepository _movimientosRepository;
        private readonly IProductosRepository _productosRepository;

        public KardexServicio(IMovimientosStockRepository movimientosRepository, IProductosRepository productosRepository)
        {
            _movimientosRepository = movimientosRepository;
            _productosRepository = productosRepository;
        }

        public async Task<IEnumerable<MovimientoStock>> ObtenerKardex(int idProducto)
        {
            var producto = await _productosRepository.ObtenerPorId(idProducto);
            if(producto == null || producto.Id <= 0)
                throw new ArgumentException("Producto no encontrado.");

            var movimientos = await _movimientosRepository.ObtenerPorProducto(idProducto);

            return movimientos.Select(m => new MovimientoStock
            {
                Fecha = m.Fecha,
                Cantidad = m.Cantidad,
                IdTipoMovimientoStock = m.IdTipoMovimientoStock,
                IdReferencia = m.IdReferencia,
                Observaciones = m.Observaciones
            });
        }
    }
}
