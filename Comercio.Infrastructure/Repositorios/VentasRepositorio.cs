using Comercio.Domain.Entidades;
using Comercio.Domain.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Comercio.Infrastructure.Repositorios
{
    public class VentasRepositorio : IVentasRepository
    {
        private readonly string _connectionString;

        public VentasRepositorio(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<Venta>> ObtenerPorFechas(DateTime desde, DateTime hasta)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT Id, NumeroComprobante, Fecha, IdCliente, IdVendedor, IdSucursal, Total, TotalPagado, 
                               SaldoPendiente, Estado, Observaciones, FechaAnulacion
                        FROM Ventas
                        WHERE Fecha BETWEEN @Desde AND @Hasta
                        ORDER BY Fecha DESC";

            return await connection.QueryAsync<Venta>(sql, new { Desde = desde, Hasta = hasta });
        }

        public async Task<Venta?> ObtenerPorId(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT Id, NumeroComprobante, Fecha, IdCliente, IdVendedor, IdSucursal, Total, TotalPagado, 
                               SaldoPendiente, Estado, Observaciones, FechaAnulacion
                        FROM Ventas
                        WHERE Id = @Id";

            return await connection.QueryFirstOrDefaultAsync<Venta>(sql, new { Id = id });
        }

        public async Task<Venta?> ObtenerPorNroComprobante(string nroComprobante)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT Id, NumeroComprobante, Fecha, IdCliente, IdVendedor, IdSucursal, Total, TotalPagado, 
                               SaldoPendiente, Estado, Observaciones, FechaAnulacion
                        FROM Ventas
                        WHERE NumeroComprobante = @NumeroComprobante";

            return await connection.QueryFirstOrDefaultAsync<Venta>(sql, new { NumeroComprobante = nroComprobante });
        }

        public async Task<IEnumerable<Venta>> ObtenerPendientes()
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT Id, NumeroComprobante, Fecha, IdCliente, IdVendedor, IdSucursal, Total, TotalPagado, 
                               SaldoPendiente, Estado, Observaciones, FechaAnulacion
                        FROM Ventas
                        WHERE SaldoPendiente > 0
                        AND Estado = 'Activa'
                        ORDER BY Fecha DESC";

            return await connection.QueryAsync<Venta>(sql);
        }

        public async Task<IEnumerable<Venta>> ObtenerPorCliente(int idCliente, bool soloPendientes = false)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT Id, NumeroComprobante, Fecha, IdCliente, IdVendedor,
                       IdSucursal, Total, TotalPagado, SaldoPendiente,
                       Estado, Observaciones, FechaAnulacion
                FROM Ventas
                WHERE IdCliente = @IdCliente";

            if (soloPendientes)
                sql += " AND SaldoPendiente > 0 AND Estado = 'Activa'";

            sql += " ORDER BY Fecha ASC";

            return await connection.QueryAsync<Venta>(sql, new { IdCliente = idCliente });
        }

        public async Task<bool> Existe(int idVenta)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT COUNT(1) FROM Ventas WHERE Id = @Id";

            var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = idVenta });

            return count > 0;
        }

        public async Task<int> Insertar(Venta venta)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"INSERT INTO Ventas (NumeroComprobante,Fecha,IdCliente,IdVendedor,IdSucursal,Total,TotalPagado,Estado,Observaciones)
                        VALUES
                        (@NumeroComprobante,@Fecha,@IdCliente,@IdVendedor,@IdSucursal,@Total,@TotalPagado,@Estado,@Observaciones);

                        SELECT CAST(SCOPE_IDENTITY() as int);";

            venta.Fecha = DateTime.Now;
            venta.TotalPagado = venta.TotalPagado;
            venta.Estado = string.IsNullOrEmpty(venta.Estado) ? "Activa" : venta.Estado;

            return await connection.ExecuteScalarAsync<int>(sql, venta);
        }

        public async Task Actualizar(Venta venta)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"UPDATE Ventas
                        SET NumeroComprobante = @NumeroComprobante,
                            IdCliente = @IdCliente,
                            IdVendedor = @IdVendedor,
                            IdSucursal = @IdSucursal,
                            Total = @Total,
                            TotalPagado = @TotalPagado,
                            Estado = @Estado,
                            Observaciones = @Observaciones
                        WHERE Id = @Id";

            await connection.ExecuteAsync(sql, venta);
        }

        public async Task CambiarEstado(int idVenta, string estado)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"UPDATE Ventas
                        SET Estado = @Estado,
                            FechaAnulacion = CASE 
                                                WHEN @Estado = 'Anulada' 
                                                THEN @Fecha 
                                                ELSE NULL 
                                             END
                        WHERE Id = @Id";

            await connection.ExecuteAsync(sql, new
            {
                Id = idVenta,
                Estado = estado,
                Fecha = DateTime.UtcNow
            });
        }
    }
}
