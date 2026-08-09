using Comercio.Application.Tests.Fakes;
using Comercio.Domain.Entidades;
using Comercio.Domain.Enums;

namespace Comercio.Application.Tests
{
    public class ComprasFlowTests
    {
        private const int IdProveedor = 1;
        private const int IdFormaPagoEfectivo = 1;

        [Fact]
        public async Task CrearCompra_ConPagoCompleto_CalculaTotalesYStockCorrectos()
        {
            var db = new FakeComercioDb();
            db.AgregarSucursal();
            var producto = db.AgregarProducto("Producto A");
            var servicio = ServiceFactory.CrearComprasServicio(db);

            var idCompra = await servicio.CrearCompra(
                new Compra { NumeroComprobante = "C-0001", IdProveedor = IdProveedor },
                new[] { new DetalleCompra { IdProducto = producto.Id, Cantidad = 10, PrecioUnitario = 100m } },
                new[] { new CompraPago { IdFormaPago = IdFormaPagoEfectivo, Importe = 1000m } });

            var compra = db.Compras[idCompra];
            Assert.Equal(1000m, compra.Total);
            Assert.Equal(1000m, compra.TotalPagado);
            Assert.Equal(0m, compra.SaldoPendiente);
            Assert.Equal(10, db.StockActualDe(producto.Id));
        }

        [Fact]
        public async Task CrearCompra_ConCreditoProveedorAFavor_AplicaCreditoYReduceSaldoPendiente()
        {
            var db = new FakeComercioDb();
            db.AgregarSucursal();
            var producto = db.AgregarProducto("Producto A");
            db.CreditosProveedor.Add(new CreditoProveedor { Id = 1, IdProveedor = IdProveedor, Importe = 300m, Saldo = 300m });
            var servicio = ServiceFactory.CrearComprasServicio(db);

            var idCompra = await servicio.CrearCompra(
                new Compra { NumeroComprobante = "C-0002", IdProveedor = IdProveedor, CreditoAplicado = 300m },
                new[] { new DetalleCompra { IdProducto = producto.Id, Cantidad = 10, PrecioUnitario = 100m } },
                new[] { new CompraPago { IdFormaPago = IdFormaPagoEfectivo, Importe = 700m } });

            var compra = db.Compras[idCompra];
            Assert.Equal(1000m, compra.Total);
            Assert.Equal(700m, compra.TotalPagado);
            Assert.Equal(300m, compra.CreditoAplicado);
            Assert.Equal(0m, compra.SaldoPendiente);
            Assert.Equal(0m, db.CreditosProveedor.Single().Saldo);
        }

        [Fact]
        public async Task CrearCompra_CreditoAplicadoSuperaDisponible_Lanza()
        {
            var db = new FakeComercioDb();
            db.AgregarSucursal();
            var producto = db.AgregarProducto("Producto A");
            db.CreditosProveedor.Add(new CreditoProveedor { Id = 1, IdProveedor = IdProveedor, Importe = 100m, Saldo = 100m });
            var servicio = ServiceFactory.CrearComprasServicio(db);

            await Assert.ThrowsAsync<InvalidOperationException>(() => servicio.CrearCompra(
                new Compra { NumeroComprobante = "C-0003", IdProveedor = IdProveedor, CreditoAplicado = 300m },
                new[] { new DetalleCompra { IdProducto = producto.Id, Cantidad = 10, PrecioUnitario = 100m } },
                new[] { new CompraPago { IdFormaPago = IdFormaPagoEfectivo, Importe = 700m } }));
        }

        [Fact]
        public async Task DevolucionCompra_Completa_ConReembolsoEnEfectivo_ActualizaSaldosDeCompra()
        {
            var db = new FakeComercioDb();
            db.AgregarSucursal();
            var producto = db.AgregarProducto("Producto A");
            var compras = ServiceFactory.CrearComprasServicio(db);
            var devoluciones = ServiceFactory.CrearDevolucionesComprasServicio(db);

            var idCompra = await compras.CrearCompra(
                new Compra { NumeroComprobante = "C-0004", IdProveedor = IdProveedor },
                new[] { new DetalleCompra { IdProducto = producto.Id, Cantidad = 10, PrecioUnitario = 100m } },
                new[] { new CompraPago { IdFormaPago = IdFormaPagoEfectivo, Importe = 1000m } });

            await devoluciones.RegistrarDevolucion(
                new DevolucionCompra { IdCompra = idCompra, Motivo = "Devolucion total" },
                new[] { new DevolucionCompraDetalle { IdProducto = producto.Id, Cantidad = 10 } },
                new[] { new PagoDevolucionCompra { IdFormaPago = IdFormaPagoEfectivo, Importe = 1000m } });

            var compra = db.Compras[idCompra];
            Assert.Equal(0m, compra.Total);
            Assert.Equal(0m, compra.TotalPagado);
            Assert.Equal(0m, compra.SaldoPendiente);
            Assert.Empty(db.CreditosProveedor);
            Assert.Equal(0, db.StockActualDe(producto.Id));
        }

        [Fact]
        public async Task DevolucionCompra_Completa_SinReembolso_GeneraCreditoProveedor()
        {
            var db = new FakeComercioDb();
            db.AgregarSucursal();
            var producto = db.AgregarProducto("Producto A");
            var compras = ServiceFactory.CrearComprasServicio(db);
            var devoluciones = ServiceFactory.CrearDevolucionesComprasServicio(db);

            var idCompra = await compras.CrearCompra(
                new Compra { NumeroComprobante = "C-0005", IdProveedor = IdProveedor },
                new[] { new DetalleCompra { IdProducto = producto.Id, Cantidad = 10, PrecioUnitario = 100m } },
                new[] { new CompraPago { IdFormaPago = IdFormaPagoEfectivo, Importe = 1000m } });

            await devoluciones.RegistrarDevolucion(
                new DevolucionCompra { IdCompra = idCompra, Motivo = "Se queda como saldo a favor" },
                new[] { new DevolucionCompraDetalle { IdProducto = producto.Id, Cantidad = 10 } });

            var compra = db.Compras[idCompra];
            Assert.Equal(0m, compra.Total);
            Assert.Equal(0m, compra.TotalPagado);
            var credito = Assert.Single(db.CreditosProveedor);
            Assert.Equal(1000m, credito.Saldo);
            Assert.Equal(IdProveedor, credito.IdProveedor);
        }

        [Fact]
        public async Task DevolucionCompra_Parcial_DosVeces_NoPermiteDobleReembolso()
        {
            // Regresion: antes del fix, cada devolucion parcial validaba contra el TotalPagado
            // ORIGINAL de la compra (nunca actualizado), permitiendo cobrar en efectivo mas de
            // una vez sobre el mismo dinero cuando la compra tenia saldo pendiente.
            var db = new FakeComercioDb();
            db.AgregarSucursal();
            var productoA = db.AgregarProducto("Producto A");
            var productoB = db.AgregarProducto("Producto B");
            var compras = ServiceFactory.CrearComprasServicio(db);
            var devoluciones = ServiceFactory.CrearDevolucionesComprasServicio(db);

            var idCompra = await compras.CrearCompra(
                new Compra { NumeroComprobante = "C-0006", IdProveedor = IdProveedor },
                new[]
                {
                    new DetalleCompra { IdProducto = productoA.Id, Cantidad = 1, PrecioUnitario = 500m },
                    new DetalleCompra { IdProducto = productoB.Id, Cantidad = 1, PrecioUnitario = 500m }
                },
                new[] { new CompraPago { IdFormaPago = IdFormaPagoEfectivo, Importe = 500m } }); // solo se pago la mitad

            await devoluciones.RegistrarDevolucion(
                new DevolucionCompra { IdCompra = idCompra, Motivo = "Devuelve A" },
                new[] { new DevolucionCompraDetalle { IdProducto = productoA.Id, Cantidad = 1 } },
                new[] { new PagoDevolucionCompra { IdFormaPago = IdFormaPagoEfectivo, Importe = 500m } });

            Assert.Equal(0m, db.Compras[idCompra].TotalPagado);

            await Assert.ThrowsAsync<InvalidOperationException>(() => devoluciones.RegistrarDevolucion(
                new DevolucionCompra { IdCompra = idCompra, Motivo = "Devuelve B, intenta cobrar de nuevo" },
                new[] { new DevolucionCompraDetalle { IdProducto = productoB.Id, Cantidad = 1 } },
                new[] { new PagoDevolucionCompra { IdFormaPago = IdFormaPagoEfectivo, Importe = 500m } }));

            await devoluciones.RegistrarDevolucion(
                new DevolucionCompra { IdCompra = idCompra, Motivo = "Devuelve B, sin cobrar (nunca se pago)" },
                new[] { new DevolucionCompraDetalle { IdProducto = productoB.Id, Cantidad = 1 } });

            var compraFinal = db.Compras[idCompra];
            Assert.Equal(0m, compraFinal.Total);
            Assert.Equal(0m, compraFinal.TotalPagado);
            Assert.Equal(0m, compraFinal.SaldoPendiente);
            Assert.Empty(db.CreditosProveedor);
        }

        [Fact]
        public async Task DevolucionCompra_GeneraCredito_YSeAplicaEnNuevaCompra()
        {
            var db = new FakeComercioDb();
            db.AgregarSucursal();
            var producto = db.AgregarProducto("Producto A");
            var compras = ServiceFactory.CrearComprasServicio(db);
            var devoluciones = ServiceFactory.CrearDevolucionesComprasServicio(db);

            var idCompra1 = await compras.CrearCompra(
                new Compra { NumeroComprobante = "C-0007", IdProveedor = IdProveedor },
                new[] { new DetalleCompra { IdProducto = producto.Id, Cantidad = 5, PrecioUnitario = 100m } },
                new[] { new CompraPago { IdFormaPago = IdFormaPagoEfectivo, Importe = 500m } });

            await devoluciones.RegistrarDevolucion(
                new DevolucionCompra { IdCompra = idCompra1, Motivo = "Se queda como saldo a favor" },
                new[] { new DevolucionCompraDetalle { IdProducto = producto.Id, Cantidad = 5 } });

            Assert.Equal(500m, db.CreditosProveedor.Single().Saldo);

            var idCompra2 = await compras.CrearCompra(
                new Compra { NumeroComprobante = "C-0008", IdProveedor = IdProveedor, CreditoAplicado = 300m },
                new[] { new DetalleCompra { IdProducto = producto.Id, Cantidad = 3, PrecioUnitario = 100m } });

            var compra2 = db.Compras[idCompra2];
            Assert.Equal(300m, compra2.Total);
            Assert.Equal(300m, compra2.CreditoAplicado);
            Assert.Equal(0m, compra2.SaldoPendiente);
            Assert.Equal(200m, db.CreditosProveedor.Single().Saldo);
        }

        [Fact]
        public async Task DevolucionCompra_LineaDuplicadaMismoProducto_RespetaCantidadComprada()
        {
            // Regresion: la validacion original consultaba la cantidad disponible sin descontar
            // lo consumido por lineas anteriores del mismo request, permitiendo devolver mas
            // unidades de las compradas si el producto aparecia repetido.
            var db = new FakeComercioDb();
            db.AgregarSucursal();
            var producto = db.AgregarProducto("Producto A");
            var compras = ServiceFactory.CrearComprasServicio(db);
            var devoluciones = ServiceFactory.CrearDevolucionesComprasServicio(db);

            var idCompra = await compras.CrearCompra(
                new Compra { NumeroComprobante = "C-0009", IdProveedor = IdProveedor },
                new[] { new DetalleCompra { IdProducto = producto.Id, Cantidad = 5, PrecioUnitario = 100m } },
                new[] { new CompraPago { IdFormaPago = IdFormaPagoEfectivo, Importe = 500m } });

            await Assert.ThrowsAsync<InvalidOperationException>(() => devoluciones.RegistrarDevolucion(
                new DevolucionCompra { IdCompra = idCompra, Motivo = "Dos lineas del mismo producto" },
                new[]
                {
                    new DevolucionCompraDetalle { IdProducto = producto.Id, Cantidad = 3 },
                    new DevolucionCompraDetalle { IdProducto = producto.Id, Cantidad = 3 }
                }));
        }

        [Fact]
        public async Task DevolucionCompra_ExcedeCantidadComprada_Lanza()
        {
            var db = new FakeComercioDb();
            db.AgregarSucursal();
            var producto = db.AgregarProducto("Producto A");
            var compras = ServiceFactory.CrearComprasServicio(db);
            var devoluciones = ServiceFactory.CrearDevolucionesComprasServicio(db);

            var idCompra = await compras.CrearCompra(
                new Compra { NumeroComprobante = "C-0010", IdProveedor = IdProveedor },
                new[] { new DetalleCompra { IdProducto = producto.Id, Cantidad = 5, PrecioUnitario = 100m } },
                new[] { new CompraPago { IdFormaPago = IdFormaPagoEfectivo, Importe = 500m } });

            await Assert.ThrowsAsync<InvalidOperationException>(() => devoluciones.RegistrarDevolucion(
                new DevolucionCompra { IdCompra = idCompra },
                new[] { new DevolucionCompraDetalle { IdProducto = producto.Id, Cantidad = 6 } }));
        }

        [Fact]
        public async Task DevolucionCompra_ExcedeStockActual_Lanza()
        {
            // Regresion: la devolucion a proveedor no validaba que la mercaderia siguiera
            // fisicamente en stock (pudo haberse vendido) antes de sacarla del inventario.
            var db = new FakeComercioDb();
            db.AgregarSucursal();
            var producto = db.AgregarProducto("Producto A");
            var compras = ServiceFactory.CrearComprasServicio(db);
            var devoluciones = ServiceFactory.CrearDevolucionesComprasServicio(db);

            var idCompra = await compras.CrearCompra(
                new Compra { NumeroComprobante = "C-0011", IdProveedor = IdProveedor },
                new[] { new DetalleCompra { IdProducto = producto.Id, Cantidad = 10, PrecioUnitario = 100m } },
                new[] { new CompraPago { IdFormaPago = IdFormaPagoEfectivo, Importe = 1000m } });

            // Se venden 8 de las 10 unidades compradas (queda stock = 2).
            await ((Comercio.Domain.Interfaces.IMovimientosStockRepository)db).RegistrarMovimiento(new MovimientoStock
            {
                IdProducto = producto.Id,
                Cantidad = -8,
                IdTipoMovimientoStock = TipoMovimientoStock.Venta,
                Fecha = DateTime.Now
            });

            Assert.Equal(2, db.StockActualDe(producto.Id));

            await Assert.ThrowsAsync<InvalidOperationException>(() => devoluciones.RegistrarDevolucion(
                new DevolucionCompra { IdCompra = idCompra },
                new[] { new DevolucionCompraDetalle { IdProducto = producto.Id, Cantidad = 10 } }));
        }
    }
}
