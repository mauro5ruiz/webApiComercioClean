using Comercio.Application.Interfaces;
using Comercio.Domain.Entidades;
using Comercio.Domain.Interfaces;

namespace Comercio.Application.Servicios
{
    public class VentasServicio : IVentasServicio
    {
        private readonly IVentasRepository _ventasRepository;
        private readonly IDetalleVentasRepostory _detalleRepository;
        private readonly IVentasPagosRepository _pagosRepository;
        private readonly IProductosRepository _productosRepository;

        public VentasServicio(IVentasRepository ventasRepository, IDetalleVentasRepostory detalleRepository, 
            IVentasPagosRepository pagosRepository, IProductosRepository productosRepository)
        {
            _ventasRepository = ventasRepository;
            _detalleRepository = detalleRepository;
            _pagosRepository = pagosRepository;
            _productosRepository = productosRepository;
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

                if (producto.StockActual < detalle.Cantidad)
                {
                    erroresStock.Add(
                        $"Stock insuficiente para {producto.Nombre}. " +
                        $"Disponible: {producto.StockActual}, " +
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
            venta.Estado = "Activa";
            venta.Fecha = DateTime.UtcNow;

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
                await _productosRepository.DescontarStock(detalle.IdProducto,detalle.Cantidad);
            }

            if (pagos != null && pagos.Any())
            {
                foreach (var pago in pagos)
                {
                    pago.IdVenta = idVenta;
                    pago.FechaPago = DateTime.UtcNow;
                    pago.Estado = "Activo";

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

            if (venta.Estado == "Anulada")
                throw new InvalidOperationException("La venta ya está anulada.");

            var detalles = await _detalleRepository.ObtenerPorVenta(idVenta);
            var pagos = await _pagosRepository.ObtenerPorVenta(idVenta);

            // Devuelvo el stock
            foreach (var detalle in detalles)
                await _productosRepository.AumentarStock(detalle.IdProducto, detalle.Cantidad);

            // Anular pagos (no los borro)
            foreach (var pago in pagos)
                await _pagosRepository.CambiarEstado(pago.Id, "Anulado");

            // Cambio estado de la venta
            await _ventasRepository.CambiarEstado(idVenta, "Anulada");
        }
    }
}
