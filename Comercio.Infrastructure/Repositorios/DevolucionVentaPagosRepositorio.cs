using Comercio.Domain.Entidades;
using Comercio.Domain.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Comercio.Infrastructure.Repositorios
{
    public class DevolucionVentaPagosRepositorio : IDevolucionPagosRepository
    {
        private readonly string _connectionString;

        public DevolucionVentaPagosRepositorio(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<DevolucionVentaPago>> ObtenerPorDevolucion(int idDevolucion)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT Id, IdDevolucionVenta, IdFormaPago, Importe, Referencia, FechaPago, Estado
                        FROM DevolucionVentaPagos
                        WHERE IdDevolucionVenta = @IdDevolucion";

            return await connection.QueryAsync<DevolucionVentaPago>(sql, new { IdDevolucion = idDevolucion });
        }

        public async Task Insertar(DevolucionVentaPago pago)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"INSERT INTO DevolucionVentaPagos
                        (IdDevolucionVenta, IdFormaPago, Importe, Referencia, FechaPago, Estado)
                        VALUES
                        (@IdDevolucionVenta, @IdFormaPago, @Importe, @Referencia, @FechaPago, @Estado);";

            pago.FechaPago = DateTime.UtcNow;
            pago.Estado = string.IsNullOrEmpty(pago.Estado) ? "Activo" : pago.Estado;

            await connection.ExecuteAsync(sql, pago);
        }

        public async Task CambiarEstado(int idPago, string estado)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"UPDATE DevolucionVentaPagos
                        SET Estado = @Estado
                        WHERE Id = @Id";

            await connection.ExecuteAsync(sql, new
            {
                Id = idPago,
                Estado = estado
            });
        }
    }
}