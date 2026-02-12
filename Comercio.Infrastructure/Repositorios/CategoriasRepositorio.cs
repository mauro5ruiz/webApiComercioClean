using Comercio.Domain.Entidades;
using Comercio.Domain.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;


namespace Comercio.Infrastructure.Repositorios
{
    public class CategoriasRepositorio : ICategoriasRepository
    {
        private readonly string _connectionString;

        public CategoriasRepositorio(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<Categoria>> ObtenerTodas()
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryAsync<Categoria>(
                "SELECT Id, Nombre FROM Categorias"
            );
        }

        public async Task<Categoria?> ObtenerPorId(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryFirstOrDefaultAsync<Categoria>(
                "SELECT Id, Nombre FROM Categorias WHERE Id = @id",
                new { id }
            );
        }

        public async Task<Categoria?> ObtenerPorNombre(string nombre)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryFirstOrDefaultAsync<Categoria>(
                "SELECT Id, Nombre FROM Categorias WHERE Nombre = @nombre",
                new { nombre }
            );
        }

        public async Task<int> Crear(Categoria categoria)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.ExecuteScalarAsync<int>(
                @"INSERT INTO Categorias (Nombre)
              VALUES (@Nombre);
              SELECT CAST(SCOPE_IDENTITY() as int);",
                categoria
            );
        }

        public async Task Actualizar(Categoria categoria)
        {
            using var connection = new SqlConnection(_connectionString);

            await connection.ExecuteAsync(
                "UPDATE Categorias SET Nombre = @Nombre WHERE Id = @Id",
                categoria
            );
        }

        public async Task Eliminar(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            await connection.ExecuteAsync(
                "DELETE FROM Categorias WHERE Id = @id",
                new { id }
            );
        }
    }
}
