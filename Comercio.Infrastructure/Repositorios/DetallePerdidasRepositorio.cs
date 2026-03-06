using Comercio.Domain.Entidades;
using Comercio.Domain.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Comercio.Infrastructure.Repositorios
{
    public class DetallePerdidasRepositorio: IDetallePerdidasRepository
    {
        private readonly string _connectionString;

        public DetallePerdidasRepositorio(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<DetallePerdida?> ObtenerPorId(int idDetalle)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT Id, IdPerdida, IdProducto, Cantidad
                FROM DetallePerdidas
                WHERE Id = @Id";

            return await connection.QueryFirstOrDefaultAsync<DetallePerdida>(sql, new { Id = idDetalle });
        }
        public async Task Insertar(DetallePerdida detalle)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"INSERT INTO DetallePerdidas
                        (IdPerdida, IdProducto, Cantidad)
                        VALUES
                        (@IdPerdida, @IdProducto, @Cantidad);";

            await connection.ExecuteAsync(sql, detalle);
        }

        public async Task<IEnumerable<DetallePerdida>> ObtenerPorPerdida(int idPerdida)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT Id, IdPerdida, IdProducto, Cantidad
                        FROM DetallePerdidas
                        WHERE IdPerdida = @IdPerdida
                        ORDER BY Id;";

            return await connection.QueryAsync<DetallePerdida>(sql, new { IdPerdida = idPerdida });
        }

        public async Task Eliminar(int idDetalle)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"DELETE FROM DetallePerdidas
                        WHERE Id = @Id;";

            await connection.ExecuteAsync(sql, new { Id = idDetalle });
        }

        public async Task Actualizar(DetallePerdida detalle)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"UPDATE DetallePerdidas
                SET IdProducto = @IdProducto,
                    Cantidad = @Cantidad
                WHERE Id = @Id;";

            await connection.ExecuteAsync(sql, detalle);
        }
    }
}
