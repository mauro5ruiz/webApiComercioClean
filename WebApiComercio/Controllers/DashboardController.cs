using Comercio.Application.Dtos.Dashboard;
using Comercio.Domain.Enums;
using Comercio.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Comercio.Api.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardController : Controller
    {
        private static readonly DateTime FechaHistoricaInicio = new(2000, 1, 1);
        private const int DiasOfertaPorVencer = 7;

        private readonly IVentasRepository _ventasRepository;
        private readonly IDevolucionesVentasRepository _devolucionesVentasRepository;
        private readonly IComprasRepostory _comprasRepository;
        private readonly IDevolucionComprasRepository _devolucionComprasRepository;
        private readonly IPerdidasRepository _perdidasRepository;
        private readonly IDetallePerdidasRepository _detallePerdidasRepository;
        private readonly IProductosRepository _productosRepository;
        private readonly ICategoriasRepository _categoriasRepository;
        private readonly IMarcasRepository _marcasRepository;
        private readonly IOfertasRepository _ofertasRepository;
        private readonly IClientesRepository _clientesRepository;
        private readonly IProveedoresRepository _proveedoresRepository;
        private readonly IVendedoresRepository _vendedoresRepository;

        public DashboardController(IVentasRepository ventasRepository, IDevolucionesVentasRepository devolucionesVentasRepository,
            IComprasRepostory comprasRepository, IDevolucionComprasRepository devolucionComprasRepository, IPerdidasRepository perdidasRepository,
            IDetallePerdidasRepository detallePerdidasRepository,IProductosRepository productosRepository,ICategoriasRepository categoriasRepository,
            IMarcasRepository marcasRepository,IOfertasRepository ofertasRepository,IClientesRepository clientesRepository,IProveedoresRepository proveedoresRepository,IVendedoresRepository vendedoresRepository)
        {
            _ventasRepository = ventasRepository;
            _devolucionesVentasRepository = devolucionesVentasRepository;
            _comprasRepository = comprasRepository;
            _devolucionComprasRepository = devolucionComprasRepository;
            _perdidasRepository = perdidasRepository;
            _detallePerdidasRepository = detallePerdidasRepository;
            _productosRepository = productosRepository;
            _categoriasRepository = categoriasRepository;
            _marcasRepository = marcasRepository;
            _ofertasRepository = ofertasRepository;
            _clientesRepository = clientesRepository;
            _proveedoresRepository = proveedoresRepository;
            _vendedoresRepository = vendedoresRepository;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerDashboard()
        {
            try
            {
                var ahora = DateTime.Now;
                var inicioMesActual = new DateTime(ahora.Year, ahora.Month, 1);
                var finMesActual = inicioMesActual.AddMonths(1).AddDays(-1);
                var inicioMesAnterior = inicioMesActual.AddMonths(-1);
                var finMesAnterior = inicioMesActual.AddDays(-1);

                var ventasMesActual = (await _ventasRepository.ObtenerPorFechas(inicioMesActual, finMesActual)).ToList();
                var ventasMesAnterior = (await _ventasRepository.ObtenerPorFechas(inicioMesAnterior, finMesAnterior)).ToList();
                var ventasHistoricas = (await _ventasRepository.ObtenerPorFechas(FechaHistoricaInicio, ahora)).ToList();
                var devolucionesVentasMesActual = (await _devolucionesVentasRepository.ObtenerPorFechas(inicioMesActual, finMesActual)).ToList();
                var devolucionesVentasMesAnterior = (await _devolucionesVentasRepository.ObtenerPorFechas(inicioMesAnterior, finMesAnterior)).ToList();
                var devolucionesVentasHistoricas = (await _devolucionesVentasRepository.ObtenerPorFechas(FechaHistoricaInicio, ahora)).ToList();

                var comprasMesActual = (await _comprasRepository.ObtenerPorFechas(inicioMesActual, finMesActual)).ToList();
                var comprasMesAnterior = (await _comprasRepository.ObtenerPorFechas(inicioMesAnterior, finMesAnterior)).ToList();
                var comprasHistoricas = (await _comprasRepository.ObtenerPorFechas(FechaHistoricaInicio, ahora)).ToList();
                var devolucionesComprasMesActual = (await _devolucionComprasRepository.ObtenerPorFechas(inicioMesActual, finMesActual)).ToList();
                var devolucionesComprasMesAnterior = (await _devolucionComprasRepository.ObtenerPorFechas(inicioMesAnterior, finMesAnterior)).ToList();
                var devolucionesComprasHistoricas = (await _devolucionComprasRepository.ObtenerPorFechas(FechaHistoricaInicio, ahora)).ToList();

                var perdidasMesActual = (await _perdidasRepository.ObtenerPorFechas(inicioMesActual, finMesActual)).ToList();
                var perdidasMesAnterior = (await _perdidasRepository.ObtenerPorFechas(inicioMesAnterior, finMesAnterior)).ToList();
                var perdidasHistoricas = (await _perdidasRepository.ObtenerPorFechas(FechaHistoricaInicio, ahora)).ToList();

                var productos = (await _productosRepository.ObtenerTodos(true)).ToList();
                var productosBajoStock = (await _productosRepository.ObtenerProductosBajoStock()).ToList();
                var categorias = (await _categoriasRepository.ObtenerTodas()).ToList();
                var marcas = (await _marcasRepository.ObtenerTodas()).ToList();
                var ofertas = (await _ofertasRepository.ObtenerTodas(true)).ToList();
                var clientes = (await _clientesRepository.ObtenerTodos(true)).ToList();
                var proveedores = (await _proveedoresRepository.ObtenerTodos(true)).ToList();
                var vendedores = (await _vendedoresRepository.ObtenerTodos(true)).ToList();

                var ventasUltimas = ventasHistoricas
                    .Where(v => !EsVentaAnulada(v.Estado))
                    .OrderByDescending(v => v.Fecha)
                    .Take(6)
                    .ToList();

                var clientesPorId = clientes.ToDictionary(c => c.Id);
                var vendedoresPorId = vendedores.ToDictionary(v => v.Id);

                var respuesta = new DashboardDto
                {
                    Cards = new DashboardCardsDto
                    {
                        VentasMesActual = ventasMesActual.Where(v => !EsVentaAnulada(v.Estado)).Sum(v => v.Total),
                        VentasMesAnterior = ventasMesAnterior.Where(v => !EsVentaAnulada(v.Estado)).Sum(v => v.Total),
                        DevolucionesVentasMesActual = devolucionesVentasMesActual.Where(EsDevolucionVentaActiva).Sum(d => d.Total),
                        DevolucionesVentasMesAnterior = devolucionesVentasMesAnterior.Where(EsDevolucionVentaActiva).Sum(d => d.Total),
                        ComprasMesActual = comprasMesActual.Where(c => c.Estado != EstadoComprobante.Anulada).Sum(c => c.Total),
                        ComprasMesAnterior = comprasMesAnterior.Where(c => c.Estado != EstadoComprobante.Anulada).Sum(c => c.Total),
                        DevolucionesComprasMesActual = devolucionesComprasMesActual.Where(EsDevolucionCompraActiva).Sum(d => d.Total),
                        DevolucionesComprasMesAnterior = devolucionesComprasMesAnterior.Where(EsDevolucionCompraActiva).Sum(d => d.Total),
                        PerdidasMesActual = await CalcularMontoPerdidas(perdidasMesActual),
                        PerdidasMesAnterior = await CalcularMontoPerdidas(perdidasMesAnterior)
                    },
                    Modulos = new DashboardModulosDto
                    {
                        Productos = productos.Count,
                        Categorias = categorias.Count,
                        Marcas = marcas.Count,
                        OfertasActivas = ofertas.Count(EsOfertaActivaVigente),
                        Perdidas = perdidasHistoricas.Count(p => p.IdEstado != (int)EstadoPerdida.Anulada),
                        Ventas = ventasHistoricas.Count(v => !EsVentaAnulada(v.Estado)),
                        DevolucionesVentas = devolucionesVentasHistoricas.Count(EsDevolucionVentaActiva),
                        Compras = comprasHistoricas.Count(c => c.Estado != EstadoComprobante.Anulada),
                        DevolucionesCompras = devolucionesComprasHistoricas.Count(EsDevolucionCompraActiva),
                        Clientes = clientes.Count,
                        Proveedores = proveedores.Count,
                        Vendedores = vendedores.Count
                    },
                    Alertas = new DashboardAlertasDto
                    {
                        ProductosBajoStock = productosBajoStock.Count,
                        OfertasPorVencer = ofertas.Count(o => EsOfertaActivaVigente(o) && o.FechaFin.Date >= ahora.Date && o.FechaFin.Date <= ahora.Date.AddDays(DiasOfertaPorVencer)),
                        PerdidasPendientes = perdidasHistoricas.Count(p => p.IdEstado == (int)EstadoPerdida.Pendiente)
                    },
                    ResumenOperativo = new DashboardResumenOperativoDto
                    {
                        ProductosActivos = productos.Count(p => p.Activo),
                        ProductosInactivos = productos.Count(p => !p.Activo),
                        ClientesActivos = clientes.Count(c => c.Activo),
                        ProveedoresActivos = proveedores.Count(p => p.Activo),
                        VendedoresActivos = vendedores.Count(v => v.Activo),
                        OfertasActivas = ofertas.Count(EsOfertaActivaVigente)
                    },
                    UltimasVentas = ventasUltimas.Select(venta => new DashboardUltimaVentaDto
                    {
                        Id = venta.Id,
                        Fecha = venta.Fecha,
                        Cliente = clientesPorId.TryGetValue(venta.IdCliente, out var cliente)
                            ? cliente.NombreCompleto
                            : "Cliente no encontrado",
                        Vendedor = vendedoresPorId.TryGetValue(venta.IdVendedor, out var vendedor)
                            ? $"{vendedor.Nombre} {vendedor.Apellido}".Trim()
                            : "Vendedor no encontrado",
                        Total = venta.Total,
                        Estado = MapearEstadoVenta(venta.Estado)
                    }).ToList()
                };

                return Ok(respuesta);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        private async Task<decimal> CalcularMontoPerdidas(IEnumerable<Comercio.Domain.Entidades.Perdida> perdidas)
        {
            decimal total = 0;

            foreach (var perdida in perdidas.Where(p => p.IdEstado == (int)EstadoPerdida.Confirmada))
            {
                var detalles = await _detallePerdidasRepository.ObtenerPorPerdida(perdida.Id);

                foreach (var detalle in detalles)
                {
                    var producto = await _productosRepository.ObtenerPorId(detalle.IdProducto);

                    if (producto is null)
                        continue;

                    total += detalle.Cantidad * producto.PrecioCompra;
                }
            }

            return total;
        }

        private static bool EsVentaAnulada(string? estado) =>
            string.Equals(estado, "Anulada", StringComparison.OrdinalIgnoreCase);

        private static bool EsDevolucionVentaActiva(Comercio.Domain.Entidades.DevolucionVenta devolucion) =>
            string.Equals(devolucion.Estado, "Activa", StringComparison.OrdinalIgnoreCase);

        private static bool EsDevolucionCompraActiva(Comercio.Domain.Entidades.DevolucionCompra devolucion) =>
            devolucion.Estado == 1;

        private static bool EsOfertaActivaVigente(Comercio.Domain.Entidades.Oferta oferta)
        {
            var hoy = DateTime.Now.Date;
            return oferta.Activa && oferta.FechaInicio.Date <= hoy && oferta.FechaFin.Date >= hoy;
        }

        private static string MapearEstadoVenta(string? estado)
        {
            if (string.Equals(estado, "Activa", StringComparison.OrdinalIgnoreCase))
                return "Finalizada";

            return estado ?? string.Empty;
        }
    }
}
