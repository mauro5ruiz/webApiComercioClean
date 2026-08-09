using Comercio.Application.Servicios;

namespace Comercio.Application.Tests.Fakes
{
    /// <summary>
    /// Arma los servicios reales de la aplicacion cableados contra un mismo FakeComercioDb,
    /// tal como los cablea Program.cs, para poder probar la logica de negocio real
    /// sin depender de una base de datos.
    /// </summary>
    internal static class ServiceFactory
    {
        public static ComprasServicio CrearComprasServicio(FakeComercioDb db) =>
            new(db, db, db, db, db, db, db, db, db);

        public static DevolucionesComprasServicio CrearDevolucionesComprasServicio(FakeComercioDb db) =>
            new(db, db, db, db, db, db, db, db);

        public static VentasServicio CrearVentasServicio(FakeComercioDb db) =>
            new(db, db, db, db, db, db, db, db, db, db, db);

        public static DevolucionesVentasServicio CrearDevolucionesVentasServicio(FakeComercioDb db) =>
            new(db, db, db, db, db, db, db, db, db);
    }
}
