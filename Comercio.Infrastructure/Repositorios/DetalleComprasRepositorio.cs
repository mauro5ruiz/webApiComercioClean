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

            var sql = @"SELECT
                Id,
                IdCompra,
                IdProducto,
                (Cantidad - ISNULL(CantidadDevuelta, 0)) AS Cantidad,
                PrecioUnitario,
                ((Cantidad - ISNULL(CantidadDevuelta, 0)) * PrecioUnitario) AS Subtotal,
                CantidadDevuelta
            FROM DetalleCompra
            WHERE IdCompra = @IdCompra;";

            return await connection.QueryAsync<DetalleCompra>(sql, new { IdCompra = idCompra });
        }

        public async Task<int?> ObtenerIdDetalleCompra(int idCompra, int idProducto)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT Id
                FROM DetalleCompra
                WHERE IdCompra = @IdCompra
                AND IdProducto = @IdProducto";

            return await connection.QueryFirstOrDefaultAsync<int?>(sql, new { IdCompra = idCompra, IdProducto = idProducto  });
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

        public async Task AgregarCantidadDevuelto(int idDetalleCompra, int idProducto, int cantidad)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"UPDATE DetalleCompra
                SET CantidadDevuelta = CantidadDevuelta + @cantidad
                WHERE Id = @idDetalleCompra
                AND IdProducto = @idProducto";

            await connection.ExecuteAsync(sql, new
            {
                idDetalleCompra,
                idProducto,
                cantidad
            });
        }
    }
}
