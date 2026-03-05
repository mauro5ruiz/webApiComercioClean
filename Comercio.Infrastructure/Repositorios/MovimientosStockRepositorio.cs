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
    }
}
