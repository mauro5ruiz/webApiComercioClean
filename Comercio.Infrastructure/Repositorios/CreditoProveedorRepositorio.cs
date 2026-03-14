using Comercio.Domain.Entidades;
using Comercio.Domain.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Comercio.Infrastructure.Repositorios
{
    public class CreditoProveedorRepositorio : ICreditoProveedorRepository
    {
        private readonly string _connectionString;

        public CreditoProveedorRepositorio(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<CreditoProveedor>> ObtenerPorProveedor(int idProveedor)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT Id, IdProveedor, IdDevolucionCompra, Importe, Saldo, Fecha
                        FROM CreditoProveedor
                        WHERE IdProveedor = @IdProveedor;";

            return await connection.QueryAsync<CreditoProveedor>(sql, new { IdProveedor = idProveedor });
        }

        public async Task Insertar(CreditoProveedor credito)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"INSERT INTO CreditoProveedor
                        (IdProveedor, IdDevolucionCompra, Importe, Saldo, Fecha)
                        VALUES
                        (@IdProveedor, @IdDevolucionCompra, @Importe, @Saldo, @Fecha);";

            credito.Fecha = DateTime.UtcNow;

            await connection.ExecuteAsync(sql, credito);
        }

        public async Task ConsumirCredito(int idCredito, decimal importe)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"UPDATE CreditoProveedor
                        SET Saldo = Saldo - @Importe
                        WHERE Id = @Id;";

            await connection.ExecuteAsync(sql, new
            {
                Id = idCredito,
                Importe = importe
            });
        }
    }
}