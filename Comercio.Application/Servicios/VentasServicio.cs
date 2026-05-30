using Comercio.Application.Interfaces;
using Comercio.Domain.Entidades;
using Comercio.Domain.Enums;
using Comercio.Domain.Interfaces;

namespace Comercio.Application.Servicios
{
    public class VentasServicio : IVentasServicio
    {
        private const string EstadoVentaActiva = "Activa";
        private const string EstadoVentaAnulada = "Anulada";
        private const string EstadoPagoActivo = "Activo";
        private const string EstadoPagoAnulado = "Anulado";

        private readonly IVentasRepository _ventasRepository;
        private readonly IDetalleVentasRepostory _detalleRepository;
        private readonly IVentasPagosRepository _pagosRepository;
        private readonly IDevolucionesVentasRepository _devolucionesRepository;
        private readonly IDetalleDevolucionesVentasRepository _devolucionDetalleRepository;
        private readonly IDevolucionPagosRepository _devolucionPagosRepository;
        private readonly IProductosRepository _productosRepository;
        private readonly IMovimientosStockRepository _movimientosStockRepository;
        private readonly ICreditoClienteRepository _creditoClienteRepository;

        public VentasServicio(IVentasRepository ventasRepository,IDetalleVentasRepostory detalleRepository,IVentasPagosRepository pagosRepository,
            IDevolucionesVentasRepository devolucionesRepository,IDetalleDevolucionesVentasRepository devolucionDetalleRepository,IDevolucionPagosRepository devolucionPagosRepository,
            IProductosRepository productosRepository,IMovimientosStockRepository movimientosStockRepository,ICreditoClienteRepository creditoClienteRepository)
        {
            _ventasRepository = ventasRepository;
            _detalleRepository = detalleRepository;
            _pagosRepository = pagosRepository;
            _devolucionesRepository = devolucionesRepository;
            _devolucionDetalleRepository = devolucionDetalleRepository;
            _devolucionPagosRepository = devolucionPagosRepository;
            _productosRepository = productosRepository;
            _movimientosStockRepository = movimientosStockRepository;
            _creditoClienteRepository = creditoClienteRepository;
        }

        public async Task<IEnumerable<Venta>> ObtenerEntreFechas(DateTime desde, DateTime hasta)
        {
            if (desde > hasta)
                throw new ArgumentException("La fecha 'desde' no puede ser mayor que 'hasta'.");

            return await _ventasRepository.ObtenerPorFechas(desde, hasta);
        }

        public async Task<Venta?> ObtenerPorId(int idVenta)
        {
            if (idVenta <= 0)
                throw new ArgumentException("Id inválido.");

            var venta = await _ventasRepository.ObtenerPorId(idVenta);

            if (venta is null)
                return null;

            venta.Detalles = (await _detalleRepository.ObtenerPorVenta(idVenta)).ToList();
            venta.Pagos = (await _pagosRepository.ObtenerPorVenta(idVenta)).ToList();


            return venta;
        }

        public async Task<int> CrearVenta(Venta venta, IEnumerable<DetalleVenta> detalles, IEnumerable<VentaPago>? pagos = null)
        {
            if (venta is null)
                throw new ArgumentNullException(nameof(venta));

            if (detalles is null || !detalles.Any())
                throw new ArgumentException("La venta debe tener al menos un detalle.");

            var erroresStock = new List<string>();

            foreach (var detalle in detalles)
            {
                var producto = await _productosRepository.ObtenerPorId(detalle.IdProducto);

                if (producto is null)
                {
                    erroresStock.Add($"Producto {detalle.IdProducto} no existe.");
                    continue;
                }

                var stockActual = await _movimientosStockRepository.ObtenerStockActual(detalle.IdProducto);

                if (stockActual < detalle.Cantidad)
                {
                    erroresStock.Add(
                        $"Stock insuficiente para {producto.Nombre}. " +
                        $"Disponible: {stockActual}, " +
                        $"Solicitado: {detalle.Cantidad}");
                }
            }

            if (erroresStock.Any())
            {
                throw new InvalidOperationException("Errores de stock:\n" + string.Join("\n", erroresStock));
            }

            var totalCalculado = detalles.Sum(d => d.Cantidad * d.PrecioUnitario);
            venta.Total = totalCalculado;
            venta.TotalPagado = 0;
            venta.Estado = EstadoVentaActiva;
            venta.Fecha = DateTime.Now;

            var idVenta = await _ventasRepository.Insertar(venta);

            foreach (var detalle in detalles)
            {
                var producto = await _productosRepository.ObtenerPorId(detalle.IdProducto);

                if (producto is null)
                    throw new ArgumentException($"Producto {detalle.IdProducto} no existe.");

                if (producto.StockActual < detalle.Cantidad)
                    throw new InvalidOperationException($"Stock insuficiente para el producto {producto.Nombre}.");

                detalle.IdVenta = idVenta;
                await _detalleRepository.Insertar(detalle);

                var movimiento = new MovimientoStock
                {
                    IdProducto = detalle.IdProducto,
                    Cantidad = -detalle.Cantidad,
                    IdTipoMovimientoStock = TipoMovimientoStock.Venta,
                    Fecha = DateTime.Now,
                    IdReferencia = idVenta,
                    Observaciones = "Venta realizada"
                };

                await _movimientosStockRepository.RegistrarMovimiento(movimiento);
            }

            if (pagos != null && pagos.Any())
            {
                foreach (var pago in pagos)
                {
                    pago.IdVenta = idVenta;
                    pago.FechaPago = DateTime.Now;
                    pago.Estado = EstadoPagoActivo;

                    await _pagosRepository.Insertar(pago);
                }

                await _pagosRepository.RecalcularTotalPagado(idVenta);
            }

            return idVenta;
        }

        public async Task AnularVenta(int idVenta)
        {
            if (idVenta <= 0)
                throw new ArgumentException("Id inválido.");

            var venta = await _ventasRepository.ObtenerPorId(idVenta);

            if (venta is null)
                throw new ArgumentException("La venta no existe.");

            if (venta.Estado == EstadoVentaAnulada)
                throw new InvalidOperationException("La venta ya está anulada.");

            var detalles = await _detalleRepository.ObtenerPorVenta(idVenta);
            var pagos = await _pagosRepository.ObtenerPorVenta(idVenta);
            var pagosActivos = pagos
                .Where(p => p.Estado == EstadoPagoActivo)
                .ToList();

            if (pagosActivos.Any())
                await RegistrarDevolucionAutomaticaPorAnulacion(venta, detalles, pagosActivos);

            foreach (var detalle in detalles)
            {
                var movimientoReverso = new MovimientoStock
                {
                    IdProducto = detalle.IdProducto,
                    Cantidad = detalle.Cantidad,
                    IdTipoMovimientoStock = TipoMovimientoStock.AnulacionVenta,
                    Fecha = DateTime.Now,
                    IdReferencia = idVenta,
                    Observaciones = "Anulación de venta"
                };

                await _movimientosStockRepository.RegistrarMovimiento(movimientoReverso);
            }

            foreach (var pago in pagos)
                await _pagosRepository.CambiarEstado(pago.Id, EstadoPagoAnulado);

            await _ventasRepository.CambiarEstado(idVenta, EstadoVentaAnulada);
        }

        private async Task RegistrarDevolucionAutomaticaPorAnulacion(Venta venta, IEnumerable<DetalleVenta> detalles, IEnumerable<VentaPago> pagosActivos)
        {
            var idDevolucion = await _devolucionesRepository.Insertar(new DevolucionVenta
            {
                IdVenta = venta.Id,
                NumeroComprobante = venta.NumeroComprobante,
                IdCliente = venta.IdCliente,
                Fecha = DateTime.Now,
                Observaciones = $"Devolucion automatica por anulacion de venta {venta.NumeroComprobante}",
                Total = venta.Total,
                Estado = EstadoVentaActiva
            });

            foreach (var detalle in detalles)
            {
                await _devolucionDetalleRepository.Insertar(new DetalleDevolucionVenta
                {
                    IdDevolucionVenta = idDevolucion,
                    IdVentaDetalle = detalle.Id,
                    IdProducto = detalle.IdProducto,
                    Cantidad = detalle.Cantidad,
                    PrecioUnitario = detalle.PrecioUnitario,
                    Subtotal = detalle.Cantidad * detalle.PrecioUnitario
                });
            }

            var totalPagado = pagosActivos.Sum(p => p.Importe);

            if (totalPagado <= 0)
                return;

            await _creditoClienteRepository.Insertar(new CreditoCliente
            {
                IdCliente = venta.IdCliente,
                IdDevolucionVenta = idDevolucion,
                Importe = totalPagado,
                Saldo = totalPagado,
                Fecha = DateTime.Now
            });
        }

        public async Task<IEnumerable<Venta>> ObtenerPendientesPorCliente(int idCliente)
        {
            if (idCliente <= 0)
                throw new ArgumentException("Cliente inválido.");

            return await _ventasRepository.ObtenerPorCliente(idCliente, true);
        }

        public async Task CobrarCliente(int idCliente, decimal importe, int idFormaPago, string referencia)
        {
            if (idCliente <= 0)
                throw new ArgumentException("Cliente inválido.");

            if (importe <= 0)
                throw new ArgumentException("El importe debe ser mayor a cero.");

            var ventasPendientes = await _ventasRepository.ObtenerPorCliente(idCliente, true);

            if (!ventasPendientes.Any())
                throw new InvalidOperationException("El cliente no tiene ventas pendientes.");

            decimal montoRestante = importe;

            foreach (var venta in ventasPendientes.OrderBy(v => v.Fecha))
            {
                if (montoRestante <= 0)
                    break;

                var saldo = venta.SaldoPendiente;

                var montoAplicar = Math.Min(saldo, montoRestante);

                var pago = new VentaPago
                {
                    IdVenta = venta.Id,
                    IdFormaPago = idFormaPago,
                    Importe = montoAplicar,
                    Referencia = referencia,
                    FechaPago = DateTime.Now,
                    Estado = EstadoPagoActivo
                };

                await _pagosRepository.Insertar(pago);

                await _pagosRepository.RecalcularTotalPagado(venta.Id);

                montoRestante -= montoAplicar;
            }
        }
    }
}
