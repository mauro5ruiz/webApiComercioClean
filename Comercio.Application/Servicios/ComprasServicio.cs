using Comercio.Application.Interfaces;
using Comercio.Domain.Entidades;
using Comercio.Domain.Enums;
using Comercio.Domain.Interfaces;
using System.Transactions;

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
        private readonly ISucursalesRepository _sucursalesRepository;

        public ComprasServicio(IComprasRepostory comprasRepository,IDetalleComprasRepository detalleRepository,IComprasPagosRepository pagosRepository,
            IDevolucionComprasRepository devolucionesRepository, IDevolucionCompraDetalleRepository devolucionDetalleRepository,
            IProductosRepository productosRepository,IMovimientosStockRepository movimientosStockRepository, ICreditoProveedorRepository creditoProveedorRepository,
            ISucursalesRepository sucursalesRepository)
        {
            _comprasRepository = comprasRepository;
            _detalleRepository = detalleRepository;
            _pagosRepository = pagosRepository;
            _devolucionesRepository = devolucionesRepository;
            _devolucionDetalleRepository = devolucionDetalleRepository;
            _productosRepository = productosRepository;
            _movimientosStockRepository = movimientosStockRepository;
            _creditoProveedorRepository = creditoProveedorRepository;
            _sucursalesRepository = sucursalesRepository;
        }

        public async Task<IEnumerable<Compra>> ObtenerEntreFechas(DateTime desde, DateTime hasta)
        {
            if (desde > hasta)
                throw new ArgumentException("La fecha 'desde' no puede ser mayor que 'hasta'.");

            return await _comprasRepository.ObtenerPorFechas(desde, hasta);
        }

        public async Task<IEnumerable<Compra>> ObtenerPorEstado(int idEstado)
        {
            if (idEstado <= 0)
                throw new ArgumentException("Debe seleccionar un estado válido");

            return await _comprasRepository.ObtenerPorEstado(idEstado);
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

            var detallesList = detalles?.ToList() ?? throw new ArgumentException("La compra debe tener al menos un detalle.");

            if (!detallesList.Any())
                throw new ArgumentException("La compra debe tener al menos un detalle.");

            var pagosList = pagos?.ToList() ?? new List<CompraPago>();
            var totalCalculado = detallesList.Sum(d => d.Cantidad * d.PrecioUnitario);
            var creditoAplicado = compra.CreditoAplicado;
            var totalPagos = pagosList.Sum(p => p.Importe);

            compra.IdSucursal = await ResolverSucursal(compra.IdSucursal);

            if (creditoAplicado < 0)
                throw new ArgumentException("El credito aplicado no puede ser menor a cero.");

            if (creditoAplicado > totalCalculado)
                throw new InvalidOperationException("El credito aplicado no puede superar el total de la compra.");

            if (totalPagos + creditoAplicado > totalCalculado)
                throw new InvalidOperationException("La suma de pagos y credito aplicado no puede superar el total de la compra.");

            compra.Total = totalCalculado;
            compra.TotalPagado = totalPagos;
            compra.SaldoPendiente = totalCalculado - totalPagos - creditoAplicado;
            compra.Estado = EstadoComprobante.Activa;
            compra.Fecha = DateTime.Now;

            var creditosProveedor = new List<CreditoProveedor>();

            if (creditoAplicado > 0)
            {
                creditosProveedor = (await _creditoProveedorRepository.ObtenerPorProveedor(compra.IdProveedor))
                    .Where(c => c.Saldo > 0)
                    .OrderBy(c => c.Fecha)
                    .ToList();

                var creditoDisponible = creditosProveedor.Sum(c => c.Saldo);

                if (creditoAplicado > creditoDisponible)
                    throw new InvalidOperationException("El credito aplicado no puede superar el credito disponible del proveedor.");
            }

            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            var idCompra = await _comprasRepository.Insertar(compra);

            foreach (var detalle in detallesList)
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

            if (creditoAplicado > 0)
            {
                var creditoRestante = creditoAplicado;

                foreach (var credito in creditosProveedor)
                {
                    if (creditoRestante <= 0)
                        break;

                    var montoAplicar = Math.Min(credito.Saldo, creditoRestante);

                    if (montoAplicar <= 0)
                        continue;

                    var creditoConsumido = await _creditoProveedorRepository.ConsumirCredito(credito.Id, montoAplicar);

                    if (!creditoConsumido)
                        throw new InvalidOperationException("No se pudo aplicar el credito solicitado. Verifique el saldo disponible del proveedor.");

                    creditoRestante -= montoAplicar;
                }

                if (creditoRestante > 0)
                    throw new InvalidOperationException("No se pudo aplicar el credito solicitado. Verifique el saldo disponible del proveedor.");
            }

            if (pagosList.Any())
            {
                foreach (var pago in pagosList)
                {
                    pago.IdCompra = idCompra;
                    pago.FechaPago = DateTime.Now;
                    pago.Estado = EstadoComprobante.Activa;

                    await _pagosRepository.Insertar(pago);
                }
            }

            if (creditoAplicado > 0 || pagosList.Any())
                await _pagosRepository.RecalcularTotalPagado(idCompra, creditoAplicado);

            scope.Complete();
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

            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            if (pagosActivos.Any() || compra.CreditoAplicado > 0)
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

            foreach (var pago in pagosActivos)
                await _pagosRepository.CambiarEstado(pago.Id, (int)EstadoComprobante.Anulada);

            await _comprasRepository.CambiarEstado(idCompra, 2);

            scope.Complete();
        }

        private async Task RegistrarDevolucionAutomaticaPorAnulacion(Compra compra, IEnumerable<DetalleCompra> detalles, IEnumerable<CompraPago> pagosActivos)
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
            var totalCancelado = totalPagado + compra.CreditoAplicado;

            if (totalCancelado <= 0)
                return;

            await _creditoProveedorRepository.Insertar(new CreditoProveedor
            {
                IdProveedor = compra.IdProveedor,
                IdDevolucionCompra = idDevolucion,
                Importe = totalCancelado,
                Saldo = totalCancelado,
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

        private async Task<int> ResolverSucursal(int idSucursal)
        {
            if (idSucursal > 0)
            {
                var sucursalSeleccionada = await _sucursalesRepository.ObtenerPorId(idSucursal);

                if (sucursalSeleccionada is null)
                    throw new InvalidOperationException("La sucursal seleccionada no existe.");

                if (!sucursalSeleccionada.Activa)
                    throw new InvalidOperationException("La sucursal seleccionada esta inactiva.");

                return sucursalSeleccionada.Id;
            }

            var sucursalesActivas = (await _sucursalesRepository.ObtenerTodas())
                .Where(s => s.Activa)
                .ToList();

            if (sucursalesActivas.Count == 1)
                return sucursalesActivas[0].Id;

            if (!sucursalesActivas.Any())
                throw new InvalidOperationException("No hay sucursales activas configuradas.");

            throw new InvalidOperationException("Debe seleccionar una sucursal valida.");
        }
    }
}
