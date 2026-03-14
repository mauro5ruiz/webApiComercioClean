using Comercio.Application.Interfaces;
using Comercio.Domain.Entidades;
using Comercio.Domain.Enums;
using Comercio.Domain.Interfaces;

namespace Comercio.Application.Servicios
{
    public class DevolucionesComprasServicio : IDevolucionesComprasServicio
    {
        private readonly IDevolucionComprasRepository _devolucionesRepository;
        private readonly IDevolucionCompraDetalleRepository _detalleRepository;
        private readonly IPagoDevolucionCompraRepository _pagosRepository;
        private readonly IComprasRepostory _comprasRepository;
        private readonly IDetalleComprasRepository _detalleComprasRepository;
        private readonly IProductosRepository _productosRepository;
        private readonly IMovimientosStockRepository _movimientosStockRepository;
        private readonly ICreditoProveedorRepository _creditoProveedorRepository;

        public DevolucionesComprasServicio(
            IDevolucionComprasRepository devolucionesRepository,
            IDevolucionCompraDetalleRepository detalleRepository,
            IPagoDevolucionCompraRepository pagosRepository,
            IComprasRepostory comprasRepository,
            IDetalleComprasRepository detalleComprasRepository,
            IProductosRepository productosRepository,
            IMovimientosStockRepository movimientosStockRepository,
            ICreditoProveedorRepository creditoProveedorRepository)
        {
            _devolucionesRepository = devolucionesRepository;
            _detalleRepository = detalleRepository;
            _pagosRepository = pagosRepository;
            _comprasRepository = comprasRepository;
            _detalleComprasRepository = detalleComprasRepository;
            _productosRepository = productosRepository;
            _movimientosStockRepository = movimientosStockRepository;
            _creditoProveedorRepository = creditoProveedorRepository;
        }

        public async Task<int> RegistrarDevolucion(
            DevolucionCompra devolucion,
            IEnumerable<DevolucionCompraDetalle> detalles,
            IEnumerable<PagoDevolucionCompra>? pagos = null)
        {
            if (devolucion is null)
                throw new ArgumentNullException(nameof(devolucion));

            if (detalles is null || !detalles.Any())
                throw new ArgumentException("La devolución debe tener al menos un producto.");

            var compra = await _comprasRepository.ObtenerPorId(devolucion.IdCompra);

            if (compra is null)
                throw new InvalidOperationException("La compra no existe.");

            if (compra.Estado == EstadoComprobante.Anulada) 
                throw new InvalidOperationException("No se puede devolver una compra anulada.");

            var detallesCompra = (await _detalleComprasRepository.ObtenerPorCompra(compra.Id)).ToList();

            decimal total = 0;

            foreach (var detalle in detalles)
            {
                var detalleCompra = detallesCompra.FirstOrDefault(x => x.IdProducto == detalle.IdProducto);

                if (detalleCompra is null)
                    throw new InvalidOperationException($"El producto {detalle.IdProducto} no existe en la compra.");

                if (detalle.Cantidad <= 0)
                    throw new InvalidOperationException("La cantidad a devolver debe ser mayor a cero.");

                if (detalle.Cantidad > detalleCompra.Cantidad)
                    throw new InvalidOperationException("No se puede devolver más cantidad de la comprada.");

                detalle.PrecioUnitario = detalleCompra.PrecioUnitario;
                detalle.Subtotal = detalle.Cantidad * detalle.PrecioUnitario;

                total += detalle.Subtotal;
            }

            devolucion.Total = total;
            devolucion.Fecha = DateTime.UtcNow;
            devolucion.Estado = 1;

            var idDevolucion = await _devolucionesRepository.Insertar(devolucion);

            foreach (var detalle in detalles)
            {
                detalle.IdDevolucionCompra = idDevolucion;

                await _detalleRepository.Insertar(detalle);

                var producto = await _productosRepository.ObtenerPorId(detalle.IdProducto);

                if (producto is null)
                    throw new InvalidOperationException($"Producto {detalle.IdProducto} no existe.");

                var movimiento = new MovimientoStock
                {
                    IdProducto = detalle.IdProducto,
                    Cantidad = -detalle.Cantidad, // sale del stock porque vuelve al proveedor
                    IdTipoMovimientoStock = TipoMovimientoStock.DevolucionCompra,
                    Fecha = DateTime.UtcNow,
                    IdReferencia = idDevolucion,
                    Observaciones = "Devolución de compra"
                };

                await _movimientosStockRepository.RegistrarMovimiento(movimiento);
            }

            if (pagos != null && pagos.Any())
            {
                foreach (var pago in pagos)
                {
                    pago.IdDevolucionCompra = idDevolucion;
                    pago.Fecha = DateTime.UtcNow;

                    await _pagosRepository.Insertar(pago);
                }
            }
            else
            {
                var credito = new CreditoProveedor
                {
                    IdProveedor = devolucion.IdProveedor,
                    IdDevolucionCompra = idDevolucion,
                    Importe = total,
                    Saldo = total,
                    Fecha = DateTime.UtcNow
                };

                await _creditoProveedorRepository.Insertar(credito);
            }

            return idDevolucion;
        }

        public async Task<DevolucionCompra?> ObtenerPorId(int id)
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

        public async Task<IEnumerable<DevolucionCompra>> ObtenerEntreFechas(DateTime desde, DateTime hasta)
        {
            if (desde > hasta)
                throw new ArgumentException("La fecha 'desde' no puede ser mayor que 'hasta'.");

            return await _devolucionesRepository.ObtenerPorFechas(desde, hasta);
        }
    }
}