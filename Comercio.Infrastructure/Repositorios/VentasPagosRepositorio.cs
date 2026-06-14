using Comercio.Domain.Entidades;
using Comercio.Domain.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Comercio.Infrastructure.Repositorios
{
    public class VentasPagosRepositorio : IVentasPagosRepository
    {
        private readonly string _connectionString;

        public VentasPagosRepositorio(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<VentaPago>> ObtenerPorVenta(int idVenta)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT Id, IdVenta, IdFormaPago, Importe, Cuotas, Referencia, FechaPago, Estado
                        FROM VentaPagos
                        WHERE IdVenta = @IdVenta";

            return await connection.QueryAsync<VentaPago>(sql, new { IdVenta = idVenta });
        }

        public async Task Insertar(VentaPago pago)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"INSERT INTO VentaPagos (IdVenta, IdFormaPago, Importe, Cuotas, Referencia, FechaPago, Estado)
                        VALUES
                        (@IdVenta, @IdFormaPago, @Importe, @Cuotas, @Referencia, @FechaPago, @Estado);";

            pago.FechaPago = DateTime.Now;
            pago.Estado = string.IsNullOrEmpty(pago.Estado) ? "Activo" : pago.Estado;

            await connection.ExecuteAsync(sql, pago);
        }

        public async Task RecalcularTotalPagado(int idVenta)
        {
            using var connection = new SqlConnection(_connectionString);

            await RecalcularTotalPagado(idVenta, null);
        }

        public async Task RecalcularTotalPagado(int idVenta, decimal creditoAplicado)
        {
            await RecalcularTotalPagado(idVenta, (decimal?)creditoAplicado);
        }

        private async Task RecalcularTotalPagado(int idVenta, decimal? creditoAplicadoOverride)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"UPDATE Ventas
                        SET TotalPagado = (
                            SELECT ISNULL(SUM(Importe), 0)
                            FROM VentaPagos
                            WHERE IdVenta = @IdVenta
                            AND Estado = 'Activo'
                        )
                        WHERE Id = @IdVenta;";

            await connection.ExecuteAsync(sql, new { IdVenta = idVenta, CreditoAplicado = creditoAplicadoOverride });
        }

        public async Task CambiarEstado(int idPago, string estado)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"UPDATE VentaPagos SET Estado = @Estado WHERE Id = @Id";

            await connection.ExecuteAsync(sql, new
            {
                Id = idPago,
                Estado = estado
            });
        }
    }
}
