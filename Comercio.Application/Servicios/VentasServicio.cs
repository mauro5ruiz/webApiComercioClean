using Comercio.Application.Interfaces;
using Comercio.Domain.Entidades;
using Comercio.Domain.Enums;
using Comercio.Domain.Interfaces;
using System.Transactions;

namespace Comercio.Application.Servicios
{
    public class VentasServicio : IVentasServicio
    {
        private const string EstadoVentaActiva = "Activa";
        private const string EstadoVentaAnulada = "Anulada";
        private const string EstadoPagoActivo = "Activo";
        private const string EstadoPagoAnulado = "Anulado";
        private const string ObservacionNotaCreditoConsumidorFinal = "Generada por devolucion de venta a Consumidor Final";

        private readonly IVentasRepository _ventasRepository;
        private readonly IDetalleVentasRepostory _detalleRepository;
        private readonly IVentasPagosRepository _pagosRepository;
        private readonly IDevolucionesVentasRepository _devolucionesRepository;
        private readonly IDetalleDevolucionesVentasRepository _devolucionDetalleRepository;
        private readonly IDevolucionPagosRepository _devolucionPagosRepository;
        private readonly IProductosRepository _productosRepository;
        private readonly IMovimientosStockRepository _movimientosStockRepository;
        private readonly ICreditoClienteRepository _creditoClienteRepository;
        private readonly INotasCreditoRepository _notasCreditoRepository;

        public VentasServicio(
            IVentasRepository ventasRepository,
            IDetalleVentasRepostory detalleRepository,
            IVentasPagosRepository pagosRepository,
            IDevolucionesVentasRepository devolucionesRepository,
            IDetalleDevolucionesVentasRepository devolucionDetalleRepository,
            IDevolucionPagosRepository devolucionPagosRepository,
            IProductosRepository productosRepository,
            IMovimientosStockRepository movimientosStockRepository,
            ICreditoClienteRepository creditoClienteRepository,
            INotasCreditoRepository notasCreditoRepository)
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
            _notasCreditoRepository = notasCreditoRepository;
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
                throw new ArgumentException("Id invalido.");

            var venta = await _ventasRepository.ObtenerPorId(idVenta);

            if (venta is null)
                return null;

            venta.Detalles = (await _detalleRepository.ObtenerPorVenta(idVenta)).ToList();
            venta.Pagos = (await _pagosRepository.ObtenerPorVenta(idVenta)).ToList();

            return venta;
        }

        public async Task<IEnumerable<Venta>> ObtenerPorEstado(int idEstado)
        {
            if (idEstado <= 0)
                throw new ArgumentException("Debe seleccionar un estado valido");

            var estado = idEstado == 1 ? EstadoComprobante.Activa.ToString() : EstadoComprobante.Anulada.ToString();
            return await _ventasRepository.ObtenerPorEstado(estado);
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
                throw new InvalidOperationException("Errores de stock:\n" + string.Join("\n", erroresStock));

            var totalCalculado = detalles.Sum(d => d.Cantidad * d.PrecioUnitario);
            venta.Total = totalCalculado;
            venta.TotalPagado = 0;
            venta.Estado = EstadoVentaActiva;
            venta.Fecha = DateTime.Now;

            if (venta.IdCliente <= 0)
            {
                var pagado = pagos?.Sum(p => p.Importe);
                if (!pagado.HasValue || pagado.Value < totalCalculado)
                    throw new ArgumentException("Para Consumidor final la venta debe quedar pagada en su totalidad.");
            }

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

                await _movimientosStockRepository.RegistrarMovimiento(new MovimientoStock
                {
                    IdProducto = detalle.IdProducto,
                    Cantidad = -detalle.Cantidad,
                    IdTipoMovimientoStock = TipoMovimientoStock.Venta,
                    Fecha = DateTime.Now,
                    IdReferencia = idVenta,
                    Observaciones = "Venta realizada"
                });
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

        public async Task AnularVenta(int idVenta, IEnumerable<DevolucionVentaPago>? pagos = null)
        {
            if (idVenta <= 0)
                throw new ArgumentException("Id invalido.");

            var venta = await _ventasRepository.ObtenerPorId(idVenta);

            if (venta is null)
                throw new ArgumentException("La venta no existe.");

            if (venta.Estado == EstadoVentaAnulada)
                throw new InvalidOperationException("La venta ya esta anulada.");

            var detalles = (await _detalleRepository.ObtenerPorVenta(idVenta)).ToList();
            var pagosVenta = (await _pagosRepository.ObtenerPorVenta(idVenta)).ToList();
            var pagosActivosVenta = pagosVenta.Where(p => p.Estado == EstadoPagoActivo).ToList();
            var pagosDevolucion = pagos?.ToList() ?? new List<DevolucionVentaPago>();

            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            await RegistrarDevolucionAutomaticaPorAnulacion(venta, detalles, pagosActivosVenta, pagosDevolucion);

            foreach (var detalle in detalles)
            {
                await _movimientosStockRepository.RegistrarMovimiento(new MovimientoStock
                {
                    IdProducto = detalle.IdProducto,
                    Cantidad = detalle.Cantidad,
                    IdTipoMovimientoStock = TipoMovimientoStock.AnulacionVenta,
                    Fecha = DateTime.Now,
                    IdReferencia = idVenta,
                    Observaciones = "Anulacion de venta"
                });
            }

            foreach (var pagoVenta in pagosVenta)
                await _pagosRepository.CambiarEstado(pagoVenta.Id, EstadoPagoAnulado);

            await _ventasRepository.CambiarEstado(idVenta, EstadoVentaAnulada);

            scope.Complete();
        }

        private async Task RegistrarDevolucionAutomaticaPorAnulacion(
            Venta venta,
            IEnumerable<DetalleVenta> detalles,
            IEnumerable<VentaPago> pagosActivos,
            IEnumerable<DevolucionVentaPago> pagosDevolucion)
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
            {
                if (pagosDevolucion.Any())
                    throw new InvalidOperationException("No se pueden registrar pagos de devolucion si la venta no tiene pagos activos.");

                return;
            }

            var totalDevueltoEnPagos = 0m;

            foreach (var pago in pagosDevolucion)
            {
                if (pago.IdFormaPago <= 0)
                    throw new InvalidOperationException("La forma de pago de la devolucion es obligatoria.");

                if (pago.Importe <= 0)
                    throw new InvalidOperationException("Los pagos de la devolucion deben ser mayores a cero.");

                pago.IdDevolucionVenta = idDevolucion;
                pago.FechaPago = DateTime.Now;
                pago.Estado = EstadoPagoActivo;

                await _devolucionPagosRepository.Insertar(pago);
                totalDevueltoEnPagos += pago.Importe;
            }

            if (totalDevueltoEnPagos > totalPagado)
                throw new InvalidOperationException("Los pagos de la devolucion no pueden superar el total pagado de la venta.");

            var saldoCreditoONota = totalPagado - totalDevueltoEnPagos;

            if (saldoCreditoONota <= 0)
                return;

            if (EsConsumidorFinal(venta.IdCliente))
            {
                var notaExistente = await _notasCreditoRepository.ObtenerPorDevolucion(idDevolucion);

                if (notaExistente is null)
                    await InsertarNotaCredito(idDevolucion, saldoCreditoONota);

                return;
            }

            await _creditoClienteRepository.Insertar(new CreditoCliente
            {
                IdCliente = venta.IdCliente,
                IdDevolucionVenta = idDevolucion,
                Importe = saldoCreditoONota,
                Saldo = saldoCreditoONota,
                Fecha = DateTime.Now
            });
        }

        private async Task InsertarNotaCredito(int idDevolucionVenta, decimal importe)
        {
            var codigo = await _notasCreditoRepository.ObtenerSiguienteCodigo();

            await _notasCreditoRepository.Insertar(new NotaCredito
            {
                IdDevolucionVenta = idDevolucionVenta,
                Codigo = codigo,
                Importe = importe,
                Saldo = importe,
                Fecha = DateTime.Now,
                Estado = EstadoVentaActiva,
                Observaciones = ObservacionNotaCreditoConsumidorFinal
            });
        }

        public async Task<IEnumerable<Venta>> ObtenerPendientesPorCliente(int idCliente)
        {
            if (idCliente <= 0)
                throw new ArgumentException("Cliente invalido.");

            return await _ventasRepository.ObtenerPorCliente(idCliente, true);
        }

        public async Task CobrarCliente(int idCliente, decimal importe, int idFormaPago, string referencia)
        {
            if (idCliente <= 0)
                throw new ArgumentException("Cliente invalido.");

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

                await _pagosRepository.Insertar(new VentaPago
                {
                    IdVenta = venta.Id,
                    IdFormaPago = idFormaPago,
                    Importe = montoAplicar,
                    Referencia = referencia,
                    FechaPago = DateTime.Now,
                    Estado = EstadoPagoActivo
                });

                await _pagosRepository.RecalcularTotalPagado(venta.Id);
                montoRestante -= montoAplicar;
            }
        }

        private static bool EsConsumidorFinal(int idCliente) => idCliente <= 0;
    }
}
