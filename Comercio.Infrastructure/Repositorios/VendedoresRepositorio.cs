using Comercio.Domain.Entidades;
using Comercio.Domain.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Comercio.Infrastructure.Repositorios
{
    public class VendedoresRepositorio : IVendedoresRepository
    {
        private readonly string _connectionString;

        public VendedoresRepositorio(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<Vendedor>> ObtenerTodos(bool incluirEliminados = false)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT Id, Nombre, Apellido, NroDni, Email, Telefono, Direccion, FechaNacimiento, Activo, 
                            FechaAlta, FechaEliminado, Observaciones, PinHash, IdSucursal
                        FROM Vendedores";

            if (!incluirEliminados)
                sql += " WHERE Activo = 1";

            return await connection.QueryAsync<Vendedor>(sql);
        }

        public async Task<Vendedor?> ObtenerPorId(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT Id, Nombre, Apellido, NroDni, Email, Telefono, Direccion, FechaNacimiento, 
                            Activo, FechaAlta, FechaEliminado, Observaciones, PinHash, IdSucursal
                        FROM Vendedores
                        WHERE Id = @Id";

            return await connection.QueryFirstOrDefaultAsync<Vendedor>(sql, new { Id = id });
        }

        public async Task<bool> ExistePorNroDni(string nroDni, int? idVendedor = null)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT COUNT(1)
                        FROM Vendedores
                        WHERE NroDni = @NroDni";

            if (idVendedor.HasValue && idVendedor.Value > 0)
                sql += " AND Id <> @IdVendedor";

            var count = await connection.ExecuteScalarAsync<int>(sql, new { NroDni = nroDni, IdVendedor = idVendedor });

            return count > 0;
        }

        public async Task<int> Crear(Vendedor vendedor)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"INSERT INTO Vendedores
                        (Nombre, Apellido, NroDni, Email, Telefono, Direccion,
                         FechaNacimiento, Activo, FechaAlta, Observaciones,
                         PinHash, IdSucursal)
                        VALUES
                        (@Nombre, @Apellido, @NroDni, @Email, @Telefono, @Direccion,
                         @FechaNacimiento, 1, @FechaAlta, @Observaciones,
                         @PinHash, @IdSucursal);

                        SELECT CAST(SCOPE_IDENTITY() as int);";

            vendedor.FechaAlta = DateTime.UtcNow;
            vendedor.Activo = true;

            return await connection.ExecuteScalarAsync<int>(sql, vendedor);
        }

        public async Task Actualizar(Vendedor vendedor)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"UPDATE Vendedores
                        SET Nombre = @Nombre,
                            Apellido = @Apellido,
                            NroDni = @NroDni,
                            Email = @Email,
                            Telefono = @Telefono,
                            Direccion = @Direccion,
                            FechaNacimiento = @FechaNacimiento,
                            Observaciones = @Observaciones,
                            PinHash = @PinHash,
                            IdSucursal = @IdSucursal
                        WHERE Id = @Id";

            await connection.ExecuteAsync(sql, vendedor);
        }

        public async Task<bool> DarDeBaja(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"UPDATE Vendedores
                        SET Activo = 0,
                            FechaEliminado = @FechaEliminado
                        WHERE Id = @Id AND Activo = 1";

            var filasAfectadas = await connection.ExecuteAsync(sql, new { Id = id, FechaEliminado = DateTime.UtcNow });

            return filasAfectadas > 0;
        }

        public async Task<bool> Restaurar(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"UPDATE Vendedores
                        SET Activo = 1,
                            FechaEliminado = NULL
                        WHERE Id = @Id AND Activo = 0";

            var filasAfectadas = await connection.ExecuteAsync(sql, new { Id = id });

            return filasAfectadas > 0;
        }

        public async Task EliminarPermanentemente(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"DELETE FROM Vendedores
                        WHERE Id = @Id";

            await connection.ExecuteAsync(sql, new { Id = id });
        }
    }
}
