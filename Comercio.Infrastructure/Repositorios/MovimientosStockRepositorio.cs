using Comercio.Domain.Entidades;
using Comercio.Domain.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Comercio.Infrastructure.Repositorios
{
    public class MovimientosStockRepositorio : IMovimientosStockRepository
    {
        private readonly string _connectionString;

        public MovimientosStockRepositorio(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<MovimientoStock?> ObtenerPorId(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryFirstOrDefaultAsync<MovimientoStock>(
                "SELECT Id, Observaciones FROM MovimientosStock WHERE Id = @id",
                new { id }
            );
        }

        public async Task<int> ObtenerStockActual(int idProducto)
        {
            using var connection = new SqlConnection(_connectionString);

            var stock = await connection.ExecuteScalarAsync<int?>(
                @"SELECT SUM(Cantidad)
                  FROM MovimientosStock
                  WHERE IdProducto = @idProducto;",
                new { idProducto }
            );

            return stock ?? 0;
        }

        public async Task RegistrarMovimiento(MovimientoStock movimiento)
        {
            using var connection = new SqlConnection(_connectionString);

            await connection.ExecuteAsync(
                @"INSERT INTO MovimientosStock(IdProducto, IdTipoMovimiento, Cantidad, Fecha, IdReferencia, Observaciones)
                  VALUES
                  (@IdProducto, @IdTipoMovimientoStock, @Cantidad, @Fecha, @IdReferencia, @Observaciones);",
                movimiento
            );
        }

        public async Task Actualizar(MovimientoStock movimiento)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"UPDATE MovimientosStock
                        SET Cantidad = @Cantidad
                        WHERE Id = @Id;";

            await connection.ExecuteAsync(sql, movimiento);
        }

        public async Task Eliminar(int idMovimiento)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"DELETE FROM MovimientosStock
                        WHERE Id = @Id;";

            await connection.ExecuteAsync(sql, new { Id = idMovimiento });
        }
    }
}
