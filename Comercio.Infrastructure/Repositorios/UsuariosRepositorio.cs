using Comercio.Domain.Entidades;
using Comercio.Domain.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Comercio.Infrastructure.Repositorios
{
    public class UsuariosRepositorio : IUsuariosRepository
    {
        private readonly string _connectionString;

        public UsuariosRepositorio(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<Usuario>> ObtenerTodos(bool incluirEliminados = false)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT Id, Nombre, Apellido, Email, 
                         Usuario AS UsuarioLogin,
                         ClaveHash, RolId, SucursalId, 
                         Telefono, Activo, DebeCambiarClave, 
                         FechaCreacion, UltimoAcceso
                  FROM Usuarios";

            if (!incluirEliminados)
                sql += " WHERE Activo = 1";

            return await connection.QueryAsync<Usuario>(sql);
        }

        public async Task<Usuario?> ObtenerPorId(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryFirstOrDefaultAsync<Usuario>(
                @"SELECT Id, Nombre, Apellido, Email, 
                         Usuario AS UsuarioLogin,
                         ClaveHash, RolId, SucursalId, 
                         Telefono, Activo, DebeCambiarClave, 
                         FechaCreacion, UltimoAcceso
                  FROM Usuarios
                  WHERE Id = @id",
                new { id }
            );
        }

        public async Task<IEnumerable<Usuario>> ObtenerPorSucursal(int sucursalId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryAsync<Usuario>(
                @"SELECT Id, Nombre, Apellido, Email, 
                         Usuario AS UsuarioLogin,
                         ClaveHash, RolId, SucursalId, 
                         Telefono, Activo, DebeCambiarClave, 
                         FechaCreacion, UltimoAcceso
                  FROM Usuarios
                  WHERE SucursalId = @sucursalId",
                new { sucursalId }
            );
        }

        public async Task<Usuario?> ObtenerPorUsuario(string usuarioLogin)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryFirstOrDefaultAsync<Usuario>(
                @"SELECT Id, Nombre, Apellido, Email, 
                         Usuario AS UsuarioLogin,
                         ClaveHash, RolId, SucursalId, 
                         Telefono, Activo, DebeCambiarClave, 
                         FechaCreacion, UltimoAcceso
                  FROM Usuarios
                  WHERE Usuario = @usuarioLogin",
                new { usuarioLogin }
            );
        }

        public async Task<bool> ExisteEmail(string email)
        {
            using var connection = new SqlConnection(_connectionString);

            var existe = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM Usuarios WHERE Email = @email",
                new { email }
            );

            return existe > 0;
        }

        public async Task<bool> ExisteUsuario(string usuarioLogin)
        {
            using var connection = new SqlConnection(_connectionString);

            var existe = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM Usuarios WHERE Usuario = @usuarioLogin",
                new { usuarioLogin }
            );

            return existe > 0;
        }

        public async Task<int> Crear(Usuario usuario)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.ExecuteScalarAsync<int>(
                @"INSERT INTO Usuarios (Nombre, Apellido, Email, Usuario, ClaveHash, RolId, SucursalId, Telefono,
                                        Activo, DebeCambiarClave, FechaCreacion)
                  VALUES (@Nombre, @Apellido, @Email, @UsuarioLogin, @ClaveHash, @RolId, @SucursalId, @Telefono, @Activo,
                          @DebeCambiarClave, @FechaCreacion);

                  SELECT CAST(SCOPE_IDENTITY() as int);",
                usuario
            );
        }

        public async Task<bool> Actualizar(Usuario usuario)
        {
            using var connection = new SqlConnection(_connectionString);

            var filas = await connection.ExecuteAsync(
                @"UPDATE Usuarios
                  SET Nombre = @Nombre,
                      Apellido = @Apellido,
                      Email = @Email,
                      RolId = @RolId,
                      SucursalId = @SucursalId,
                      Telefono = @Telefono,
                      Activo = @Activo
                  WHERE Id = @Id",
                usuario
            );

            return filas > 0;
        }

        public async Task<bool> ActualizarUltimoAcceso(int usuarioId)
        {
            using var connection = new SqlConnection(_connectionString);

            var filas = await connection.ExecuteAsync(
                @"UPDATE Usuarios
                  SET UltimoAcceso = GETDATE()
                  WHERE Id = @usuarioId",
                new { usuarioId }
            );

            return filas > 0;
        }

        public async Task<bool> CambiarClave(int usuarioId, string nuevoHash)
        {
            using var connection = new SqlConnection(_connectionString);

            var filas = await connection.ExecuteAsync(
                @"UPDATE Usuarios
                  SET ClaveHash = @nuevoHash,
                      DebeCambiarClave = 0
                  WHERE Id = @usuarioId",
                new { usuarioId, nuevoHash }
            );

            return filas > 0;
        }

        public async Task<bool> Activar(int usuarioId)
        {
            using var connection = new SqlConnection(_connectionString);

            var filas = await connection.ExecuteAsync(
                "UPDATE Usuarios SET Activo = 1 WHERE Id = @usuarioId",
                new { usuarioId }
            );

            return filas > 0;
        }

        public async Task<bool> Desactivar(int usuarioId)
        {
            using var connection = new SqlConnection(_connectionString);

            var filas = await connection.ExecuteAsync(
                "UPDATE Usuarios SET Activo = 0 WHERE Id = @usuarioId",
                new { usuarioId }
            );

            return filas > 0;
        }
    }
}
