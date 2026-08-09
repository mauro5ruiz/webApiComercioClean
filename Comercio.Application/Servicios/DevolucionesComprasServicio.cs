using Comercio.Application.Interfaces;
using Comercio.Domain.Entidades;
using Comercio.Domain.Enums;
using Comercio.Domain.Interfaces;
using System.Transactions;

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

        public DevolucionesComprasServicio(IDevolucionComprasRepository devolucionesRepository, IDevolucionCompraDetalleRepository detalleRepository,
            IPagoDevolucionCompraRepository pagosRepository, IComprasRepostory comprasRepository, IDetalleComprasRepository detalleComprasRepository,
            IProductosRepository productosRepository, IMovimientosStockRepository movimientosStockRepository, ICreditoProveedorRepository creditoProveedorRepository)
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

        public async Task<int> RegistrarDevolucion(DevolucionCompra devolucion,IEnumerable<DevolucionCompraDetalle> detalles,IEnumerable<PagoDevolucionCompra>? pagos = null)
        {
            if (devolucion is null)
                throw new ArgumentNullException(nameof(devolucion));

            var detallesList = detalles?.ToList() ?? throw new ArgumentException("La devolución debe tener al menos un producto.");

            if (!detallesList.Any())
                throw new ArgumentException("La devolución debe tener al menos un producto.");

            var compra = await _comprasRepository.ObtenerPorId(devolucion.IdCompra);

            if (compra is null)
                throw new InvalidOperationException("La compra no existe.");

            if (compra.Estado == EstadoComprobante.Anulada) 
                throw new InvalidOperationException("No se puede devolver una compra anulada.");

            devolucion.IdProveedor = compra.IdProveedor;

            var detallesCompra = (await _detalleComprasRepository.ObtenerPorCompra(compra.Id)).ToList();
            var pagosList = pagos?.ToList() ?? new List<PagoDevolucionCompra>();

            decimal total = 0;
            var cantidadDisponiblePorProducto = detallesCompra.ToDictionary(d => d.IdProducto, d => d.Cantidad);
            var cantidadSolicitadaPorProducto = new Dictionary<int, int>();

            foreach (var detalle in detallesList)
            {
                var detalleCompra = detallesCompra.FirstOrDefault(x => x.IdProducto == detalle.IdProducto);

                if (detalleCompra is null)
                    throw new InvalidOperationException($"El producto {detalle.IdProducto} no existe en la compra.");

                if (detalle.Cantidad <= 0)
                    throw new InvalidOperationException("La cantidad a devolver debe ser mayor a cero.");

                var disponible = cantidadDisponiblePorProducto[detalle.IdProducto];

                if (detalle.Cantidad > disponible)
                    throw new InvalidOperationException("No se puede devolver más cantidad de la comprada.");

                cantidadDisponiblePorProducto[detalle.IdProducto] = disponible - detalle.Cantidad;
                cantidadSolicitadaPorProducto[detalle.IdProducto] = cantidadSolicitadaPorProducto.GetValueOrDefault(detalle.IdProducto) + detalle.Cantidad;

                detalle.PrecioUnitario = detalleCompra.PrecioUnitario;
                detalle.Subtotal = detalle.Cantidad * detalle.PrecioUnitario;

                total += detalle.Subtotal;
            }

            foreach (var (idProducto, cantidadSolicitada) in cantidadSolicitadaPorProducto)
            {
                var stockActual = await _movimientosStockRepository.ObtenerStockActual(idProducto);

                if (cantidadSolicitada > stockActual)
                    throw new InvalidOperationException($"Stock insuficiente del producto {idProducto} para realizar la devolución (disponible: {stockActual}).");
            }

            devolucion.Total = total;
            devolucion.Fecha = DateTime.Now;
            devolucion.Estado = 1;

            var totalPagadoCompra = compra.TotalPagado;
            var totalCanceladoCompra = compra.TotalPagado + compra.CreditoAplicado;
            var totalPagadoEnDevolucion = 0m;

            foreach (var pago in pagosList)
            {
                if (pago.IdFormaPago <= 0)
                    throw new InvalidOperationException("La forma de pago de la devolución es obligatoria.");

                if (pago.Importe <= 0)
                    throw new InvalidOperationException("Los pagos de la devolución deben ser mayores a cero.");

                totalPagadoEnDevolucion += pago.Importe;
            }

            var maximoRefundableEnPagos = Math.Min(devolucion.Total, totalPagadoCompra);

            if (totalPagadoEnDevolucion > maximoRefundableEnPagos)
                throw new InvalidOperationException("Los pagos de la devolución no pueden superar lo efectivamente pagado ni el total devuelto.");

            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            var idDevolucion = await _devolucionesRepository.Insertar(devolucion);

            foreach (var detalle in detallesList)
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
                    Fecha = DateTime.Now,
                    IdReferencia = idDevolucion,
                    Observaciones = "Devolución de compra"
                };

                await _movimientosStockRepository.RegistrarMovimiento(movimiento);

                int? idDetalleCompra = await _detalleComprasRepository.ObtenerIdDetalleCompra(compra.Id, detalle.IdProducto);
                if(idDetalleCompra.HasValue)
                    await _detalleComprasRepository.AgregarCantidadDevuelto(idDetalleCompra.Value, detalle.IdProducto, detalle.Cantidad);
            }

            foreach (var pago in pagosList)
            {
                pago.IdDevolucionCompra = idDevolucion;
                pago.Fecha = DateTime.Now;

                await _pagosRepository.Insertar(pago);
            }

            var creditoGenerado = Math.Min(devolucion.Total, totalCanceladoCompra) - totalPagadoEnDevolucion;

            if (creditoGenerado > 0)
            {
                var credito = new CreditoProveedor
                {
                    IdProveedor = devolucion.IdProveedor,
                    IdDevolucionCompra = idDevolucion,
                    Importe = creditoGenerado,
                    Saldo = creditoGenerado,
                    Fecha = DateTime.Now
                };

                await _creditoProveedorRepository.Insertar(credito);
            }

            var reducirTotalPagado = maximoRefundableEnPagos;
            var reducirCreditoAplicado = Math.Min(devolucion.Total, totalCanceladoCompra) - reducirTotalPagado;

            await _comprasRepository.RegistrarDevolucion(compra.Id, devolucion.Total, reducirTotalPagado, reducirCreditoAplicado);

            scope.Complete();
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
