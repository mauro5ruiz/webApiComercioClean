using Comercio.Domain.Entidades;
using Comercio.Domain.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Comercio.Infrastructure.Repositorios
{
    public class DevolucionesVentasRepositorio: IDevolucionesVentasRepository
    {
        private readonly string _connectionString;

        public DevolucionesVentasRepositorio(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<DevolucionVenta>> ObtenerPorFechas(DateTime desde, DateTime hasta)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT Id, IdVenta, NumeroComprobante, Fecha, IdCliente, IdSucursal,
                               Total, Observaciones, Estado
                        FROM DevolucionesVenta
                        WHERE Fecha BETWEEN @Desde AND @Hasta
                        ORDER BY Fecha DESC";

            return await connection.QueryAsync<DevolucionVenta>(sql, new { Desde = desde, Hasta = hasta });
        }

        public async Task<DevolucionVenta?> ObtenerPorId(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT Id, IdVenta, NumeroComprobante, Fecha, IdCliente, IdSucursal,
                               Total, Observaciones, Estado
                        FROM DevolucionesVenta
                        WHERE Id = @Id";

            return await connection.QueryFirstOrDefaultAsync<DevolucionVenta>(sql, new { Id = id });
        }

        public async Task<IEnumerable<DevolucionVenta>> ObtenerPorVenta(int idVenta)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT Id, IdVenta, NumeroComprobante, Fecha, IdCliente, IdSucursal,
                               Total, Observaciones, Estado
                        FROM DevolucionesVenta
                        WHERE IdVenta = @IdVenta
                        ORDER BY Fecha DESC";

            return await connection.QueryAsync<DevolucionVenta>(sql, new { IdVenta = idVenta });
        }

        public async Task<int> Insertar(DevolucionVenta devolucion)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"INSERT INTO DevolucionesVenta
                        (IdVenta, NumeroComprobante, Fecha, IdCliente, IdSucursal, Total, Observaciones, Estado)
                        VALUES
                        (@IdVenta, @NumeroComprobante, @Fecha, @IdCliente, @IdSucursal, @Total, @Observaciones, @Estado);

                        SELECT CAST(SCOPE_IDENTITY() as int);";

            devolucion.Fecha = DateTime.Now;
            devolucion.Estado = string.IsNullOrEmpty(devolucion.Estado) ? "Activa" : devolucion.Estado;

            return await connection.ExecuteScalarAsync<int>(sql, devolucion);
        }

        public async Task CambiarEstado(int idDevolucion, string estado)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"UPDATE DevolucionesVenta
                        SET Estado = @Estado
                        WHERE Id = @Id";

            await connection.ExecuteAsync(sql, new
            {
                Id = idDevolucion,
                Estado = estado
            });
        }
    }
}
