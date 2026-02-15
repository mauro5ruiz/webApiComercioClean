using Comercio.Domain.Entidades;
using Comercio.Domain.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Comercio.Infrastructure.Repositorios
{
    public class ProveedoresRepositorio : IProveedoresRepostory
    {
        private readonly string _connectionString;

        public ProveedoresRepositorio(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<Proveedor?> ObtenerPorId(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT Id, RazonSocial, Cuit, CondicionIva, Telefono, Email, PersonaContacto, 
                               Direccion, Localidad, Provincia, CodigoPostal, Observaciones, 
                               Activo, UrlImagen, FechaCreacion, FechaBaja
                        FROM Proveedores
                        WHERE Id = @Id";

            return await connection.QueryFirstOrDefaultAsync<Proveedor>(sql, new { Id = id });
        }

        public async Task<IEnumerable<Proveedor>> ObtenerTodos(bool incluirEliminados = false)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT Id, RazonSocial, Cuit, CondicionIva, Telefono, Email, PersonaContacto, 
                               Direccion, Localidad, Provincia, CodigoPostal, Observaciones, 
                               Activo, UrlImagen, FechaCreacion, FechaBaja
                        FROM Proveedores";

            if (!incluirEliminados)
                sql += " WHERE Activo = 1";

            return await connection.QueryAsync<Proveedor>(sql);
        }

        public async Task<bool> ExistePorCuit(string cuit, int? idProveedor = null)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT COUNT(1)
                FROM Proveedores
                WHERE Cuit = @Cuit";

            if (idProveedor.HasValue && idProveedor.Value > 0)
                sql += " AND Id <> @IdProveedor";

            var count = await connection.ExecuteScalarAsync<int>(
                sql,
                new { Cuit = cuit, IdProveedor = idProveedor }
            );

            return count > 0;
        }


        public async Task<int> Crear(Proveedor proveedor)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"INSERT INTO Proveedores
                        (RazonSocial, Cuit, CondicionIva, Telefono, Email, PersonaContacto,
                         Direccion, Localidad, Provincia, CodigoPostal, Observaciones,
                         Activo, UrlImagen, FechaCreacion)
                        VALUES
                        (@RazonSocial, @Cuit, @CondicionIva, @Telefono, @Email, @PersonaContacto,
                         @Direccion, @Localidad, @Provincia, @CodigoPostal, @Observaciones,
                         1, @UrlImagen, @FechaCreacion);

                        SELECT CAST(SCOPE_IDENTITY() as int);";

            proveedor.FechaCreacion = DateTime.UtcNow;
            proveedor.Activo = true;

            return await connection.ExecuteScalarAsync<int>(sql, proveedor);
        }

        public async Task Actualizar(Proveedor proveedor)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"UPDATE Proveedores
                        SET RazonSocial = @RazonSocial,
                            Cuit = @Cuit,
                            CondicionIva = @CondicionIva,
                            Telefono = @Telefono,
                            Email = @Email,
                            PersonaContacto = @PersonaContacto,
                            Direccion = @Direccion,
                            Localidad = @Localidad,
                            Provincia = @Provincia,
                            CodigoPostal = @CodigoPostal,
                            Observaciones = @Observaciones,
                            UrlImagen = @UrlImagen
                        WHERE Id = @Id";

            await connection.ExecuteAsync(sql, proveedor);
        }

        public async Task<bool> DarDeBaja(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"UPDATE Proveedores
                        SET Activo = 0,
                            FechaBaja = @FechaBaja
                        WHERE Id = @Id AND Activo = 1";

            var filasAfectadas = await connection.ExecuteAsync(sql,
                new { Id = id, FechaBaja = DateTime.UtcNow });

            return filasAfectadas > 0;
        }

        public async Task<bool> Restaurar(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"UPDATE Proveedores
                        SET Activo = 1,
                            FechaBaja = NULL
                        WHERE Id = @Id AND Activo = 0";

            var filasAfectadas = await connection.ExecuteAsync(sql, new { Id = id });

            return filasAfectadas > 0;
        }

        public async Task EliminarPermanentemente(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"DELETE FROM Proveedores
                        WHERE Id = @Id";

            await connection.ExecuteAsync(sql, new { Id = id });
        }
    }
}
