using Comercio.Domain.Entidades;
using Comercio.Domain.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Comercio.Infrastructure.Repositorios
{
    public class SucursalesRepositorio : ISucursalesRepository
    {
        private readonly string _connectionString;

        public SucursalesRepositorio(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<Sucursal>> ObtenerTodas()
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryAsync<Sucursal>(
                "SELECT Id, Nombre, Direccion, Localidad, NroTelefono, Email, Codigo, Activa FROM Sucursales"
            );
        }

        public async Task<Sucursal?> ObtenerPorId(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryFirstOrDefaultAsync<Sucursal>(
                "SELECT Id, Nombre, Direccion, Localidad, NroTelefono, Email, Codigo, Activa FROM Sucursales WHERE Id = @id",
                new { id }
            );
        }

        public async Task<Sucursal?> ObtenerPorNombre(string nombre)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryFirstOrDefaultAsync<Sucursal>(
                "SELECT Id, Nombre, Direccion, Localidad, NroTelefono, Email, Codigo, Activa FROM Sucursales WHERE Nombre = @nombre",
                new { nombre }
            );
        }

        public async Task<bool> ExistePorCodigo(string codigo)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.ExecuteScalarAsync<bool>(
                @"SELECT CASE 
                    WHEN EXISTS (SELECT 1 FROM Sucursales WHERE Codigo = @codigo) 
                    THEN CAST(1 AS BIT) 
                    ELSE CAST(0 AS BIT) 
                END",
                new { codigo }
            );
        }

        public async Task<int> Crear(Sucursal sucursal)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.ExecuteScalarAsync<int>(
                @"INSERT INTO Sucursales (Nombre, Direccion, Localidad, NroTelefono, Email, Codigo, Activa)
                VALUES (@Nombre, @Direccion, @Localidad, @NroTelefono, @Email, @Codigo, @Activa);
                SELECT CAST(SCOPE_IDENTITY() as int);",
                  sucursal
            );
        }

        public async Task Actualizar(Sucursal sucursal)
        {
            using var connection = new SqlConnection(_connectionString);

            await connection.ExecuteAsync(
                "UPDATE Sucursales SET Nombre = @Nombre, Direccion = @Direccion, Localidad = @Localidad, NroTelefono = @NroTelefono, Email = @Email, Codigo = @Codigo, Activa = @Activa WHERE Id = @Id",
                sucursal
            );
        }

        public async Task Eliminar(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            await connection.ExecuteAsync(
                "DELETE FROM Sucursales WHERE Id = @id",
                new { id }
            );
        }

        public async Task<bool> DarDeBaja(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            var rowsAffected = await connection.ExecuteAsync(
                "UPDATE Sucursales SET Activa = 0 WHERE Id = @Id",
                new { Id = id }
            );

            return rowsAffected > 0;
        }


        public async Task<bool> Restaurar(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            var rowsAffected = await connection.ExecuteAsync(
                "UPDATE Sucursales SET Activa = 1 WHERE Id = @Id",
                new { Id = id }
            );

            return rowsAffected > 0;
        }
    }
}
