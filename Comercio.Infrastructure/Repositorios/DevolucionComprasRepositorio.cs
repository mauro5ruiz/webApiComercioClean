using Comercio.Domain.Entidades;
using Comercio.Domain.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Comercio.Infrastructure.Repositorios
{
    public class DevolucionComprasRepositorio : IDevolucionComprasRepository
    {
        private readonly string _connectionString;

        public DevolucionComprasRepositorio(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<DevolucionCompra>> ObtenerPorFechas(DateTime desde, DateTime hasta)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT Id, IdCompra, IdProveedor, Fecha, Motivo, Total, Estado
                        FROM DevolucionCompra
                        WHERE Fecha BETWEEN @Desde AND @Hasta
                        ORDER BY Fecha DESC;";

            return await connection.QueryAsync<DevolucionCompra>(sql, new { Desde = desde, Hasta = hasta });
        }

        public async Task<DevolucionCompra?> ObtenerPorId(int idDevolucion)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT Id, IdCompra, IdProveedor, Fecha, Motivo, Total, Estado
                        FROM DevolucionCompra
                        WHERE Id = @Id;";

            return await connection.QueryFirstOrDefaultAsync<DevolucionCompra>(sql, new { Id = idDevolucion });
        }

        public async Task<bool> Existe(int idDevolucion)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT COUNT(1) FROM DevolucionCompra WHERE Id = @Id;";

            var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = idDevolucion });

            return count > 0;
        }

        public async Task<int> Insertar(DevolucionCompra devolucion)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"INSERT INTO DevolucionCompra
                        (IdCompra, IdProveedor, Fecha, Motivo, Total, Estado)
                        VALUES
                        (@IdCompra, @IdProveedor, @Fecha, @Motivo, @Total, @Estado);

                        SELECT CAST(SCOPE_IDENTITY() as int);";

            return await connection.ExecuteScalarAsync<int>(sql, devolucion);
        }

        public async Task CambiarEstado(int idDevolucion, int estado)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"UPDATE DevolucionCompra
                        SET Estado = @Estado
                        WHERE Id = @Id;";

            await connection.ExecuteAsync(sql, new
            {
                Id = idDevolucion,
                Estado = estado
            });
        }

        public async Task ActualizarTotal(int idDevolucion, decimal total)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"UPDATE DevolucionCompra
                        SET Total = @Total
                        WHERE Id = @Id;";

            await connection.ExecuteAsync(sql, new
            {
                Id = idDevolucion,
                Total = total
            });
        }
    }
}