using Comercio.Domain.Entidades;
using Comercio.Domain.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Comercio.Infrastructure.Repositorios
{
    public class DetalleComprasRepositorio: IDetalleComprasRepository
    {
        private readonly string _connectionString;

        public DetalleComprasRepositorio(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<DetalleCompra>> ObtenerPorCompra(int idCompra)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT Id, IdCompra, IdProducto, Cantidad, PrecioUnitario, Subtotal
                        FROM DetalleCompra
                        WHERE IdCompra = @IdCompra;";

            return await connection.QueryAsync<DetalleCompra>(sql, new { IdCompra = idCompra });
        }

        public async Task Insertar(DetalleCompra detalle)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"INSERT INTO DetalleCompra
                        (IdCompra, IdProducto, Cantidad, PrecioUnitario, Subtotal)
                        VALUES
                        (@IdCompra, @IdProducto, @Cantidad, @PrecioUnitario, @Subtotal);";

            await connection.ExecuteAsync(sql, detalle);
        }
    }
}
