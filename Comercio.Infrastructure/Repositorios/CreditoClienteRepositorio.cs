using Comercio.Domain.Entidades;
using Comercio.Domain.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Comercio.Infrastructure.Repositorios
{
    public class CreditoClienteRepositorio: ICreditoClienteRepository
    {
        private readonly string _connectionString;

        public CreditoClienteRepositorio(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<CreditoCliente>> ObtenerPorCliente(int idCliente)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT Id, IdCliente, IdDevolucionVenta, Importe, Saldo, Fecha
                        FROM CreditoCliente
                        WHERE IdCliente = @IdCliente;";

            return await connection.QueryAsync<CreditoCliente>(sql, new { IdCliente = idCliente });
        }

        public async Task Insertar(CreditoCliente credito)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"INSERT INTO CreditoCliente
                        (IdCliente, IdDevolucionVenta, Importe, Saldo, Fecha)
                        VALUES
                        (@IdCliente, @IdDevolucionVenta, @Importe, @Saldo, @Fecha);";

            credito.Fecha = DateTime.Now;

            await connection.ExecuteAsync(sql, credito);
        }

        public async Task<bool> ConsumirCredito(int idCredito, decimal importe)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"UPDATE CreditoCliente
                        SET Saldo = Saldo - @Importe
                        WHERE Id = @Id
                          AND Saldo >= @Importe;";

            var filasAfectadas = await connection.ExecuteAsync(sql, new
            {
                Id = idCredito,
                Importe = importe
            });

            return filasAfectadas > 0;
        }
    }
}
