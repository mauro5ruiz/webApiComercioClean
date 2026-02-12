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

            return await connection.QueryAsync<FormaDePago>(
                "SELECT Id, Nombre FROM FormasDePago"
            );
        }

        public async Task<FormaDePago?> ObtenerPorId(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryFirstOrDefaultAsync<FormaDePago>(
                "SELECT Id, Nombre FROM FormasDePago WHERE Id = @id",
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
