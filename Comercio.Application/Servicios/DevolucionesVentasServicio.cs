using Comercio.Application.Interfaces;
using Comercio.Domain.Entidades;
using Comercio.Domain.Enums;
using Comercio.Domain.Interfaces;

namespace Comercio.Application.Servicios
{
    public class DevolucionesVentasServicio : IDevolucionesVentasServicio
    {
        private readonly IDevolucionesVentasRepository _devolucionesRepository;
        private readonly IDetalleDevolucionesVentasRepository _detalleRepository;
        private readonly IDevolucionPagosRepository _pagosRepository;
        private readonly IVentasRepository _ventasRepository;
        private readonly IDetalleVentasRepostory _detalleVentasRepository;
        private readonly IProductosRepository _productosRepository;
        private readonly IMovimientosStockRepository _movimientosStockRepository;

        public DevolucionesVentasServicio(
            IDevolucionesVentasRepository devolucionesRepository,
            IDetalleDevolucionesVentasRepository detalleRepository,
            IDevolucionPagosRepository pagosRepository,
            IVentasRepository ventasRepository,
            IDetalleVentasRepostory detalleVentasRepository,
            IProductosRepository productosRepository,
            IMovimientosStockRepository movimientosStockRepository)
        {
            _devolucionesRepository = devolucionesRepository;
            _detalleRepository = detalleRepository;
            _pagosRepository = pagosRepository;
            _ventasRepository = ventasRepository;
            _detalleVentasRepository = detalleVentasRepository;
            _productosRepository = productosRepository;
            _movimientosStockRepository = movimientosStockRepository;
        }

        public async Task<int> RegistrarDevolucion(
            DevolucionVenta devolucion,
            IEnumerable<DetalleDevolucionVenta> detalles,
            IEnumerable<DevolucionVentaPago>? pagos = null)
        {
            if (devolucion is null)
                throw new ArgumentNullException(nameof(devolucion));

            if (detalles is null || !detalles.Any())
                throw new ArgumentException("La devolución debe tener al menos un producto.");

            var venta = await _ventasRepository.ObtenerPorId(devolucion.IdVenta);

            if (venta is null)
                throw new InvalidOperationException("La venta no existe.");

            if (venta.Estado == "Anulada")
                throw new InvalidOperationException("No se puede devolver una venta anulada.");

            var detallesVenta = (await _detalleVentasRepository.ObtenerPorVenta(venta.Id)).ToList();

            decimal total = 0;

            foreach (var detalle in detalles)
            {
                var detalleVenta = detallesVenta.FirstOrDefault(x => x.Id == detalle.IdVentaDetalle);

                if (detalleVenta is null)
                    throw new InvalidOperationException($"El detalle de venta {detalle.IdVentaDetalle} no existe.");

                if (detalle.Cantidad <= 0)
                    throw new InvalidOperationException("La cantidad a devolver debe ser mayor a cero.");

                if (detalle.Cantidad > detalleVenta.Cantidad)
                    throw new InvalidOperationException("No se puede devolver más cantidad de la vendida.");

                detalle.PrecioUnitario = detalleVenta.PrecioUnitario;
                detalle.Subtotal = detalle.Cantidad * detalle.PrecioUnitario;

                total += detalle.Subtotal;
            }

            devolucion.Total = total;
            devolucion.Fecha = DateTime.UtcNow;
            devolucion.Estado = "Activa";

            var idDevolucion = await _devolucionesRepository.Insertar(devolucion);

            foreach (var detalle in detalles)
            {
                detalle.IdDevolucionVenta = idDevolucion;

                await _detalleRepository.Insertar(detalle);

                var producto = await _productosRepository.ObtenerPorId(detalle.IdProducto);

                if (producto is null)
                    throw new InvalidOperationException($"Producto {detalle.IdProducto} no existe.");

                var movimiento = new MovimientoStock
                {
                    IdProducto = detalle.IdProducto,
                    Cantidad = detalle.Cantidad, // vuelve al stock
                    IdTipoMovimientoStock = TipoMovimientoStock.DevolucionVenta,
                    Fecha = DateTime.UtcNow,
                    IdReferencia = idDevolucion,
                    Observaciones = "Devolución de venta"
                };

                await _movimientosStockRepository.RegistrarMovimiento(movimiento);
            }

            if (pagos != null && pagos.Any())
            {
                foreach (var pago in pagos)
                {
                    pago.IdDevolucionVenta = idDevolucion;
                    pago.FechaPago = DateTime.UtcNow;
                    pago.Estado = "Activo";

                    await _pagosRepository.Insertar(pago);
                }
            }

            return idDevolucion;
        }

        public async Task<DevolucionVenta?> ObtenerPorId(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Id inválido.");

            var devolucion = await _devolucionesRepository.ObtenerPorId(id);

            if (devolucion is null)
                return null;

            devolucion.Detalles = (await _detalleRepository.ObtenerPorDevolucion(id)).ToList();
            devolucion.Pagos = (await _pagosRepository.ObtenerPorDevolucion(id)).ToList();

            return devolucion;
        }

        public async Task<IEnumerable<DevolucionVenta>> ObtenerEntreFechas(DateTime desde, DateTime hasta)
        {
            if (desde > hasta)
                throw new ArgumentException("La fecha 'desde' no puede ser mayor que 'hasta'.");

            return await _devolucionesRepository.ObtenerPorFechas(desde, hasta);
        }
    }
}