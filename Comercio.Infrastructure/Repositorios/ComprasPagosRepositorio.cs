using Comercio.Domain.Entidades;
using Comercio.Domain.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Comercio.Infrastructure.Repositorios
{
    public class ComprasPagosRepositorio: IComprasPagosRepository
    {
        private readonly string _connectionString;

        public ComprasPagosRepositorio(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<CompraPago>> ObtenerPorCompra(int idCompra)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT Id, IdCompra, IdFormaPago, Importe, Cuotas, Referencia, FechaPago, Estado
                        FROM CompraPagos
                        WHERE IdCompra = @IdCompra;";

            return await connection.QueryAsync<CompraPago>(sql, new { IdCompra = idCompra });
        }

        public async Task Insertar(CompraPago pago)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"INSERT INTO CompraPagos (IdCompra, IdFormaPago, Importe, Cuotas, Referencia, FechaPago, Estado)
                        VALUES
                        (@IdCompra, @IdFormaPago, @Importe, @Cuotas, @Referencia, @FechaPago, @Estado);";

            pago.FechaPago = DateTime.UtcNow;

            await connection.ExecuteAsync(sql, pago);
        }

        public async Task RecalcularTotalPagado(int idCompra)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"UPDATE Compras
                        SET TotalPagado = (
                            SELECT ISNULL(SUM(Importe), 0)
                            FROM CompraPagos
                            WHERE IdCompra = @IdCompra
                              AND Estado = 1 -- Activo
                        ),
                        SaldoPendiente = Total - (
                            SELECT ISNULL(SUM(Importe), 0)
                            FROM CompraPagos
                            WHERE IdCompra = @IdCompra
                              AND Estado = 1
                        )
                        WHERE Id = @IdCompra;";

            await connection.ExecuteAsync(sql, new { IdCompra = idCompra });
        }

        public async Task CambiarEstado(int idPago, int estado)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"UPDATE CompraPagos SET Estado = @Estado WHERE Id = @Id;";

            await connection.ExecuteAsync(sql, new { Id = idPago, Estado = estado });
        }
    }
}
