using Comercio.Domain.Entidades;
using Comercio.Domain.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Comercio.Infrastructure.Repositorios
{
    public class OfertasRepositorio : IOfertasRepository
    {
        private readonly string _connectionString;

        public OfertasRepositorio(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<Oferta>> ObtenerTodas(bool incluirVencidas = false)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                SELECT Id, IdProducto, TipoDescuentoId as TipoDescuento, ValorDescuento,
                       FechaInicio, FechaFin, Activa
                FROM Ofertas";

            if (!incluirVencidas)
                sql += @" WHERE activa = 1";

            return await connection.QueryAsync<Oferta>(sql);
        }

        public async Task<Oferta?> ObtenerOfertaActivaVigente(int productoId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryFirstOrDefaultAsync<Oferta>(
                @"SELECT Id, IdProducto, TipoDescuentoId as TipoDescuento, ValorDescuento, FechaInicio, FechaFin, Activa
                  FROM Ofertas
                  WHERE IdProducto = @productoId
                  AND Activa = 1", 
                new { productoId }
            );
        }

        public async Task<Oferta?> ObtenerPorId(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryFirstOrDefaultAsync<Oferta>(
                @"SELECT Id, IdProducto, TipoDescuentoId as TipoDescuento, ValorDescuento, FechaInicio, FechaFin, Activa
                  FROM Ofertas
                  WHERE Id = @id",
                new { id }
            );
        }

        public async Task<IEnumerable<Oferta>> ObtenerPorProducto(int productoId, bool incluirVencidas = false)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"SELECT Id, IdProducto, TipoDescuentoId as TipoDescuento, ValorDescuento, FechaInicio, FechaFin, Activa
                           FROM Ofertas
                           WHERE IdProducto = @productoId";

            if (!incluirVencidas)
                sql += " AND FechaFin >= GETDATE()";

            return await connection.QueryAsync<Oferta>(sql, new { productoId });
        }

        public async Task<int> Crear(Oferta oferta)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.ExecuteScalarAsync<int>(
                @"INSERT INTO Ofertas(IdProducto, TipoDescuentoId, ValorDescuento,FechaInicio, FechaFin, Activa)
                  VALUES
                  (@IdProducto, @TipoDescuento, @ValorDescuento,@FechaInicio, @FechaFin, @Activa);

                  SELECT CAST(SCOPE_IDENTITY() as int);",
                oferta
            );
        }

        public async Task Actualizar(Oferta oferta)
        {
            using var connection = new SqlConnection(_connectionString);

            await connection.ExecuteAsync(
                @"UPDATE Ofertas
                  SET IdProducto = @IdProducto,
                      TipoDescuentoId = @TipoDescuentoId,
                      ValorDescuento = @ValorDescuento,
                      FechaInicio = @FechaInicio,
                      FechaFin = @FechaFin,
                      Activa = @Activa
                  WHERE Id = @Id",
                oferta
            );
        }

        public async Task DesactivarOfertasPorProducto(int productoId)
        {
            using var connection = new SqlConnection(_connectionString);

            await connection.ExecuteAsync(
                @"UPDATE Ofertas
                  SET Activa = 0
                  WHERE IdProducto = @productoId
                  AND Activa = 1",
                new { productoId }
            );
        }
    }
}
