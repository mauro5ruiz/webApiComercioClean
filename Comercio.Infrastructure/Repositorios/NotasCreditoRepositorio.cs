using Comercio.Domain.Entidades;
using Comercio.Domain.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Comercio.Infrastructure.Repositorios
{
    public class NotasCreditoRepositorio : INotasCreditoRepository
    {
        private readonly string _connectionString;

        public NotasCreditoRepositorio(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<NotaCredito?> ObtenerPorDevolucion(int idDevolucionVenta)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT Id, IdDevolucionVenta, Codigo, Importe, Saldo, Fecha, FechaVencimiento, Estado, Observaciones
                        FROM NotasCredito
                        WHERE IdDevolucionVenta = @IdDevolucionVenta;";

            return await connection.QueryFirstOrDefaultAsync<NotaCredito>(sql, new { IdDevolucionVenta = idDevolucionVenta });
        }

        public async Task<string> ObtenerSiguienteCodigo()
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT 'NC-' + RIGHT(REPLICATE('0', 6) + CAST(ISNULL(MAX(Id), 0) + 1 AS VARCHAR(6)), 6)
                        FROM NotasCredito WITH (UPDLOCK, HOLDLOCK);";

            return await connection.ExecuteScalarAsync<string>(sql) ?? "NC-000001";
        }

        public async Task<int> Insertar(NotaCredito notaCredito)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"INSERT INTO NotasCredito
                        (IdDevolucionVenta, Codigo, Importe, Saldo, Fecha, FechaVencimiento, Estado, Observaciones)
                        VALUES
                        (@IdDevolucionVenta, @Codigo, @Importe, @Saldo, @Fecha, @FechaVencimiento, @Estado, @Observaciones);

                        SELECT CAST(SCOPE_IDENTITY() AS INT);";

            notaCredito.Fecha = DateTime.Now;
            notaCredito.Estado = string.IsNullOrWhiteSpace(notaCredito.Estado) ? "Activa" : notaCredito.Estado;

            var id = await connection.ExecuteScalarAsync<int>(sql, notaCredito);
            notaCredito.Id = id;

            return id;
        }
    }
}
