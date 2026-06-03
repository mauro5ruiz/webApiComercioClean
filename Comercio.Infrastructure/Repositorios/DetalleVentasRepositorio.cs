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

            var sql = @"SELECT Id, 
                IdVenta, 
                IdProducto, 
                (Cantidad - ISNULL(CantidadDevuelta, 0)) AS Cantidad, 
                PrecioUnitario, 
                ((Cantidad - ISNULL(CantidadDevuelta, 0)) * PrecioUnitario) AS Subtotal,
                CantidadDevuelta
             FROM DetalleVenta
             WHERE IdVenta = @IdVenta";

            return await connection.QueryAsync<DetalleVenta>(sql, new { IdVenta = idVenta });
        }

        public async Task<int?> ObtenerIdDetalleVenta(int idVenta, int idProducto)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT Id
                FROM DetalleVenta
                WHERE IdVenta = @IdVenta
                AND IdProducto = @IdProducto";

            return await connection.QueryFirstOrDefaultAsync<int?>(sql, new { IdVenta = idVenta, IdProducto = idProducto });
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

        public async Task AgregarCantidadDevuelto(int idDetalleVenta, int idProducto, int cantidad)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"UPDATE DetalleVenta
                SET CantidadDevuelta = CantidadDevuelta + @cantidad
                WHERE Id = @idDetalleVenta
                AND IdProducto = @idProducto";

            await connection.ExecuteAsync(sql, new { idDetalleVenta, idProducto, cantidad });
        }
    }
}
