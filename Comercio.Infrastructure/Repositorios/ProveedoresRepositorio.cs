using Comercio.Domain.Entidades;
using Comercio.Domain.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Comercio.Infrastructure.Repositorios
{
    public class ProveedoresRepositorio : IProveedoresRepository
    {
        private readonly string _connectionString;

        public ProveedoresRepositorio(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<Proveedor?> ObtenerPorId(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT Id, RazonSocial, Cuit, CondicionIva, Telefono, Email, PersonaContacto, 
                               Direccion, Localidad, Provincia, CodigoPostal, Observaciones, 
                               Activo, UrlImagen, FechaCreacion, FechaBaja
                        FROM Proveedores
                        WHERE Id = @Id";

            return await connection.QueryFirstOrDefaultAsync<Proveedor>(sql, new { Id = id });
        }

        public async Task<IEnumerable<Proveedor>> ObtenerTodos(bool incluirEliminados = false)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT Id, RazonSocial, Cuit, CondicionIva, Telefono, Email, PersonaContacto, 
                               Direccion, Localidad, Provincia, CodigoPostal, Observaciones, 
                               Activo, UrlImagen, FechaCreacion, FechaBaja
                        FROM Proveedores";

            if (!incluirEliminados)
                sql += " WHERE Activo = 1";

            return await connection.QueryAsync<Proveedor>(sql);
        }

        public async Task<bool> ExistePorCuit(string cuit, int? idProveedor = null)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT COUNT(1)
                FROM Proveedores
                WHERE Cuit = @Cuit";

            if (idProveedor.HasValue && idProveedor.Value > 0)
                sql += " AND Id <> @IdProveedor";

            var count = await connection.ExecuteScalarAsync<int>(
                sql,
                new { Cuit = cuit, IdProveedor = idProveedor }
            );

            return count > 0;
        }


        public async Task<int> Crear(Proveedor proveedor)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"INSERT INTO Proveedores
                        (RazonSocial, Cuit, CondicionIva, Telefono, Email, PersonaContacto,
                         Direccion, Localidad, Provincia, CodigoPostal, Observaciones,
                         Activo, UrlImagen, FechaCreacion)
                        VALUES
                        (@RazonSocial, @Cuit, @CondicionIva, @Telefono, @Email, @PersonaContacto,
                         @Direccion, @Localidad, @Provincia, @CodigoPostal, @Observaciones,
                         @Activo, @UrlImagen, @FechaCreacion);

                        SELECT CAST(SCOPE_IDENTITY() as int);";

            proveedor.FechaCreacion = DateTime.Now;
            proveedor.Activo = true;

            return await connection.ExecuteScalarAsync<int>(sql, proveedor);
        }

        public async Task Actualizar(Proveedor proveedor)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"UPDATE Proveedores
                        SET RazonSocial = @RazonSocial,
                            Cuit = @Cuit,
                            CondicionIva = @CondicionIva,
                            Telefono = @Telefono,
                            Email = @Email,
                            PersonaContacto = @PersonaContacto,
                            Direccion = @Direccion,
                            Localidad = @Localidad,
                            Provincia = @Provincia,
                            CodigoPostal = @CodigoPostal,
                            Observaciones = @Observaciones,
                            UrlImagen = @UrlImagen,
                            Activo = @Activo
                        WHERE Id = @Id";

            await connection.ExecuteAsync(sql, proveedor);
        }

        public async Task<bool> DarDeBaja(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"UPDATE Proveedores
                        SET Activo = 0,
                            FechaBaja = @FechaBaja
                        WHERE Id = @Id AND Activo = 1";

            var filasAfectadas = await connection.ExecuteAsync(sql,
                new { Id = id, FechaBaja = DateTime.Now });

            return filasAfectadas > 0;
        }

        public async Task<bool> Restaurar(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"UPDATE Proveedores
                        SET Activo = 1,
                            FechaBaja = NULL
                        WHERE Id = @Id AND Activo = 0";

            var filasAfectadas = await connection.ExecuteAsync(sql, new { Id = id });

            return filasAfectadas > 0;
        }

        public async Task EliminarPermanentemente(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"DELETE FROM Proveedores
                        WHERE Id = @Id";

            await connection.ExecuteAsync(sql, new { Id = id });
        }

        public async Task<List<Compra>> ObtenerComprasCuentaCorriente(int idProveedor, DateTime? desde, DateTime? hasta)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
                SELECT 
                    c.Id,
                    c.NumeroComprobante,
                    c.Fecha,
                    c.IdProveedor,
                    c.IdSucursal,
                    c.Total,
                    c.TotalPagado,
                    c.SaldoPendiente,
                    c.Estado,
                    c.Observaciones,
                    c.FechaAnulacion
                FROM Compras c
                WHERE c.IdProveedor = @IdProveedor
                  AND c.Estado = 1
                  AND (@Desde IS NULL OR c.Fecha >= @Desde)
                  AND (@Hasta IS NULL OR c.Fecha < DATEADD(DAY, 1, @Hasta))
                ORDER BY c.Fecha, c.Id;

                SELECT 
                    cp.Id,
                    cp.IdCompra,
                    cp.IdFormaPago,
                    cp.Importe,
                    cp.Cuotas,
                    cp.Referencia,
                    cp.FechaPago,
                    cp.Estado,
                    fp.Nombre AS NombreFormaPago
                FROM CompraPagos cp
                INNER JOIN Compras c ON c.Id = cp.IdCompra
                LEFT JOIN FormasDePago fp ON fp.Id = cp.IdFormaPago
                WHERE c.IdProveedor = @IdProveedor
                  AND (@Desde IS NULL OR c.Fecha >= @Desde)
                  AND (@Hasta IS NULL OR c.Fecha < DATEADD(DAY, 1, @Hasta))
                ORDER BY cp.FechaPago, cp.Id;
            ";

            using var multi = await connection.QueryMultipleAsync(sql, new
            {
                IdProveedor = idProveedor,
                Desde = desde?.Date,
                Hasta = hasta?.Date
            });

            var compras = (await multi.ReadAsync<Compra>()).ToList();
            var pagos = (await multi.ReadAsync<CompraPago>()).ToList();

            var pagosPorCompra = pagos.GroupBy(p => p.IdCompra).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var compra in compras)
            {
                compra.Pagos = pagosPorCompra.TryGetValue(compra.Id, out var pagosCompra)
                    ? pagosCompra
                    : new List<CompraPago>();
            }

            return compras;
        }
    }
}
