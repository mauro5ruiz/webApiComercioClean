using Comercio.Domain.Entidades;
using Comercio.Domain.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Comercio.Infrastructure.Repositorios
{
    public class FormasDePagoRepositorio : IFormasDePagoRepository
    {
        private readonly string _connectionString;

        public FormasDePagoRepositorio(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<FormaDePago>> ObtenerTodas()
        {
            using var connection = new SqlConnection(_connectionString);

            var data = await connection.QueryAsync<FormaDePago>(
                @"SELECT f.Id, f.Nombre, COUNT(DISTINCT vp.IdVenta) AS CantidadVentas, COUNT(DISTINCT cp.IdCompra) AS CantidadCompras
                   FROM FormasDePago f
                   LEFT JOIN VentaPagos vp ON vp.IdFormaPago = f.Id
                   LEFT JOIN CompraPagos cp ON cp.IdFormaPago = f.Id
                   GROUP BY f.Id, f.Nombre
                   ORDER BY f.Id"
            );

            return data;
        }

        public async Task<FormaDePago?> ObtenerPorId(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryFirstOrDefaultAsync<FormaDePago>(
                "SELECT f.Id, f.Nombre, COUNT(DISTINCT vp.IdVenta) AS CantidadVentas, COUNT(DISTINCT cp.IdCompra) AS CantidadCompras " +
                "FROM FormasDePago f " +
                "LEFT JOIN VentaPagos vp ON vp.IdFormaPago = f.Id " +
                "LEFT JOIN CompraPagos cp ON cp.IdFormaPago = f.Id " +
                "WHERE f.Id = @id " +
                "GROUP BY f.Id, f.Nombre " +
                "ORDER BY f.Id",
                new { id }
            );
        }

        public async Task<FormaDePago?> ObtenerPorNombre(string nombre)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryFirstOrDefaultAsync<FormaDePago>(
                "SELECT Id, Nombre FROM FormasDePago WHERE Nombre = @nombre",
                new { nombre }
            );
        }

        public async Task<int> Crear(FormaDePago formaDePago)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.ExecuteScalarAsync<int>(
                @"INSERT INTO FormasDePago (Nombre)
              VALUES (@Nombre);
              SELECT CAST(SCOPE_IDENTITY() as int);",
                formaDePago
            );
        }

        public async Task Actualizar(FormaDePago formaDePago)
        {
            using var connection = new SqlConnection(_connectionString);

            await connection.ExecuteAsync(
                "UPDATE FormasDePago SET Nombre = @Nombre WHERE Id = @Id",
                formaDePago
            );
        }

        public async Task Eliminar(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            await connection.ExecuteAsync(
                "DELETE FROM FormasDePago WHERE Id = @id",
                new { id }
            );
        }
    }
}
