using Comercio.Application.Interfaces;
using Comercio.Domain.Entidades;
using Comercio.Domain.Enums;
using Comercio.Domain.Interfaces;
using System.Transactions;

namespace Comercio.Application.Servicios
{
    public class DevolucionesVentasServicio : IDevolucionesVentasServicio
    {
        private const string EstadoActiva = "Activa";
        private const string EstadoPagoActivo = "Activo";
        private const string ObservacionNotaCreditoConsumidorFinal = "Generada por devolucion de venta a Consumidor Final";

        private readonly IDevolucionesVentasRepository _devolucionesRepository;
        private readonly IDetalleDevolucionesVentasRepository _detalleRepository;
        private readonly IDevolucionPagosRepository _pagosRepository;
        private readonly IVentasRepository _ventasRepository;
        private readonly IDetalleVentasRepostory _detalleVentasRepository;
        private readonly IProductosRepository _productosRepository;
        private readonly IMovimientosStockRepository _movimientosStockRepository;
        private readonly ICreditoClienteRepository _creditoClienteRepository;
        private readonly INotasCreditoRepository _notasCreditoRepository;

        public DevolucionesVentasServicio(
            IDevolucionesVentasRepository devolucionesRepository,
            IDetalleDevolucionesVentasRepository detalleRepository,
            IDevolucionPagosRepository pagosRepository,
            IVentasRepository ventasRepository,
            IDetalleVentasRepostory detalleVentasRepository,
            IProductosRepository productosRepository,
            IMovimientosStockRepository movimientosStockRepository,
            ICreditoClienteRepository creditoClienteRepository,
            INotasCreditoRepository notasCreditoRepository)
        {
            _devolucionesRepository = devolucionesRepository;
            _detalleRepository = detalleRepository;
            _pagosRepository = pagosRepository;
            _ventasRepository = ventasRepository;
            _detalleVentasRepository = detalleVentasRepository;
            _productosRepository = productosRepository;
            _movimientosStockRepository = movimientosStockRepository;
            _creditoClienteRepository = creditoClienteRepository;
            _notasCreditoRepository = notasCreditoRepository;
        }

        public async Task<int> RegistrarDevolucion(DevolucionVenta devolucion, IEnumerable<DetalleDevolucionVenta> detalles, IEnumerable<DevolucionVentaPago>? pagos = null)
        {
            if (devolucion is null)
                throw new ArgumentNullException(nameof(devolucion));

            var detallesList = detalles?.ToList() ?? throw new ArgumentException("La devolucion debe tener al menos un producto.");

            if (!detallesList.Any())
                throw new ArgumentException("La devolucion debe tener al menos un producto.");

            var venta = await _ventasRepository.ObtenerPorId(devolucion.IdVenta);

            if (venta is null)
                throw new InvalidOperationException("La venta no existe.");

            if (venta.Estado == EstadoComprobante.Anulada.ToString())
                throw new InvalidOperationException("No se puede devolver una venta anulada.");

            devolucion.IdCliente = venta.IdCliente;

            var detallesVenta = (await _detalleVentasRepository.ObtenerPorVenta(venta.Id)).ToList();

            decimal totalDevuelto = 0;
            var cantidadDisponiblePorProducto = detallesVenta.ToDictionary(d => d.IdProducto, d => d.Cantidad);

            foreach (var detalle in detallesList)
            {
                var detalleVenta = detallesVenta.FirstOrDefault(x => x.IdProducto == detalle.IdProducto);

                if (detalleVenta is null)
                    throw new InvalidOperationException($"El detalle de venta {detalle.IdVentaDetalle} no existe.");

                if (detalle.Cantidad <= 0)
                    throw new InvalidOperationException("La cantidad a devolver debe ser mayor a cero.");

                var disponible = cantidadDisponiblePorProducto[detalle.IdProducto];

                if (detalle.Cantidad > disponible)
                    throw new InvalidOperationException("No se puede devolver mas cantidad de la vendida.");

                cantidadDisponiblePorProducto[detalle.IdProducto] = disponible - detalle.Cantidad;

                detalle.PrecioUnitario = detalleVenta.PrecioUnitario;
                detalle.Subtotal = detalle.Cantidad * detalle.PrecioUnitario;
                totalDevuelto += detalle.Subtotal;
            }

            devolucion.Total = totalDevuelto;
            devolucion.Fecha = DateTime.Now;
            devolucion.Estado = EstadoActiva;

            var pagosDevolucion = pagos?.ToList() ?? new List<DevolucionVentaPago>();
            var totalCobradoVenta = venta.TotalPagado;
            var totalCanceladoVenta = venta.TotalPagado + venta.CreditoAplicado;
            var totalPagadoEnDevolucion = 0m;

            if (totalCobradoVenta <= 0)
            {
                if (pagosDevolucion.Any())
                    throw new InvalidOperationException("No se pueden registrar pagos de devolucion si la venta no tiene pagos cobrados.");
            }
            else
            {
                foreach (var pago in pagosDevolucion)
                {
                    if (pago.IdFormaPago <= 0)
                        throw new InvalidOperationException("La forma de pago de la devolucion es obligatoria.");

                    if (pago.Importe <= 0)
                        throw new InvalidOperationException("Los pagos de la devolucion deben ser mayores a cero.");

                    totalPagadoEnDevolucion += pago.Importe;
                }
            }

            var maximoRefundableEnPagos = Math.Min(totalDevuelto, totalCobradoVenta);

            if (totalPagadoEnDevolucion > maximoRefundableEnPagos)
                throw new InvalidOperationException("Los pagos de la devolucion no pueden superar lo efectivamente cobrado ni el total devuelto.");

            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            var idDevolucion = await _devolucionesRepository.Insertar(devolucion);

            foreach (var detalle in detallesList)
            {
                detalle.IdDevolucionVenta = idDevolucion;
                await _detalleRepository.Insertar(detalle);

                var producto = await _productosRepository.ObtenerPorId(detalle.IdProducto);

                if (producto is null)
                    throw new InvalidOperationException($"Producto {detalle.IdProducto} no existe.");

                await _movimientosStockRepository.RegistrarMovimiento(new MovimientoStock
                {
                    IdProducto = detalle.IdProducto,
                    Cantidad = detalle.Cantidad,
                    IdTipoMovimientoStock = TipoMovimientoStock.DevolucionVenta,
                    Fecha = DateTime.Now,
                    IdReferencia = idDevolucion,
                    Observaciones = "Devolucion de venta"
                });

                var idDetalleVenta = await _detalleVentasRepository.ObtenerIdDetalleVenta(venta.Id, detalle.IdProducto);
                if (idDetalleVenta.HasValue)
                    await _detalleVentasRepository.AgregarCantidadDevuelto(idDetalleVenta.Value, detalle.IdProducto, detalle.Cantidad);
            }

            foreach (var pago in pagosDevolucion)
            {
                pago.IdDevolucionVenta = idDevolucion;
                pago.FechaPago = DateTime.Now;
                pago.Estado = EstadoPagoActivo;

                await _pagosRepository.Insertar(pago);
            }

            var saldoCreditoONota = Math.Min(totalDevuelto, totalCanceladoVenta) - totalPagadoEnDevolucion;

            if (saldoCreditoONota > 0)
                await GenerarCreditoONotaCredito(devolucion.IdCliente, idDevolucion, saldoCreditoONota);

            var reducirTotalPagado = maximoRefundableEnPagos;
            var reducirCreditoAplicado = Math.Min(totalDevuelto, totalCanceladoVenta) - reducirTotalPagado;

            await _ventasRepository.RegistrarDevolucion(venta.Id, totalDevuelto, reducirTotalPagado, reducirCreditoAplicado);

            scope.Complete();
            return idDevolucion;
        }

        public async Task<DevolucionVenta?> ObtenerPorId(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Id invalido.");

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

        private async Task GenerarCreditoONotaCredito(int idCliente, int idDevolucionVenta, decimal importe)
        {
            if (importe <= 0)
                return;

            if (EsConsumidorFinal(idCliente))
            {
                var notaExistente = await _notasCreditoRepository.ObtenerPorDevolucion(idDevolucionVenta);

                if (notaExistente is not null)
                    return;

                await InsertarNotaCredito(idDevolucionVenta, importe);
                return;
            }

            await _creditoClienteRepository.Insertar(new CreditoCliente
            {
                IdCliente = idCliente,
                IdDevolucionVenta = idDevolucionVenta,
                Importe = importe,
                Saldo = importe,
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
                Estado = EstadoActiva,
                Observaciones = ObservacionNotaCreditoConsumidorFinal
            });
        }

        private static bool EsConsumidorFinal(int idCliente) => idCliente <= 0;
    }
}
