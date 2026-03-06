using Comercio.Application.Interfaces;
using Comercio.Domain.Entidades;
using Comercio.Domain.Enums;
using Comercio.Domain.Interfaces;

namespace Comercio.Application.Servicios
{
    public class PerdidasServicio: IPerdidasServicio
    {
        private readonly IPerdidasRepository _perdidasRepository;
        private readonly IDetallePerdidasRepository _detalleRepository;
        private readonly IMovimientosStockRepository _movimientosStockRepository;

        public PerdidasServicio(IPerdidasRepository perdidasRepository, IDetallePerdidasRepository detalleRepository, IMovimientosStockRepository movimientosStockRepository)
        {
            _perdidasRepository = perdidasRepository;
            _detalleRepository = detalleRepository;
            _movimientosStockRepository = movimientosStockRepository;
        }

        public async Task<IEnumerable<Perdida>> ObtenerPorFechas(DateTime desde, DateTime hasta)
        {
            return await _perdidasRepository.ObtenerPorFechas(desde, hasta);
        }

        public async Task<Perdida?> ObtenerPorId(int idPerdida)
        {
            return await _perdidasRepository.ObtenerPorId(idPerdida);
        }

        public async Task<Perdida> ObtenerPerdidaCompleta(int idPerdida)
        {
            var perdida = await _perdidasRepository.ObtenerPorId(idPerdida);

            if (perdida == null)
                throw new Exception("La pérdida no existe");

            var detalles = await _detalleRepository.ObtenerPorPerdida(idPerdida);

            perdida.Detalles = detalles.ToList();

            return perdida;
        }

        public async Task<int> CrearPerdida(Perdida perdida, List<DetallePerdida> detalles)
        {
            var idPerdida = await _perdidasRepository.Insertar(perdida);

            foreach (var detalle in detalles)
            {
                detalle.IdPerdida = idPerdida;
                await _detalleRepository.Insertar(detalle);
                var movimiento = new MovimientoStock
                {
                    IdProducto = detalle.IdProducto,
                    Cantidad = -detalle.Cantidad,
                    IdTipoMovimientoStock = TipoMovimientoStock.Perdida,
                    Fecha = DateTime.UtcNow,
                    IdReferencia = idPerdida,
                    Observaciones = "Perdida registrada"
                };
                await _movimientosStockRepository.RegistrarMovimiento(movimiento);
            }

            return idPerdida;
        }

        public async Task AgregarDetalle(DetallePerdida detalle)
        {
            await _detalleRepository.Insertar(detalle);
        }

        public async Task EliminarDetalle(int idDetalle)
        {
            var detalle = await _detalleRepository.ObtenerPorId(idDetalle);

            if (detalle == null)
                throw new Exception("El detalle no existe");

            // movimiento inverso
            var movimiento = new MovimientoStock
            {
                IdProducto = detalle.IdProducto,
                Cantidad = detalle.Cantidad, // devuelve el stock
                IdTipoMovimientoStock = TipoMovimientoStock.AnulacionPerdida,
                Fecha = DateTime.UtcNow,
                IdReferencia = detalle.IdPerdida,
                Observaciones = "Eliminación de detalle de pérdida"
            };

            await _movimientosStockRepository.RegistrarMovimiento(movimiento);

            await _detalleRepository.Eliminar(idDetalle);
        }

        public async Task ActualizarPerdida(int idPerdida, string motivo, string observacion)
        {
            await _perdidasRepository.Actualizar(idPerdida, motivo, observacion);
        }

        public async Task ActualizarDetalle(DetallePerdida detalle)
        {
            var detalleActual = await _detalleRepository.ObtenerPorId(detalle.Id);

            if (detalleActual == null)
                throw new Exception("El detalle no existe");

            var diferencia = detalle.Cantidad - detalleActual.Cantidad;

            if (diferencia != 0)
            {
                var movimiento = new MovimientoStock
                {
                    IdProducto = detalle.IdProducto,
                    Cantidad = -diferencia,
                    IdTipoMovimientoStock = TipoMovimientoStock.AjustePerdida,
                    Fecha = DateTime.UtcNow,
                    IdReferencia = detalle.IdPerdida,
                    Observaciones = "Ajuste de pérdida"
                };

                await _movimientosStockRepository.RegistrarMovimiento(movimiento);
            }

            await _detalleRepository.Actualizar(detalle);
        }
    }
}
