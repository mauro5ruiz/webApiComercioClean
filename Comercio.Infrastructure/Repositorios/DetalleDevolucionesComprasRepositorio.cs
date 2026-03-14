using Comercio.Domain.Entidades;
using Comercio.Domain.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Comercio.Infrastructure.Repositorios
{
    public class DetalleDevolucionesComprasRepositorio : IDevolucionCompraDetalleRepository
    {
        private readonly string _connectionString;

        public DetalleDevolucionesComprasRepositorio(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<DevolucionCompraDetalle>> ObtenerPorDevolucion(int idDevolucion)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT Id, IdDevolucionCompra, IdProducto, Cantidad, PrecioUnitario, Subtotal
                        FROM DevolucionCompraDetalle
                        WHERE IdDevolucionCompra = @IdDevolucion;";

            return await connection.QueryAsync<DevolucionCompraDetalle>(sql, new { IdDevolucion = idDevolucion });
        }

        public async Task Insertar(DevolucionCompraDetalle detalle)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"INSERT INTO DevolucionCompraDetalle
                        (IdDevolucionCompra, IdProducto, Cantidad, PrecioUnitario, Subtotal)
                        VALUES
                        (@IdDevolucionCompra, @IdProducto, @Cantidad, @PrecioUnitario, @Subtotal);";

            await connection.ExecuteAsync(sql, detalle);
        }
    }
}