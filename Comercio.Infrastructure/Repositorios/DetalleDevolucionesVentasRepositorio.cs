using Comercio.Domain.Entidades;
using Comercio.Domain.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Comercio.Infrastructure.Repositorios
{
    public class DetalleDevolucionesVentasRepositorio : IDetalleDevolucionesVentasRepository
    {
        private readonly string _connectionString;

        public DetalleDevolucionesVentasRepositorio(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<DetalleDevolucionVenta>> ObtenerPorDevolucion(int idDevolucion)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT Id, IdDevolucionVenta, IdVentaDetalle, IdProducto, Cantidad,
                               PrecioUnitario, Subtotal
                        FROM DetalleDevolucionesVenta
                        WHERE IdDevolucionVenta = @IdDevolucion";

            return await connection.QueryAsync<DetalleDevolucionVenta>(sql, new { IdDevolucion = idDevolucion });
        }

        public async Task Insertar(DetalleDevolucionVenta detalle)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"INSERT INTO DetalleDevolucionesVenta
                        (IdDevolucionVenta, IdVentaDetalle, IdProducto, Cantidad, PrecioUnitario, Subtotal)
                        VALUES
                        (@IdDevolucionVenta, @IdVentaDetalle, @IdProducto, @Cantidad, @PrecioUnitario, @Subtotal);";

            await connection.ExecuteAsync(sql, detalle);
        }
    }
}