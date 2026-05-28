using Comercio.Application.Interfaces;
using Comercio.Domain.Entidades;
using Comercio.Domain.Enums;
using Comercio.Domain.Interfaces;

namespace Comercio.Application.Servicios
{
    public class ComprasServicio: IComprasServicio
    {
        private readonly IComprasRepostory _comprasRepository;
        private readonly IDetalleComprasRepository _detalleRepository;
        private readonly IComprasPagosRepository _pagosRepository;
        private readonly IDevolucionComprasRepository _devolucionesRepository;
        private readonly IDevolucionCompraDetalleRepository _devolucionDetalleRepository;
        private readonly IProductosRepository _productosRepository;
        private readonly IMovimientosStockRepository _movimientosStockRepository;
        private readonly ICreditoProveedorRepository _creditoProveedorRepository;

        public ComprasServicio(IComprasRepostory comprasRepository,IDetalleComprasRepository detalleRepository,IComprasPagosRepository pagosRepository,
            IDevolucionComprasRepository devolucionesRepository, IDevolucionCompraDetalleRepository devolucionDetalleRepository,
            IProductosRepository productosRepository,IMovimientosStockRepository movimientosStockRepository, ICreditoProveedorRepository creditoProveedorRepository)
        {
            _comprasRepository = comprasRepository;
            _detalleRepository = detalleRepository;
            _pagosRepository = pagosRepository;
            _devolucionesRepository = devolucionesRepository;
            _devolucionDetalleRepository = devolucionDetalleRepository;
            _productosRepository = productosRepository;
            _movimientosStockRepository = movimientosStockRepository;
            _creditoProveedorRepository = creditoProveedorRepository;
        }

        public async Task<IEnumerable<Compra>> ObtenerEntreFechas(DateTime desde, DateTime hasta)
        {
            if (desde > hasta)
                throw new ArgumentException("La fecha 'desde' no puede ser mayor que 'hasta'.");

            return await _comprasRepository.ObtenerPorFechas(desde, hasta);
        }

        public async Task<Compra?> ObtenerPorId(int idCompra)
        {
            if (idCompra <= 0)
                throw new ArgumentException("Id inválido.");

            var compra = await _comprasRepository.ObtenerPorId(idCompra);

            if (compra is null)
                return null;

            compra.Detalles = (await _detalleRepository.ObtenerPorCompra(idCompra)).ToList();
            compra.Pagos = (await _pagosRepository.ObtenerPorCompra(idCompra)).ToList();

            return compra;
        }

        public async Task<int> CrearCompra(Compra compra, IEnumerable<DetalleCompra> detalles, IEnumerable<CompraPago>? pagos = null)
        {
            if (compra is null)
                throw new ArgumentNullException(nameof(compra));

            if (detalles is null || !detalles.Any())
                throw new ArgumentException("La compra debe tener al menos un detalle.");

            // Calculo el total
            var totalCalculado = detalles.Sum(d => d.Cantidad * d.PrecioUnitario);

            compra.Total = totalCalculado;
            compra.TotalPagado = 0;
            compra.SaldoPendiente = totalCalculado;
            compra.Estado = EstadoComprobante.Activa;
            compra.Fecha = DateTime.Now;

            var idCompra = await _comprasRepository.Insertar(compra);

            // Inserto los detalles y sumo el stock
            foreach (var detalle in detalles)
            {
                var producto = await _productosRepository.ObtenerPorId(detalle.IdProducto);

                if (producto is null)
                    throw new ArgumentException($"Producto {detalle.IdProducto} no existe.");

                detalle.IdCompra = idCompra;
                detalle.Subtotal = detalle.Cantidad * detalle.PrecioUnitario;

                await _detalleRepository.Insertar(detalle);
                var movimiento = new MovimientoStock
                {
                    IdProducto = detalle.IdProducto,
                    Cantidad = detalle.Cantidad,
                    IdTipoMovimientoStock = TipoMovimientoStock.Compra,
                    Fecha = DateTime.Now,
                    IdReferencia = idCompra,
                    Observaciones = "Compra registrada"
                };
                await _movimientosStockRepository.RegistrarMovimiento(movimiento);
            }

            // Inserto los pagos (si existen)
            if (pagos != null && pagos.Any())
            {
                foreach (var pago in pagos)
                {
                    pago.IdCompra = idCompra;
                    pago.FechaPago = DateTime.Now;
                    pago.Estado = EstadoComprobante.Activa;

                    await _pagosRepository.Insertar(pago);
                }
                await _pagosRepository.RecalcularTotalPagado(idCompra);
            }

            return idCompra;
        }

        public async Task AnularCompra(int idCompra)
        {
            if (idCompra <= 0)
                throw new ArgumentException("Id inválido.");

            var compra = await _comprasRepository.ObtenerPorId(idCompra);

            if (compra is null)
                throw new ArgumentException("La compra no existe.");

            if (compra.Estado == EstadoComprobante.Anulada) 
                throw new InvalidOperationException("La compra ya está anulada.");

            var detalles = await _detalleRepository.ObtenerPorCompra(idCompra);
            var pagos = await _pagosRepository.ObtenerPorCompra(idCompra);

            var pagosActivos = pagos
                .Where(p => p.Estado == EstadoComprobante.Activa)
                .ToList();

            if (pagosActivos.Any())
                await RegistrarDevolucionAutomaticaPorAnulacion(compra, detalles, pagosActivos);

            foreach (var detalle in detalles)
            {
                var movimientoReverso = new MovimientoStock
                {
                    IdProducto = detalle.IdProducto,
                    Cantidad = -detalle.Cantidad,
                    IdTipoMovimientoStock = TipoMovimientoStock.AnulacionCompra,
                    Fecha = DateTime.Now,
                    IdReferencia = idCompra,
                    Observaciones = "Anulación de compra"
                };

                await _movimientosStockRepository.RegistrarMovimiento(movimientoReverso);
            }

            await _comprasRepository.CambiarEstado(idCompra, 2);
        }

        private async Task RegistrarDevolucionAutomaticaPorAnulacion(
            Compra compra,
            IEnumerable<DetalleCompra> detalles,
            IEnumerable<CompraPago> pagosActivos)
        {
            var idDevolucion = await _devolucionesRepository.Insertar(new DevolucionCompra
            {
                IdCompra = compra.Id,
                IdProveedor = compra.IdProveedor,
                Fecha = DateTime.Now,
                Motivo = $"Devolucion automatica por anulacion de compra {compra.NumeroComprobante}",
                Total = compra.Total,
                Estado = 1
            });

            foreach (var detalle in detalles)
            {
                await _devolucionDetalleRepository.Insertar(new DevolucionCompraDetalle
                {
                    IdDevolucionCompra = idDevolucion,
                    IdProducto = detalle.IdProducto,
                    Cantidad = detalle.Cantidad,
                    PrecioUnitario = detalle.PrecioUnitario,
                    Subtotal = detalle.Cantidad * detalle.PrecioUnitario
                });
            }

            var totalPagado = pagosActivos.Sum(p => p.Importe);

            if (totalPagado <= 0)
                return;

            await _creditoProveedorRepository.Insertar(new CreditoProveedor
            {
                IdProveedor = compra.IdProveedor,
                IdDevolucionCompra = idDevolucion,
                Importe = totalPagado,
                Saldo = totalPagado,
                Fecha = DateTime.Now
            });
        }

        public async Task PagarCompra(int idCompra, decimal importe, int idFormaPago)
        {
            if (importe <= 0)
                throw new ArgumentException("El importe a guardar debe ser mayor que 0");

            if (idCompra <= 0)
                throw new ArgumentException("Id inválido.");

            var compra = (await _comprasRepository.ObtenerPorId(idCompra));

            if (compra == null)
                throw new Exception("La compra no existe.");

            if (compra.SaldoPendiente <= 0)
                throw new Exception("La compra no tiene saldo pendiente para abonar.");

            if (importe > compra.SaldoPendiente)
                throw new ArgumentException($"El importe a abonar excede el total de la compra (${compra.SaldoPendiente}).");

            decimal saldoCompra = compra.SaldoPendiente;

            if (saldoCompra > 0 && importe > 0)
            {
                var montoPago = Math.Min(saldoCompra, importe);

                var pago = new CompraPago
                {
                    IdCompra = compra.Id,
                    IdFormaPago = idFormaPago,
                    Importe = montoPago,
                    Estado = EstadoComprobante.Activa
                };

                await _pagosRepository.Insertar(pago);
                await _pagosRepository.RecalcularTotalPagado(compra.Id);
            }
        }
    }
}
