using Comercio.Domain.Entidades;
using Comercio.Domain.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Comercio.Infrastructure.Repositorios
{
    public class MarcasRepositorio: IMarcasRepository
    {
        private readonly string _connectionString;

        public MarcasRepositorio(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<Marca?> ObtenerPorId(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryFirstOrDefaultAsync<Marca>(
                "SELECT Id, Nombre, UrlImagen as Imagen, Activa FROM Marcas WHERE Id = @id",
                new { id }
            );
        }

        public async Task<Marca?> ObtenerPorNombre(string nombre)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryFirstOrDefaultAsync<Marca>(
                "SELECT Id, Nombre, UrlImagen as Imagen, Activa FROM Marcas WHERE Nombre = @nombre",
                new { nombre }
            );
        }

        public async Task<IEnumerable<Marca>> ObtenerTodas()
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryAsync<Marca>(
                "SELECT Id, Nombre, UrlImagen as Imagen, Activa FROM Marcas"
            );
        }

        public async Task<int> Crear(Marca marca)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.ExecuteScalarAsync<int>(
                @"INSERT INTO Marcas (Nombre, UrlImagen, Activa)
                VALUES (@Nombre, @Imagen, @Activa);
                SELECT CAST(SCOPE_IDENTITY() as int);",
                  marca
            );
        }

        public async Task Actualizar(Marca marca)
        {
            using var connection = new SqlConnection(_connectionString);

            await connection.ExecuteAsync(
                "UPDATE Marcas SET Nombre = @Nombre, UrlImagen = @Imagen, Activa = @Activa WHERE Id = @Id",
                marca
            );
        }

        public async Task Eliminar(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            await connection.ExecuteAsync(
                "DELETE FROM Marcas WHERE Id = @id",
                new { id }
            );
        }
    }
}
