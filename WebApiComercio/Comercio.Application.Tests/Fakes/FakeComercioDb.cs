using Comercio.Domain.Entidades;
using Comercio.Domain.Enums;
using Comercio.Domain.Interfaces;

namespace Comercio.Application.Tests.Fakes
{
    /// <summary>
    /// Doble de prueba en memoria que implementa todos los repositorios usados por los
    /// flujos de Compras/Ventas y sus devoluciones, replicando el comportamiento real de
    /// las tablas (incluida la columna calculada Ventas.SaldoPendiente).
    /// Todos los miembros de interfaz se implementan de forma explicita porque varias
    /// interfaces comparten nombre y firma de parametros (solo difieren en el tipo de
    /// retorno), lo que impide la implementacion implicita.
    /// </summary>
    public class FakeComercioDb :
        IComprasRepostory, IDetalleComprasRepository, IComprasPagosRepository,
        IDevolucionComprasRepository, IDevolucionCompraDetalleRepository, IPagoDevolucionCompraRepository,
        ICreditoProveedorRepository, IProductosRepository, IMovimientosStockRepository, ISucursalesRepository,
        IVentasRepository, IDetalleVentasRepostory, IVentasPagosRepository,
        IDevolucionesVentasRepository, IDetalleDevolucionesVentasRepository, IDevolucionPagosRepository,
        ICreditoClienteRepository, INotasCreditoRepository
    {
        private class DetalleCompraRow
        {
            public int Id;
            public int IdCompra;
            public int IdProducto;
            public int CantidadComprada;
            public decimal PrecioUnitario;
            public int CantidadDevuelta;
        }

        private class DetalleVentaRow
        {
            public int Id;
            public int IdVenta;
            public int IdProducto;
            public int CantidadVendida;
            public decimal PrecioUnitario;
            public int CantidadDevuelta;
        }

        private int _nextCompraId = 1;
        private int _nextDetalleCompraId = 1;
        private int _nextCompraPagoId = 1;
        private int _nextDevolucionCompraId = 1;
        private int _nextDevolucionCompraDetalleId = 1;
        private int _nextPagoDevolucionCompraId = 1;
        private int _nextCreditoProveedorId = 1;
        private int _nextProductoId = 1;
        private int _nextMovimientoId = 1;
        private int _nextSucursalId = 1;

        private int _nextVentaId = 1;
        private int _nextDetalleVentaId = 1;
        private int _nextVentaPagoId = 1;
        private int _nextDevolucionVentaId = 1;
        private int _nextDetalleDevolucionVentaId = 1;
        private int _nextDevolucionVentaPagoId = 1;
        private int _nextCreditoClienteId = 1;
        private int _nextNotaCreditoId = 1;

        public Dictionary<int, Compra> Compras { get; } = new();
        private readonly List<DetalleCompraRow> _detallesCompra = new();
        private readonly List<CompraPago> _compraPagos = new();
        public Dictionary<int, DevolucionCompra> DevolucionesCompra { get; } = new();
        private readonly List<DevolucionCompraDetalle> _devolucionCompraDetalles = new();
        private readonly List<PagoDevolucionCompra> _pagosDevolucionCompra = new();
        public List<CreditoProveedor> CreditosProveedor { get; } = new();

        public Dictionary<int, Producto> Productos { get; } = new();
        private readonly List<MovimientoStock> _movimientos = new();
        public Dictionary<int, Sucursal> Sucursales { get; } = new();

        public Dictionary<int, Venta> Ventas { get; } = new();
        private readonly List<DetalleVentaRow> _detallesVenta = new();
        private readonly List<VentaPago> _ventaPagos = new();
        public Dictionary<int, DevolucionVenta> DevolucionesVenta { get; } = new();
        private readonly List<DetalleDevolucionVenta> _detallesDevolucionVenta = new();
        private readonly List<DevolucionVentaPago> _devolucionVentaPagos = new();
        public List<CreditoCliente> CreditosCliente { get; } = new();
        public List<NotaCredito> NotasCredito { get; } = new();

        // ---------- Helpers de setup ----------

        public Producto AgregarProducto(string nombre = "Producto", int stockInicial = 0)
        {
            var producto = new Producto { Id = _nextProductoId++, Nombre = nombre, Activo = true, FechaAlta = DateTime.Now };
            Productos[producto.Id] = producto;

            if (stockInicial != 0)
            {
                _movimientos.Add(new MovimientoStock
                {
                    Id = _nextMovimientoId++,
                    IdProducto = producto.Id,
                    Cantidad = stockInicial,
                    IdTipoMovimientoStock = TipoMovimientoStock.CargaInicial,
                    Fecha = DateTime.Now,
                    Observaciones = "Carga inicial de stock (test)"
                });
            }

            return producto;
        }

        public Sucursal AgregarSucursal(bool activa = true)
        {
            var sucursal = new Sucursal { Id = _nextSucursalId++, Nombre = "Casa Central", Codigo = "CC", Activa = activa };
            Sucursales[sucursal.Id] = sucursal;
            return sucursal;
        }

        public int StockActualDe(int idProducto) => _movimientos.Where(m => m.IdProducto == idProducto).Sum(m => m.Cantidad);

        private static Compra CloneCompra(Compra c) => new()
        {
            Id = c.Id,
            NumeroComprobante = c.NumeroComprobante,
            Fecha = c.Fecha,
            IdProveedor = c.IdProveedor,
            IdSucursal = c.IdSucursal,
            Total = c.Total,
            TotalPagado = c.TotalPagado,
            CreditoAplicado = c.CreditoAplicado,
            SaldoPendiente = c.SaldoPendiente,
            Estado = c.Estado,
            Observaciones = c.Observaciones,
            FechaAnulacion = c.FechaAnulacion
        };

        private static Venta CloneVenta(Venta v) => new()
        {
            Id = v.Id,
            NumeroComprobante = v.NumeroComprobante,
            Fecha = v.Fecha,
            IdCliente = v.IdCliente,
            IdVendedor = v.IdVendedor,
            IdSucursal = v.IdSucursal,
            Total = v.Total,
            TotalPagado = v.TotalPagado,
            CreditoAplicado = v.CreditoAplicado,
            SaldoPendiente = v.Total - v.TotalPagado - v.CreditoAplicado, // columna calculada en la DB real
            Estado = v.Estado,
            Observaciones = v.Observaciones,
            FechaAnulacion = v.FechaAnulacion
        };

        private Producto ClonarProducto(Producto p) => new()
        {
            Id = p.Id,
            Nombre = p.Nombre,
            Descripcion = p.Descripcion,
            Codigo = p.Codigo,
            CodigoBarra = p.CodigoBarra,
            IdCategoria = p.IdCategoria,
            IdMarca = p.IdMarca,
            PrecioCompra = p.PrecioCompra,
            PrecioVenta = p.PrecioVenta,
            StockMinimo = p.StockMinimo,
            ControlStock = p.ControlStock,
            StockActual = StockActualDe(p.Id),
            UrlImagen = p.UrlImagen,
            Activo = p.Activo,
            FechaAlta = p.FechaAlta,
            FechaBaja = p.FechaBaja
        };

        // ================= IComprasRepostory =================

        Task<IEnumerable<Compra>> IComprasRepostory.ObtenerPorFechas(DateTime desde, DateTime hasta) =>
            Task.FromResult(Compras.Values.Where(c => c.Fecha >= desde.Date && c.Fecha < hasta.Date.AddDays(1)).Select(CloneCompra));

        Task<IEnumerable<Compra>> IComprasRepostory.ObtenerPorEstado(int idEstado) =>
            Task.FromResult(Compras.Values.Where(c => (int)c.Estado == idEstado).Select(CloneCompra));

        Task<Compra?> IComprasRepostory.ObtenerPorId(int idCompra) =>
            Task.FromResult(Compras.TryGetValue(idCompra, out var c) ? CloneCompra(c) : null);

        Task<Compra?> IComprasRepostory.ObtenerPorNroComprobante(string nroComprobante) =>
            Task.FromResult(Compras.Values.FirstOrDefault(c => c.NumeroComprobante == nroComprobante) is { } c ? CloneCompra(c) : null);

        Task<bool> IComprasRepostory.Existe(int idVenta) => Task.FromResult(Compras.ContainsKey(idVenta));

        Task<int> IComprasRepostory.Insertar(Compra compra)
        {
            compra.Id = _nextCompraId++;
            Compras[compra.Id] = CloneCompra(compra);
            return Task.FromResult(compra.Id);
        }

        Task IComprasRepostory.CambiarEstado(int idCompra, int estado)
        {
            if (Compras.TryGetValue(idCompra, out var c))
            {
                c.Estado = (EstadoComprobante)estado;
                c.FechaAnulacion = estado == 2 ? DateTime.Now : null;
            }
            return Task.CompletedTask;
        }

        Task IComprasRepostory.ActualizarTotales(int idCompra, decimal total, decimal totalPagado, decimal saldoPendiente)
        {
            if (Compras.TryGetValue(idCompra, out var c))
            {
                c.Total = total;
                c.TotalPagado = totalPagado;
                c.CreditoAplicado = 0;
                c.SaldoPendiente = saldoPendiente;
            }
            return Task.CompletedTask;
        }

        Task IComprasRepostory.AgregarCreditoAplicado(int idCompra, decimal importe)
        {
            if (Compras.TryGetValue(idCompra, out var c))
            {
                c.CreditoAplicado += importe;
                c.SaldoPendiente = c.Total - c.TotalPagado - c.CreditoAplicado;
            }
            return Task.CompletedTask;
        }

        Task IComprasRepostory.RegistrarDevolucion(int idCompra, decimal montoTotal, decimal montoTotalPagado, decimal montoCreditoAplicado)
        {
            if (Compras.TryGetValue(idCompra, out var c))
            {
                c.Total -= montoTotal;
                c.TotalPagado -= montoTotalPagado;
                c.CreditoAplicado -= montoCreditoAplicado;
                c.SaldoPendiente = c.Total - c.TotalPagado - c.CreditoAplicado;
            }
            return Task.CompletedTask;
        }

        Task<IEnumerable<Compra>> IComprasRepostory.ObtenerPendientesPorProveedor(int idProveedor) =>
            Task.FromResult(Compras.Values.Where(c => c.IdProveedor == idProveedor && c.SaldoPendiente > 0 && c.Estado == EstadoComprobante.Activa).Select(CloneCompra));

        // ================= IDetalleComprasRepository =================

        Task<IEnumerable<DetalleCompra>> IDetalleComprasRepository.ObtenerPorCompra(int idCompra) =>
            Task.FromResult(_detallesCompra.Where(d => d.IdCompra == idCompra).Select(d => new DetalleCompra
            {
                Id = d.Id,
                IdCompra = d.IdCompra,
                IdProducto = d.IdProducto,
                Cantidad = d.CantidadComprada - d.CantidadDevuelta,
                PrecioUnitario = d.PrecioUnitario,
                Subtotal = (d.CantidadComprada - d.CantidadDevuelta) * d.PrecioUnitario,
                CantidadDevuelta = d.CantidadDevuelta
            }));

        Task<int?> IDetalleComprasRepository.ObtenerIdDetalleCompra(int idCompra, int idProducto) =>
            Task.FromResult(_detallesCompra.FirstOrDefault(d => d.IdCompra == idCompra && d.IdProducto == idProducto) is { } row ? (int?)row.Id : null);

        Task IDetalleComprasRepository.Insertar(DetalleCompra detalle)
        {
            var row = new DetalleCompraRow
            {
                Id = _nextDetalleCompraId++,
                IdCompra = detalle.IdCompra,
                IdProducto = detalle.IdProducto,
                CantidadComprada = detalle.Cantidad,
                PrecioUnitario = detalle.PrecioUnitario,
                CantidadDevuelta = 0
            };
            _detallesCompra.Add(row);
            detalle.Id = row.Id;
            return Task.CompletedTask;
        }

        Task IDetalleComprasRepository.AgregarCantidadDevuelto(int idDetalleCompra, int idProducto, int cantidad)
        {
            var row = _detallesCompra.FirstOrDefault(d => d.Id == idDetalleCompra && d.IdProducto == idProducto);
            if (row != null)
                row.CantidadDevuelta += cantidad;
            return Task.CompletedTask;
        }

        // ================= IComprasPagosRepository =================

        Task IComprasPagosRepository.Insertar(CompraPago pago)
        {
            pago.Id = _nextCompraPagoId++;
            pago.FechaPago = DateTime.Now;
            _compraPagos.Add(pago);
            return Task.CompletedTask;
        }

        Task<IEnumerable<CompraPago>> IComprasPagosRepository.ObtenerPorCompra(int idCompra) =>
            Task.FromResult(_compraPagos.Where(p => p.IdCompra == idCompra).AsEnumerable());

        Task IComprasPagosRepository.RecalcularTotalPagado(int idCompra) => RecalcularTotalPagadoCompraInterno(idCompra, null);

        Task IComprasPagosRepository.RecalcularTotalPagado(int idCompra, decimal creditoAplicado) => RecalcularTotalPagadoCompraInterno(idCompra, creditoAplicado);

        private Task RecalcularTotalPagadoCompraInterno(int idCompra, decimal? creditoOverride)
        {
            if (Compras.TryGetValue(idCompra, out var c))
            {
                var totalPagado = _compraPagos.Where(p => p.IdCompra == idCompra && p.Estado == EstadoComprobante.Activa).Sum(p => p.Importe);
                c.TotalPagado = totalPagado;
                var credito = creditoOverride ?? c.CreditoAplicado;
                c.SaldoPendiente = c.Total - totalPagado - credito;
            }
            return Task.CompletedTask;
        }

        Task IComprasPagosRepository.CambiarEstado(int idPago, int estado)
        {
            var p = _compraPagos.FirstOrDefault(x => x.Id == idPago);
            if (p != null)
                p.Estado = (EstadoComprobante)estado;
            return Task.CompletedTask;
        }

        // ================= IDevolucionComprasRepository =================

        Task<IEnumerable<DevolucionCompra>> IDevolucionComprasRepository.ObtenerPorFechas(DateTime desde, DateTime hasta) =>
            Task.FromResult(DevolucionesCompra.Values.Where(d => d.Fecha >= desde.Date && d.Fecha < hasta.Date.AddDays(1)));

        Task<DevolucionCompra?> IDevolucionComprasRepository.ObtenerPorId(int idDevolucion) =>
            Task.FromResult(DevolucionesCompra.TryGetValue(idDevolucion, out var d) ? d : null);

        Task<bool> IDevolucionComprasRepository.Existe(int idDevolucion) => Task.FromResult(DevolucionesCompra.ContainsKey(idDevolucion));

        Task<int> IDevolucionComprasRepository.Insertar(DevolucionCompra devolucion)
        {
            devolucion.Id = _nextDevolucionCompraId++;
            DevolucionesCompra[devolucion.Id] = devolucion;
            return Task.FromResult(devolucion.Id);
        }

        Task IDevolucionComprasRepository.CambiarEstado(int idDevolucion, int estado)
        {
            if (DevolucionesCompra.TryGetValue(idDevolucion, out var d))
                d.Estado = estado;
            return Task.CompletedTask;
        }

        Task IDevolucionComprasRepository.ActualizarTotal(int idDevolucion, decimal total)
        {
            if (DevolucionesCompra.TryGetValue(idDevolucion, out var d))
                d.Total = total;
            return Task.CompletedTask;
        }

        // ================= IDevolucionCompraDetalleRepository =================

        Task<IEnumerable<DevolucionCompraDetalle>> IDevolucionCompraDetalleRepository.ObtenerPorCompra(int idCompra) =>
            Task.FromResult(_devolucionCompraDetalles.Where(d =>
                DevolucionesCompra.TryGetValue(d.IdDevolucionCompra, out var dev) && dev.IdCompra == idCompra && dev.Estado == 1));

        Task<IEnumerable<DevolucionCompraDetalle>> IDevolucionCompraDetalleRepository.ObtenerPorDevolucion(int idDevolucion) =>
            Task.FromResult(_devolucionCompraDetalles.Where(d => d.IdDevolucionCompra == idDevolucion));

        Task IDevolucionCompraDetalleRepository.Insertar(DevolucionCompraDetalle detalle)
        {
            detalle.Id = _nextDevolucionCompraDetalleId++;
            _devolucionCompraDetalles.Add(detalle);
            return Task.CompletedTask;
        }

        // ================= IPagoDevolucionCompraRepository =================

        Task<IEnumerable<PagoDevolucionCompra>> IPagoDevolucionCompraRepository.ObtenerPorDevolucion(int idDevolucion) =>
            Task.FromResult(_pagosDevolucionCompra.Where(p => p.IdDevolucionCompra == idDevolucion));

        Task IPagoDevolucionCompraRepository.Insertar(PagoDevolucionCompra pago)
        {
            pago.Id = _nextPagoDevolucionCompraId++;
            pago.Fecha = DateTime.Now;
            _pagosDevolucionCompra.Add(pago);
            return Task.CompletedTask;
        }

        // ================= ICreditoProveedorRepository =================

        Task<IEnumerable<CreditoProveedor>> ICreditoProveedorRepository.ObtenerPorProveedor(int idProveedor) =>
            Task.FromResult(CreditosProveedor.Where(c => c.IdProveedor == idProveedor).AsEnumerable());

        Task ICreditoProveedorRepository.Insertar(CreditoProveedor credito)
        {
            credito.Id = _nextCreditoProveedorId++;
            credito.Fecha = DateTime.Now;
            CreditosProveedor.Add(credito);
            return Task.CompletedTask;
        }

        Task<bool> ICreditoProveedorRepository.ConsumirCredito(int idCredito, decimal importe)
        {
            var c = CreditosProveedor.FirstOrDefault(x => x.Id == idCredito);
            if (c is null || c.Saldo < importe)
                return Task.FromResult(false);
            c.Saldo -= importe;
            return Task.FromResult(true);
        }

        // ================= IProductosRepository =================

        Task<IEnumerable<Producto>> IProductosRepository.ObtenerTodos(bool incluirEliminados) =>
            Task.FromResult(Productos.Values.Select(ClonarProducto));

        Task<Producto?> IProductosRepository.ObtenerPorId(int id) =>
            Task.FromResult(Productos.TryGetValue(id, out var p) ? ClonarProducto(p) : null);

        Task<bool> IProductosRepository.ExistePorCodigo(string? codigo, int? excluirId) => Task.FromResult(false);
        Task<bool> IProductosRepository.ExistePorCodigoBarra(string? codigoBarra, int? excluirId) => Task.FromResult(false);

        Task<int> IProductosRepository.Crear(Producto producto)
        {
            producto.Id = _nextProductoId++;
            Productos[producto.Id] = producto;
            return Task.FromResult(producto.Id);
        }

        Task<bool> IProductosRepository.Actualizar(Producto producto)
        {
            if (!Productos.ContainsKey(producto.Id)) return Task.FromResult(false);
            Productos[producto.Id] = producto;
            return Task.FromResult(true);
        }

        Task<bool> IProductosRepository.DarDeBaja(int id) => Task.FromResult(true);
        Task<bool> IProductosRepository.Restaurar(int id) => Task.FromResult(true);
        Task<bool> IProductosRepository.EliminarPermanentemente(int id) => Task.FromResult(true);
        Task<bool> IProductosRepository.ActualizarPrecioIndividual(int idProducto, decimal valor, string tipoOperacion) => Task.FromResult(true);
        Task<int> IProductosRepository.ActualizarPrecios(decimal valor, string tipoOperacion, int? idCategoria, int? idMarca, bool soloActivos) => Task.FromResult(0);
        Task<IEnumerable<Producto>> IProductosRepository.ObtenerProductosBajoStock() => Task.FromResult(Enumerable.Empty<Producto>());

        // ================= IMovimientosStockRepository =================

        Task<IEnumerable<MovimientoStock>> IMovimientosStockRepository.ObtenerPorProducto(int idProducto) =>
            Task.FromResult(_movimientos.Where(m => m.IdProducto == idProducto));

        Task<IEnumerable<MovimientoStock>> IMovimientosStockRepository.ObtenerPorTipoMovimiento(int idTipoMovimiento) =>
            Task.FromResult(_movimientos.Where(m => (int)m.IdTipoMovimientoStock == idTipoMovimiento));

        Task IMovimientosStockRepository.RegistrarMovimiento(MovimientoStock movimiento)
        {
            movimiento.Id = _nextMovimientoId++;
            _movimientos.Add(movimiento);
            return Task.CompletedTask;
        }

        Task<int> IMovimientosStockRepository.ObtenerStockActual(int idProducto) => Task.FromResult(StockActualDe(idProducto));

        Task IMovimientosStockRepository.Actualizar(MovimientoStock movimiento) => Task.CompletedTask;

        Task IMovimientosStockRepository.Eliminar(int idMovimiento)
        {
            _movimientos.RemoveAll(m => m.Id == idMovimiento);
            return Task.CompletedTask;
        }

        // ================= ISucursalesRepository =================

        Task<IEnumerable<Sucursal>> ISucursalesRepository.ObtenerTodas() => Task.FromResult(Sucursales.Values.AsEnumerable());
        Task<Sucursal?> ISucursalesRepository.ObtenerPorId(int id) => Task.FromResult(Sucursales.TryGetValue(id, out var s) ? s : null);
        Task<Sucursal?> ISucursalesRepository.ObtenerPorNombre(string nombre) => Task.FromResult(Sucursales.Values.FirstOrDefault(s => s.Nombre == nombre));
        Task<bool> ISucursalesRepository.ExistePorCodigo(string codigo) => Task.FromResult(Sucursales.Values.Any(s => s.Codigo == codigo));

        Task<int> ISucursalesRepository.Crear(Sucursal sucursal)
        {
            sucursal.Id = _nextSucursalId++;
            Sucursales[sucursal.Id] = sucursal;
            return Task.FromResult(sucursal.Id);
        }

        Task ISucursalesRepository.Actualizar(Sucursal sucursal)
        {
            Sucursales[sucursal.Id] = sucursal;
            return Task.CompletedTask;
        }

        Task ISucursalesRepository.Eliminar(int id)
        {
            Sucursales.Remove(id);
            return Task.CompletedTask;
        }

        Task<bool> ISucursalesRepository.DarDeBaja(int id)
        {
            if (Sucursales.TryGetValue(id, out var s)) s.Activa = false;
            return Task.FromResult(true);
        }

        Task<bool> ISucursalesRepository.Restaurar(int id)
        {
            if (Sucursales.TryGetValue(id, out var s)) s.Activa = true;
            return Task.FromResult(true);
        }

        // ================= IVentasRepository (Ventas.SaldoPendiente es columna calculada) =================

        Task<IEnumerable<Venta>> IVentasRepository.ObtenerPorFechas(DateTime desde, DateTime hasta) =>
            Task.FromResult(Ventas.Values.Where(v => v.Fecha >= desde.Date && v.Fecha < hasta.Date.AddDays(1)).Select(CloneVenta));

        Task<IEnumerable<Venta>> IVentasRepository.ObtenerPorEstado(string estado) =>
            Task.FromResult(Ventas.Values.Where(v => v.Estado == estado).Select(CloneVenta));

        Task<Venta?> IVentasRepository.ObtenerPorId(int id) =>
            Task.FromResult(Ventas.TryGetValue(id, out var v) ? CloneVenta(v) : null);

        Task<Venta?> IVentasRepository.ObtenerPorNroComprobante(string nroComprobante) =>
            Task.FromResult(Ventas.Values.FirstOrDefault(v => v.NumeroComprobante == nroComprobante) is { } v ? CloneVenta(v) : null);

        Task<IEnumerable<Venta>> IVentasRepository.ObtenerPorCliente(int idCliente, bool soloPendientes)
        {
            var query = Ventas.Values.Where(v => v.IdCliente == idCliente);

            if (soloPendientes)
                query = query.Where(v => v.Estado == "Activa" && (v.Total - v.TotalPagado - v.CreditoAplicado) > 0);

            return Task.FromResult(query.OrderBy(v => v.Fecha).Select(CloneVenta));
        }

        Task<IEnumerable<Venta>> IVentasRepository.ObtenerPendientes() =>
            Task.FromResult(Ventas.Values.Where(v => v.Estado == "Activa" && (v.Total - v.TotalPagado - v.CreditoAplicado) > 0).Select(CloneVenta));

        Task<bool> IVentasRepository.Existe(int idVenta) => Task.FromResult(Ventas.ContainsKey(idVenta));

        Task<IEnumerable<Venta>> IVentasRepository.ObtenerCuentaCorrientePorCliente(int idCliente) =>
            Task.FromResult(Ventas.Values.Where(v => v.IdCliente == idCliente).Select(CloneVenta));

        Task<int> IVentasRepository.Insertar(Venta venta)
        {
            venta.Id = _nextVentaId++;
            venta.Estado = string.IsNullOrEmpty(venta.Estado) ? "Activa" : venta.Estado;
            Ventas[venta.Id] = CloneVenta(venta);
            return Task.FromResult(venta.Id);
        }

        Task IVentasRepository.Actualizar(Venta venta)
        {
            Ventas[venta.Id] = CloneVenta(venta);
            return Task.CompletedTask;
        }

        Task IVentasRepository.AgregarCreditoAplicado(int idVenta, decimal importe)
        {
            if (Ventas.TryGetValue(idVenta, out var v))
            {
                v.CreditoAplicado += importe;
                RecomputarSaldoPendiente(v);
            }
            return Task.CompletedTask;
        }

        Task IVentasRepository.RegistrarDevolucion(int idVenta, decimal montoTotal, decimal montoTotalPagado, decimal montoCreditoAplicado)
        {
            if (Ventas.TryGetValue(idVenta, out var v))
            {
                v.Total -= montoTotal;
                v.TotalPagado -= montoTotalPagado;
                v.CreditoAplicado -= montoCreditoAplicado;
                RecomputarSaldoPendiente(v);
            }
            return Task.CompletedTask;
        }

        // Ventas.SaldoPendiente es una columna calculada en la DB real; el objeto guardado
        // en memoria no se recalcula solo, asi que lo mantenemos sincronizado a mano para que
        // tambien sea correcto si algo lo lee directamente del diccionario (sin pasar por ObtenerPorId).
        private static void RecomputarSaldoPendiente(Venta v) => v.SaldoPendiente = v.Total - v.TotalPagado - v.CreditoAplicado;

        Task IVentasRepository.CambiarEstado(int idVenta, string estado)
        {
            if (Ventas.TryGetValue(idVenta, out var v))
            {
                v.Estado = estado;
                v.FechaAnulacion = estado == "Anulada" ? DateTime.Now : null;
            }
            return Task.CompletedTask;
        }

        // ================= IDetalleVentasRepostory =================

        Task<IEnumerable<DetalleVenta>> IDetalleVentasRepostory.ObtenerPorVenta(int idVenta) =>
            Task.FromResult(_detallesVenta.Where(d => d.IdVenta == idVenta).Select(d => new DetalleVenta
            {
                Id = d.Id,
                IdVenta = d.IdVenta,
                IdProducto = d.IdProducto,
                Cantidad = d.CantidadVendida - d.CantidadDevuelta,
                PrecioUnitario = d.PrecioUnitario,
                Subtotal = (d.CantidadVendida - d.CantidadDevuelta) * d.PrecioUnitario
            }));

        Task<int?> IDetalleVentasRepostory.ObtenerIdDetalleVenta(int idVenta, int idProducto) =>
            Task.FromResult(_detallesVenta.FirstOrDefault(d => d.IdVenta == idVenta && d.IdProducto == idProducto) is { } row ? (int?)row.Id : null);

        Task IDetalleVentasRepostory.Insertar(DetalleVenta detalle)
        {
            var row = new DetalleVentaRow
            {
                Id = _nextDetalleVentaId++,
                IdVenta = detalle.IdVenta,
                IdProducto = detalle.IdProducto,
                CantidadVendida = detalle.Cantidad,
                PrecioUnitario = detalle.PrecioUnitario,
                CantidadDevuelta = 0
            };
            _detallesVenta.Add(row);
            detalle.Id = row.Id;
            return Task.CompletedTask;
        }

        Task IDetalleVentasRepostory.AgregarCantidadDevuelto(int idDetalleVenta, int idProducto, int cantidad)
        {
            var row = _detallesVenta.FirstOrDefault(d => d.Id == idDetalleVenta && d.IdProducto == idProducto);
            if (row != null)
                row.CantidadDevuelta += cantidad;
            return Task.CompletedTask;
        }

        // ================= IVentasPagosRepository =================

        Task IVentasPagosRepository.Insertar(VentaPago pago)
        {
            pago.Id = _nextVentaPagoId++;
            pago.FechaPago = DateTime.Now;
            pago.Estado = string.IsNullOrEmpty(pago.Estado) ? "Activo" : pago.Estado;
            _ventaPagos.Add(pago);
            return Task.CompletedTask;
        }

        Task<IEnumerable<VentaPago>> IVentasPagosRepository.ObtenerPorVenta(int idVenta) =>
            Task.FromResult(_ventaPagos.Where(p => p.IdVenta == idVenta).AsEnumerable());

        Task IVentasPagosRepository.RecalcularTotalPagado(int idVenta) => RecalcularTotalPagadoVentaInterno(idVenta);
        Task IVentasPagosRepository.RecalcularTotalPagado(int idVenta, decimal creditoAplicado) => RecalcularTotalPagadoVentaInterno(idVenta);

        private Task RecalcularTotalPagadoVentaInterno(int idVenta)
        {
            if (Ventas.TryGetValue(idVenta, out var v))
            {
                v.TotalPagado = _ventaPagos.Where(p => p.IdVenta == idVenta && p.Estado == "Activo").Sum(p => p.Importe);
                RecomputarSaldoPendiente(v);
            }
            return Task.CompletedTask;
        }

        Task IVentasPagosRepository.CambiarEstado(int idPago, string estado)
        {
            var p = _ventaPagos.FirstOrDefault(x => x.Id == idPago);
            if (p != null)
                p.Estado = estado;
            return Task.CompletedTask;
        }

        // ================= IDevolucionesVentasRepository =================

        Task<IEnumerable<DevolucionVenta>> IDevolucionesVentasRepository.ObtenerPorFechas(DateTime desde, DateTime hasta) =>
            Task.FromResult(DevolucionesVenta.Values.Where(d => d.Fecha >= desde.Date && d.Fecha < hasta.Date.AddDays(1)));

        Task<DevolucionVenta?> IDevolucionesVentasRepository.ObtenerPorId(int id) =>
            Task.FromResult(DevolucionesVenta.TryGetValue(id, out var d) ? d : null);

        Task<IEnumerable<DevolucionVenta>> IDevolucionesVentasRepository.ObtenerPorVenta(int idVenta) =>
            Task.FromResult(DevolucionesVenta.Values.Where(d => d.IdVenta == idVenta));

        Task<int> IDevolucionesVentasRepository.Insertar(DevolucionVenta devolucion)
        {
            devolucion.Id = _nextDevolucionVentaId++;
            DevolucionesVenta[devolucion.Id] = devolucion;
            return Task.FromResult(devolucion.Id);
        }

        Task IDevolucionesVentasRepository.CambiarEstado(int idDevolucion, string estado)
        {
            if (DevolucionesVenta.TryGetValue(idDevolucion, out var d))
                d.Estado = estado;
            return Task.CompletedTask;
        }

        // ================= IDetalleDevolucionesVentasRepository =================

        Task<IEnumerable<DetalleDevolucionVenta>> IDetalleDevolucionesVentasRepository.ObtenerPorDevolucion(int idDevolucion) =>
            Task.FromResult(_detallesDevolucionVenta.Where(d => d.IdDevolucionVenta == idDevolucion));

        Task IDetalleDevolucionesVentasRepository.Insertar(DetalleDevolucionVenta detalle)
        {
            detalle.Id = _nextDetalleDevolucionVentaId++;
            _detallesDevolucionVenta.Add(detalle);
            return Task.CompletedTask;
        }

        // ================= IDevolucionPagosRepository (pagos de devoluciones de venta) =================

        Task<IEnumerable<DevolucionVentaPago>> IDevolucionPagosRepository.ObtenerPorDevolucion(int idDevolucion) =>
            Task.FromResult(_devolucionVentaPagos.Where(p => p.IdDevolucionVenta == idDevolucion));

        Task IDevolucionPagosRepository.Insertar(DevolucionVentaPago pago)
        {
            pago.Id = _nextDevolucionVentaPagoId++;
            pago.FechaPago = DateTime.Now;
            pago.Estado = string.IsNullOrEmpty(pago.Estado) ? "Activo" : pago.Estado;
            _devolucionVentaPagos.Add(pago);
            return Task.CompletedTask;
        }

        Task IDevolucionPagosRepository.CambiarEstado(int idPago, string estado)
        {
            var p = _devolucionVentaPagos.FirstOrDefault(x => x.Id == idPago);
            if (p != null)
                p.Estado = estado;
            return Task.CompletedTask;
        }

        // ================= ICreditoClienteRepository =================

        Task<IEnumerable<CreditoCliente>> ICreditoClienteRepository.ObtenerPorCliente(int idCliente) =>
            Task.FromResult(CreditosCliente.Where(c => c.IdCliente == idCliente).AsEnumerable());

        Task ICreditoClienteRepository.Insertar(CreditoCliente credito)
        {
            credito.Id = _nextCreditoClienteId++;
            credito.Fecha = DateTime.Now;
            CreditosCliente.Add(credito);
            return Task.CompletedTask;
        }

        Task<bool> ICreditoClienteRepository.ConsumirCredito(int idCredito, decimal importe)
        {
            var c = CreditosCliente.FirstOrDefault(x => x.Id == idCredito);
            if (c is null || c.Saldo < importe)
                return Task.FromResult(false);
            c.Saldo -= importe;
            return Task.FromResult(true);
        }

        // ================= INotasCreditoRepository =================

        Task<NotaCredito?> INotasCreditoRepository.ObtenerPorDevolucion(int idDevolucionVenta) =>
            Task.FromResult(NotasCredito.FirstOrDefault(n => n.IdDevolucionVenta == idDevolucionVenta));

        Task<string> INotasCreditoRepository.ObtenerSiguienteCodigo() => Task.FromResult($"NC-{_nextNotaCreditoId:0000}");

        Task<int> INotasCreditoRepository.Insertar(NotaCredito notaCredito)
        {
            notaCredito.Id = _nextNotaCreditoId++;
            NotasCredito.Add(notaCredito);
            return Task.FromResult(notaCredito.Id);
        }
    }
}
