using Comercio.Domain.Entidades;
using Comercio.Domain.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Comercio.Infrastructure.Repositorios
{
    public class ComprasRepositorio: IComprasRepostory
    {
        private readonly string _connectionString;

        public ComprasRepositorio(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<Compra>> ObtenerPorFechas(DateTime desde, DateTime hasta)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT Id, NumeroComprobante, Fecha, IdProveedor, IdSucursal, Total, TotalPagado, SaldoPendiente, Estado, Observaciones, FechaAnulacion
                        FROM Compras
                        WHERE Fecha BETWEEN @Desde AND @Hasta
                        ORDER BY Fecha DESC;";

            return await connection.QueryAsync<Compra>(sql, new { Desde = desde, Hasta = hasta });
        }

        public async Task<Compra?> ObtenerPorId(int idCompra)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT Id, NumeroComprobante, Fecha, IdProveedor, IdSucursal, Total, TotalPagado, SaldoPendiente, Estado, Observaciones, FechaAnulacion
                        FROM Compras
                        WHERE Id = @Id;";

            return await connection.QueryFirstOrDefaultAsync<Compra>(sql, new { Id = idCompra });
        }

        public async Task<Compra?> ObtenerPorNroComprobante(string nroComprobante)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT Id, NumeroComprobante, Fecha, IdProveedor, IdSucursal, Total, TotalPagado, SaldoPendiente, Estado, Observaciones, FechaAnulacion
                        FROM Compras
                        WHERE NumeroComprobante = @NumeroComprobante;";

            return await connection.QueryFirstOrDefaultAsync<Compra>(sql, new { NumeroComprobante = nroComprobante });
        }

        public async Task<bool> Existe(int idCompra)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT COUNT(1) FROM Compras WHERE Id = @Id;";

            var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = idCompra });

            return count > 0;
        }

        public async Task<int> Insertar(Compra compra)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"INSERT INTO Compras
                        (NumeroComprobante, Fecha, IdProveedor, IdSucursal, Total, TotalPagado, SaldoPendiente, Estado, Observaciones)
                        VALUES
                        (@NumeroComprobante, @Fecha, @IdProveedor, @IdSucursal, @Total, @TotalPagado, @SaldoPendiente, @Estado, @Observaciones);

                        SELECT CAST(SCOPE_IDENTITY() as int);";

            return await connection.ExecuteScalarAsync<int>(sql, compra);
        }

        public async Task CambiarEstado(int idCompra, int estado)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"UPDATE Compras
                        SET Estado = @Estado,
                            FechaAnulacion = CASE 
                                                WHEN @Estado = 2 
                                                THEN @Fecha 
                                                ELSE NULL 
                                             END
                        WHERE Id = @Id;";

            await connection.ExecuteAsync(sql, new
            {
                Id = idCompra,
                Estado = estado,
                Fecha = DateTime.UtcNow
            });
        }

        public async Task ActualizarTotales(int idCompra, decimal total, decimal totalPagado, decimal saldoPendiente)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"UPDATE Compras
                        SET Total = @Total,
                            TotalPagado = @TotalPagado,
                            SaldoPendiente = @SaldoPendiente
                        WHERE Id = @Id;";

            await connection.ExecuteAsync(sql, new
            {
                Id = idCompra,
                Total = total,
                TotalPagado = totalPagado,
                SaldoPendiente = saldoPendiente
            });
        }

        public async Task<IEnumerable<Compra>> ObtenerPendientesPorProveedor(int idProveedor)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT Id, NumeroComprobante, Fecha, IdProveedor, IdSucursal, Total, TotalPagado, SaldoPendiente, Estado
                FROM Compras
                WHERE IdProveedor = @IdProveedor
                AND SaldoPendiente > 0
                AND Estado = 1
                ORDER BY Fecha ASC";

            return await connection.QueryAsync<Compra>(sql, new { IdProveedor = idProveedor });
        }
    }
}
