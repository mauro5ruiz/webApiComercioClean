using Comercio.Domain.Entidades;
using Comercio.Domain.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Comercio.Infrastructure.Repositorios
{
    public class PerdidasRepositorio: IPerdidasRepository
    {
        private readonly string _connectionString;

        public PerdidasRepositorio(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<Perdida>> ObtenerPorFechas(DateTime desde, DateTime hasta)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT Id, Fecha, Motivo, Observacion, IdUsuario, IdEstado
                        FROM Perdidas
                        WHERE Fecha BETWEEN @Desde AND @Hasta
                        ORDER BY Fecha DESC;";

            return await connection.QueryAsync<Perdida>(sql, new { Desde = desde, Hasta = hasta });
        }

        public async Task<Perdida?> ObtenerPorId(int idPerdida)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT Id, Fecha, Motivo, Observacion, IdUsuario, IdEstado
                        FROM Perdidas
                        WHERE Id = @Id;";

            return await connection.QueryFirstOrDefaultAsync<Perdida>(sql, new { Id = idPerdida });
        }

        public async Task<int> Insertar(Perdida perdida)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"INSERT INTO Perdidas
                        (Fecha, Motivo, Observacion, IdUsuario, IdEstado)
                        VALUES
                        (@Fecha, @Motivo, @Observacion, @IdUsuario, @IdEstado);

                        SELECT CAST(SCOPE_IDENTITY() AS int);";

            return await connection.ExecuteScalarAsync<int>(sql, perdida);
        }

        public async Task Actualizar(int idPerdida, string motivo, string observacion)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"UPDATE Perdidas
                        SET Motivo = @Motivo,
                            Observacion = @Observacion
                        WHERE Id = @Id;";

            await connection.ExecuteAsync(sql, new
            {
                Id = idPerdida,
                Motivo = motivo,
                Observacion = observacion
            });
        }
    }
}
