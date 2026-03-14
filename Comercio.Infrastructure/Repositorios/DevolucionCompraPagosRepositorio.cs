using Comercio.Domain.Entidades;
using Comercio.Domain.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Comercio.Infrastructure.Repositorios
{
    public class DevolucionCompraPagosRepositorio : IPagoDevolucionCompraRepository
    {
        private readonly string _connectionString;

        public DevolucionCompraPagosRepositorio(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<PagoDevolucionCompra>> ObtenerPorDevolucion(int idDevolucion)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT Id, IdDevolucionCompra, IdFormaPago, Importe, Fecha
                        FROM PagoDevolucionCompra
                        WHERE IdDevolucionCompra = @IdDevolucion;";

            return await connection.QueryAsync<PagoDevolucionCompra>(sql, new { IdDevolucion = idDevolucion });
        }

        public async Task Insertar(PagoDevolucionCompra pago)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"INSERT INTO PagoDevolucionCompra
                        (IdDevolucionCompra, IdFormaPago, Importe, Fecha)
                        VALUES
                        (@IdDevolucionCompra, @IdFormaPago, @Importe, @Fecha);";

            pago.Fecha = DateTime.UtcNow;

            await connection.ExecuteAsync(sql, pago);
        }
    }
}