using Comercio.Domain.Entidades;
using Comercio.Domain.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Comercio.Infrastructure.Repositorios
{
    public class ClientesRepostorio : IClientesRepository
    {
        private readonly string _connectionString;

        public ClientesRepostorio(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<Cliente?> ObtenerPorId(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT Id, TipoCliente, Nombre, Apellido, RazonSocial,
                               NroDocumento, Cuit, NroTelefono, Email, UrlImagen,
                               FechaAlta, Direccion, Localidad, Provincia,
                               CodigoPostal, CondicionIva, Observaciones,
                               FechaBaja, Activo
                        FROM Clientes
                        WHERE Id = @Id";

            return await connection.QueryFirstOrDefaultAsync<Cliente>(sql, new { Id = id });
        }

        public async Task<IEnumerable<Cliente>> ObtenerTodos(bool incluirEliminados = false)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT Id, TipoCliente, Nombre, Apellido, RazonSocial,
                               NroDocumento, Cuit, NroTelefono, Email, UrlImagen,
                               FechaAlta, Direccion, Localidad, Provincia,
                               CodigoPostal, CondicionIva, Observaciones,
                               FechaBaja, Activo
                        FROM Clientes";

            if (!incluirEliminados)
                sql += " WHERE Activo = 1";

            return await connection.QueryAsync<Cliente>(sql);
        }

        public async Task<bool> ExisteRepetido(int tipoPersona, string dniCuit, int? idCliente = null)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT COUNT(1)
                        FROM Clientes
                        WHERE TipoCliente = @TipoCliente
                        AND (
                                (@TipoCliente = 1 AND NroDocumento = @DniCuit)
                             OR (@TipoCliente = 2 AND Cuit = @DniCuit)
                            )";

            if (idCliente.HasValue && idCliente.Value > 0)
                sql += " AND Id <> @IdCliente";

            var count = await connection.ExecuteScalarAsync<int>(
                sql,
                new
                {
                    TipoCliente = tipoPersona,
                    DniCuit = dniCuit,
                    IdCliente = idCliente
                });

            return count > 0;
        }

        public async Task<int> Crear(Cliente cliente)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"INSERT INTO Clientes
                        (TipoCliente, Nombre, Apellido, RazonSocial,NroDocumento, Cuit, NroTelefono, Email, UrlImagen,
                         FechaAlta, Direccion, Localidad, Provincia,CodigoPostal, CondicionIva, Observaciones,Activo)
                        VALUES
                        (@TipoCliente, @Nombre, @Apellido, @RazonSocial,@NroDocumento, @Cuit, @NroTelefono, @Email, @UrlImagen,
                         @FechaAlta, @Direccion, @Localidad, @Provincia,@CodigoPostal, @CondicionIva, @Observaciones,1);

                        SELECT CAST(SCOPE_IDENTITY() as int);";

            cliente.FechaAlta = DateTime.UtcNow;
            cliente.Activo = true;

            return await connection.ExecuteScalarAsync<int>(sql, cliente);
        }

        public async Task Actualizar(Cliente cliente)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"UPDATE Clientes
                        SET TipoCliente = @TipoCliente,
                            Nombre = @Nombre,
                            Apellido = @Apellido,
                            RazonSocial = @RazonSocial,
                            NroDocumento = @NroDocumento,
                            Cuit = @Cuit,
                            NroTelefono = @NroTelefono,
                            Email = @Email,
                            UrlImagen = @UrlImagen,
                            Direccion = @Direccion,
                            Localidad = @Localidad,
                            Provincia = @Provincia,
                            CodigoPostal = @CodigoPostal,
                            CondicionIva = @CondicionIva,
                            Observaciones = @Observaciones
                        WHERE Id = @Id";

            await connection.ExecuteAsync(sql, cliente);

        }

        public async Task<bool> DarDeBaja(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"UPDATE Clientes
                        SET Activo = 0,
                            FechaBaja = @FechaBaja
                        WHERE Id = @Id AND Activo = 1";

            var filas = await connection.ExecuteAsync(sql,
                new { Id = id, FechaBaja = DateTime.UtcNow });

            return filas > 0;
        }

        public async Task<bool> Restaurar(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"UPDATE Clientes
                        SET Activo = 1,
                            FechaBaja = NULL
                        WHERE Id = @Id AND Activo = 0";

            var filas = await connection.ExecuteAsync(sql, new { Id = id });

            return filas > 0;
        }

        public async Task EliminarPermanentemente(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"DELETE FROM Clientes WHERE Id = @Id";

            await connection.ExecuteAsync(sql, new { Id = id });
        }
    }
}
