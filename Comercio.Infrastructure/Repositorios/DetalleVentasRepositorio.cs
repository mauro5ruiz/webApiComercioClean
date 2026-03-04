using Comercio.Domain.Entidades;
using Comercio.Domain.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Comercio.Infrastructure.Repositorios
{
    public class DetalleVentasRepositorio : IDetalleVentasRepostory
    {
        private readonly string _connectionString;

        public DetalleVentasRepositorio(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<DetalleVenta>> ObtenerPorVenta(int idVenta)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT Id, IdVenta, IdProducto, Cantidad, PrecioUnitario, Subtotal
                        FROM DetalleVenta
                        WHERE IdVenta = @IdVenta";

            return await connection.QueryAsync<DetalleVenta>(sql, new { IdVenta = idVenta });
        }

        public async Task Insertar(DetalleVenta detalle)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"INSERT INTO DetalleVenta
                        (IdVenta, IdProducto, Cantidad, PrecioUnitario)
                        VALUES
                        (@IdVenta, @IdProducto, @Cantidad, @PrecioUnitario);";

            await connection.ExecuteAsync(sql, detalle);
        }
    }
}
