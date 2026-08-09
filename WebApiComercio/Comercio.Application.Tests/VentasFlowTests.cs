using Comercio.Application.Tests.Fakes;
using Comercio.Domain.Entidades;

namespace Comercio.Application.Tests
{
    public class VentasFlowTests
    {
        private const int IdClienteRegistrado = 1;
        private const int IdConsumidorFinal = 0;
        private const int IdFormaPagoEfectivo = 1;

        [Fact]
        public async Task CrearVenta_ConPagoCompleto_CalculaTotalesYStockCorrectos()
        {
            var db = new FakeComercioDb();
            db.AgregarSucursal();
            var producto = db.AgregarProducto("Producto A", stockInicial: 20);
            var servicio = ServiceFactory.CrearVentasServicio(db);

            var idVenta = await servicio.CrearVenta(
                new Venta { NumeroComprobante = "V-0001", IdCliente = IdConsumidorFinal },
                new[] { new DetalleVenta { IdProducto = producto.Id, Cantidad = 10, PrecioUnitario = 150m } },
                new[] { new VentaPago { IdFormaPago = IdFormaPagoEfectivo, Importe = 1500m } });

            var venta = db.Ventas[idVenta];
            Assert.Equal(1500m, venta.Total);
            Assert.Equal(1500m, venta.TotalPagado);
            Assert.Equal(0m, venta.Total - venta.TotalPagado - venta.CreditoAplicado);
            Assert.Equal(10, db.StockActualDe(producto.Id));
        }

        [Fact]
        public async Task CrearVenta_ConCreditoClienteAFavor_AplicaCreditoYReduceSaldoPendiente()
        {
            var db = new FakeComercioDb();
            db.AgregarSucursal();
            var producto = db.AgregarProducto("Producto A", stockInicial: 20);
            db.CreditosCliente.Add(new CreditoCliente { Id = 1, IdCliente = IdClienteRegistrado, Importe = 300m, Saldo = 300m });
            var servicio = ServiceFactory.CrearVentasServicio(db);

            var idVenta = await servicio.CrearVenta(
                new Venta { NumeroComprobante = "V-0002", IdCliente = IdClienteRegistrado, CreditoAplicado = 300m },
                new[] { new DetalleVenta { IdProducto = producto.Id, Cantidad = 10, PrecioUnitario = 150m } },
                new[] { new VentaPago { IdFormaPago = IdFormaPagoEfectivo, Importe = 1200m } });

            var venta = db.Ventas[idVenta];
            Assert.Equal(1500m, venta.Total);
            Assert.Equal(1200m, venta.TotalPagado);
            Assert.Equal(300m, venta.CreditoAplicado);
            Assert.Equal(0m, venta.SaldoPendiente);
            Assert.Equal(0m, db.CreditosCliente.Single().Saldo);
        }

        [Fact]
        public async Task CrearVenta_CreditoAplicadoSuperaDisponible_Lanza()
        {
            var db = new FakeComercioDb();
            db.AgregarSucursal();
            var producto = db.AgregarProducto("Producto A", stockInicial: 20);
            db.CreditosCliente.Add(new CreditoCliente { Id = 1, IdCliente = IdClienteRegistrado, Importe = 100m, Saldo = 100m });
            var servicio = ServiceFactory.CrearVentasServicio(db);

            await Assert.ThrowsAsync<InvalidOperationException>(() => servicio.CrearVenta(
                new Venta { NumeroComprobante = "V-0003", IdCliente = IdClienteRegistrado, CreditoAplicado = 300m },
                new[] { new DetalleVenta { IdProducto = producto.Id, Cantidad = 10, PrecioUnitario = 150m } },
                new[] { new VentaPago { IdFormaPago = IdFormaPagoEfectivo, Importe = 1200m } }));
        }

        [Fact]
        public async Task DevolucionVenta_Completa_ConReembolsoEnEfectivo_ActualizaSaldosDeVenta()
        {
            var db = new FakeComercioDb();
            db.AgregarSucursal();
            var producto = db.AgregarProducto("Producto A", stockInicial: 20);
            var ventas = ServiceFactory.CrearVentasServicio(db);
            var devoluciones = ServiceFactory.CrearDevolucionesVentasServicio(db);

            var idVenta = await ventas.CrearVenta(
                new Venta { NumeroComprobante = "V-0004", IdCliente = IdConsumidorFinal },
                new[] { new DetalleVenta { IdProducto = producto.Id, Cantidad = 10, PrecioUnitario = 100m } },
                new[] { new VentaPago { IdFormaPago = IdFormaPagoEfectivo, Importe = 1000m } });

            await devoluciones.RegistrarDevolucion(
                new DevolucionVenta { IdVenta = idVenta, Observaciones = "Devolucion total" },
                new[] { new DetalleDevolucionVenta { IdProducto = producto.Id, Cantidad = 10 } },
                new[] { new DevolucionVentaPago { IdFormaPago = IdFormaPagoEfectivo, Importe = 1000m } });

            var venta = db.Ventas[idVenta];
            Assert.Equal(0m, venta.Total);
            Assert.Equal(0m, venta.TotalPagado);
            Assert.Equal(0m, venta.SaldoPendiente);
            Assert.Empty(db.CreditosCliente);
            Assert.Empty(db.NotasCredito);
            Assert.Equal(20, db.StockActualDe(producto.Id));
        }

        [Fact]
        public async Task DevolucionVenta_Completa_SinReembolso_ClienteRegistrado_GeneraSaldoAFavor()
        {
            var db = new FakeComercioDb();
            db.AgregarSucursal();
            var producto = db.AgregarProducto("Producto A", stockInicial: 20);
            var ventas = ServiceFactory.CrearVentasServicio(db);
            var devoluciones = ServiceFactory.CrearDevolucionesVentasServicio(db);

            var idVenta = await ventas.CrearVenta(
                new Venta { NumeroComprobante = "V-0005", IdCliente = IdClienteRegistrado },
                new[] { new DetalleVenta { IdProducto = producto.Id, Cantidad = 10, PrecioUnitario = 100m } },
                new[] { new VentaPago { IdFormaPago = IdFormaPagoEfectivo, Importe = 1000m } });

            await devoluciones.RegistrarDevolucion(
                new DevolucionVenta { IdVenta = idVenta, IdCliente = IdClienteRegistrado, Observaciones = "Queda como saldo a favor" },
                new[] { new DetalleDevolucionVenta { IdProducto = producto.Id, Cantidad = 10 } });

            var credito = Assert.Single(db.CreditosCliente);
            Assert.Equal(1000m, credito.Saldo);
            Assert.Equal(IdClienteRegistrado, credito.IdCliente);
            Assert.Empty(db.NotasCredito);
        }

        [Fact]
        public async Task DevolucionVenta_ConsumidorFinal_SinReembolso_GeneraNotaCredito()
        {
            var db = new FakeComercioDb();
            db.AgregarSucursal();
            var producto = db.AgregarProducto("Producto A", stockInicial: 20);
            var ventas = ServiceFactory.CrearVentasServicio(db);
            var devoluciones = ServiceFactory.CrearDevolucionesVentasServicio(db);

            var idVenta = await ventas.CrearVenta(
                new Venta { NumeroComprobante = "V-0006", IdCliente = IdConsumidorFinal },
                new[] { new DetalleVenta { IdProducto = producto.Id, Cantidad = 5, PrecioUnitario = 100m } },
                new[] { new VentaPago { IdFormaPago = IdFormaPagoEfectivo, Importe = 500m } });

            await devoluciones.RegistrarDevolucion(
                new DevolucionVenta { IdVenta = idVenta, IdCliente = IdConsumidorFinal, Observaciones = "Consumidor final" },
                new[] { new DetalleDevolucionVenta { IdProducto = producto.Id, Cantidad = 5 } });

            Assert.Empty(db.CreditosCliente);
            var nota = Assert.Single(db.NotasCredito);
            Assert.Equal(500m, nota.Importe);
        }

        [Fact]
        public async Task DevolucionVenta_Parcial_DosVeces_NoPermiteDobleReembolso()
        {
            // Regresion: antes del fix, cada devolucion parcial validaba contra el TotalPagado
            // ORIGINAL de la venta (nunca actualizado), permitiendo cobrar/emitir saldo a favor
            // mas de una vez sobre el mismo dinero cuando la venta tenia saldo pendiente.
            var db = new FakeComercioDb();
            db.AgregarSucursal();
            var productoA = db.AgregarProducto("Producto A", stockInicial: 20);
            var productoB = db.AgregarProducto("Producto B", stockInicial: 20);
            var ventas = ServiceFactory.CrearVentasServicio(db);
            var devoluciones = ServiceFactory.CrearDevolucionesVentasServicio(db);

            var idVenta = await ventas.CrearVenta(
                new Venta { NumeroComprobante = "V-0007", IdCliente = IdClienteRegistrado },
                new[]
                {
                    new DetalleVenta { IdProducto = productoA.Id, Cantidad = 1, PrecioUnitario = 500m },
                    new DetalleVenta { IdProducto = productoB.Id, Cantidad = 1, PrecioUnitario = 500m }
                },
                new[] { new VentaPago { IdFormaPago = IdFormaPagoEfectivo, Importe = 500m } }); // solo se cobro la mitad

            await devoluciones.RegistrarDevolucion(
                new DevolucionVenta { IdVenta = idVenta, IdCliente = IdClienteRegistrado, Observaciones = "Devuelve A" },
                new[] { new DetalleDevolucionVenta { IdProducto = productoA.Id, Cantidad = 1 } },
                new[] { new DevolucionVentaPago { IdFormaPago = IdFormaPagoEfectivo, Importe = 500m } });

            Assert.Equal(0m, db.Ventas[idVenta].TotalPagado);

            await Assert.ThrowsAsync<InvalidOperationException>(() => devoluciones.RegistrarDevolucion(
                new DevolucionVenta { IdVenta = idVenta, IdCliente = IdClienteRegistrado, Observaciones = "Devuelve B, intenta cobrar de nuevo" },
                new[] { new DetalleDevolucionVenta { IdProducto = productoB.Id, Cantidad = 1 } },
                new[] { new DevolucionVentaPago { IdFormaPago = IdFormaPagoEfectivo, Importe = 500m } }));

            await devoluciones.RegistrarDevolucion(
                new DevolucionVenta { IdVenta = idVenta, IdCliente = IdClienteRegistrado, Observaciones = "Devuelve B, sin cobrar (nunca se cobro)" },
                new[] { new DetalleDevolucionVenta { IdProducto = productoB.Id, Cantidad = 1 } });

            var ventaFinal = db.Ventas[idVenta];
            Assert.Equal(0m, ventaFinal.Total);
            Assert.Equal(0m, ventaFinal.TotalPagado);
            Assert.Equal(0m, ventaFinal.SaldoPendiente);
            Assert.Empty(db.CreditosCliente);
        }

        [Fact]
        public async Task DevolucionVenta_GeneraSaldoAFavor_YSeAplicaEnNuevaVenta()
        {
            var db = new FakeComercioDb();
            db.AgregarSucursal();
            var producto = db.AgregarProducto("Producto A", stockInicial: 20);
            var ventas = ServiceFactory.CrearVentasServicio(db);
            var devoluciones = ServiceFactory.CrearDevolucionesVentasServicio(db);

            var idVenta1 = await ventas.CrearVenta(
                new Venta { NumeroComprobante = "V-0008", IdCliente = IdClienteRegistrado },
                new[] { new DetalleVenta { IdProducto = producto.Id, Cantidad = 5, PrecioUnitario = 100m } },
                new[] { new VentaPago { IdFormaPago = IdFormaPagoEfectivo, Importe = 500m } });

            await devoluciones.RegistrarDevolucion(
                new DevolucionVenta { IdVenta = idVenta1, IdCliente = IdClienteRegistrado, Observaciones = "Saldo a favor" },
                new[] { new DetalleDevolucionVenta { IdProducto = producto.Id, Cantidad = 5 } });

            Assert.Equal(500m, db.CreditosCliente.Single().Saldo);

            var idVenta2 = await ventas.CrearVenta(
                new Venta { NumeroComprobante = "V-0009", IdCliente = IdClienteRegistrado, CreditoAplicado = 300m },
                new[] { new DetalleVenta { IdProducto = producto.Id, Cantidad = 3, PrecioUnitario = 100m } });

            var venta2 = db.Ventas[idVenta2];
            Assert.Equal(300m, venta2.Total);
            Assert.Equal(300m, venta2.CreditoAplicado);
            Assert.Equal(0m, venta2.SaldoPendiente);
            Assert.Equal(200m, db.CreditosCliente.Single().Saldo);
        }

        [Fact]
        public async Task DevolucionVenta_LineaDuplicadaMismoProducto_RespetaCantidadVendida()
        {
            // Regresion: la validacion original consultaba la cantidad disponible sin descontar
            // lo consumido por lineas anteriores del mismo request.
            var db = new FakeComercioDb();
            db.AgregarSucursal();
            var producto = db.AgregarProducto("Producto A", stockInicial: 20);
            var ventas = ServiceFactory.CrearVentasServicio(db);
            var devoluciones = ServiceFactory.CrearDevolucionesVentasServicio(db);

            var idVenta = await ventas.CrearVenta(
                new Venta { NumeroComprobante = "V-0010", IdCliente = IdConsumidorFinal },
                new[] { new DetalleVenta { IdProducto = producto.Id, Cantidad = 5, PrecioUnitario = 100m } },
                new[] { new VentaPago { IdFormaPago = IdFormaPagoEfectivo, Importe = 500m } });

            await Assert.ThrowsAsync<InvalidOperationException>(() => devoluciones.RegistrarDevolucion(
                new DevolucionVenta { IdVenta = idVenta, Observaciones = "Dos lineas del mismo producto" },
                new[]
                {
                    new DetalleDevolucionVenta { IdProducto = producto.Id, Cantidad = 3 },
                    new DetalleDevolucionVenta { IdProducto = producto.Id, Cantidad = 3 }
                }));
        }

        [Fact]
        public async Task DevolucionVenta_ExcedeCantidadVendida_Lanza()
        {
            var db = new FakeComercioDb();
            db.AgregarSucursal();
            var producto = db.AgregarProducto("Producto A", stockInicial: 20);
            var ventas = ServiceFactory.CrearVentasServicio(db);
            var devoluciones = ServiceFactory.CrearDevolucionesVentasServicio(db);

            var idVenta = await ventas.CrearVenta(
                new Venta { NumeroComprobante = "V-0011", IdCliente = IdConsumidorFinal },
                new[] { new DetalleVenta { IdProducto = producto.Id, Cantidad = 5, PrecioUnitario = 100m } },
                new[] { new VentaPago { IdFormaPago = IdFormaPagoEfectivo, Importe = 500m } });

            await Assert.ThrowsAsync<InvalidOperationException>(() => devoluciones.RegistrarDevolucion(
                new DevolucionVenta { IdVenta = idVenta },
                new[] { new DetalleDevolucionVenta { IdProducto = producto.Id, Cantidad = 6 } }));
        }
    }
}
