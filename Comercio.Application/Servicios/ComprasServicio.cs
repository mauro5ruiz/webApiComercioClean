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
        private readonly IProductosRepository _productosRepository;

        public ComprasServicio(
            IComprasRepostory comprasRepository,
            IDetalleComprasRepository detalleRepository,
            IComprasPagosRepository pagosRepository,
            IProductosRepository productosRepository)
        {
            _comprasRepository = comprasRepository;
            _detalleRepository = detalleRepository;
            _pagosRepository = pagosRepository;
            _productosRepository = productosRepository;
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
            compra.Fecha = DateTime.UtcNow;

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
                await _productosRepository.AumentarStock(detalle.IdProducto, detalle.Cantidad);
            }

            // Inserto los pagos (si existen)
            if (pagos != null && pagos.Any())
            {
                foreach (var pago in pagos)
                {
                    pago.IdCompra = idCompra;
                    pago.FechaPago = DateTime.UtcNow;
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

            foreach (var detalle in detalles)
                await _productosRepository.DescontarStock(detalle.IdProducto, detalle.Cantidad);
            
            // Anulo los pagos
            foreach (var pago in pagos)
                await _pagosRepository.CambiarEstado(pago.Id, 2);

            await _pagosRepository.RecalcularTotalPagado(idCompra);

            await _comprasRepository.CambiarEstado(idCompra, 2);
        }
    }
}
