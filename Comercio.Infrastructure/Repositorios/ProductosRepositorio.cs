using Comercio.Domain.Entidades;
using Comercio.Domain.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Comercio.Infrastructure.Repositorios
{
    public class ProductosRepositorio : IProductosRepository
    {
        private readonly string _connectionString;

        public ProductosRepositorio(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<Producto>> ObtenerTodos(bool incluirEliminados = false)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT Id, Nombre, Descripcion, Codigo, CodigoBarra,
                               IdCategoria, IdMarca,
                               PrecioCompra, PrecioVenta,
                               StockMinimo, ControlStock, StockActual,
                               UrlImagen, Activo, FechaAlta, FechaBaja
                        FROM Productos";

            if (!incluirEliminados)
                sql += " WHERE Activo = 1";

            return await connection.QueryAsync<Producto>(sql);
        }

        public async Task<Producto?> ObtenerPorId(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT Id, Nombre, Descripcion, Codigo, CodigoBarra, IdCategoria, IdMarca, PrecioCompra, PrecioVenta,
                               StockMinimo, ControlStock, StockActual, UrlImagen, Activo, FechaAlta, FechaBaja
                        FROM Productos
                        WHERE Id = @Id";

            return await connection.QueryFirstOrDefaultAsync<Producto>(sql, new { Id = id });
        }

        public async Task<bool> ExistePorCodigo(string? codigo, int? excluirId = null)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return false;

            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT COUNT(1)
                        FROM Productos
                        WHERE Codigo = @Codigo";

            if (excluirId.HasValue && excluirId.Value > 0)
                sql += " AND Id <> @ExcluirId";

            var count = await connection.ExecuteScalarAsync<int>(sql, new { Codigo = codigo, ExcluirId = excluirId } );

            return count > 0;
        }

        public async Task<bool> ExistePorCodigoBarra(string? codigoBarra, int? excluirId = null)
        {
            if (string.IsNullOrWhiteSpace(codigoBarra))
                return false;

            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT COUNT(1)
                        FROM Productos
                        WHERE CodigoBarra = @CodigoBarra";

            if (excluirId.HasValue && excluirId.Value > 0)
                sql += " AND Id <> @ExcluirId";

            var count = await connection.ExecuteScalarAsync<int>(sql, new { CodigoBarra = codigoBarra, ExcluirId = excluirId } );

            return count > 0;
        }

        public async Task<int> Crear(Producto producto)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"INSERT INTO Productos
                        (Nombre, Descripcion, Codigo, CodigoBarra, IdCategoria, IdMarca, PrecioCompra, PrecioVenta,
                         StockMinimo, ControlStock, StockActual, UrlImagen, Activo, FechaAlta)
                        VALUES
                        (@Nombre, @Descripcion, @Codigo, @CodigoBarra, @IdCategoria, @IdMarca, @PrecioCompra, 
                         @PrecioVenta, @StockMinimo, @ControlStock, @StockActual, @UrlImagen, 1, @FechaAlta);

                        SELECT CAST(SCOPE_IDENTITY() as int);";

            producto.FechaAlta = DateTime.UtcNow;
            producto.Activo = true;

            return await connection.ExecuteScalarAsync<int>(sql, producto);
        }

        public async Task<bool> DarDeBaja(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"UPDATE Productos
                        SET Activo = 0,
                            FechaBaja = @FechaBaja
                        WHERE Id = @Id AND Activo = 1";

            var filas = await connection.ExecuteAsync(sql, new { Id = id, FechaBaja = DateTime.UtcNow });

            return filas > 0;
        }

        public async Task<bool> Restaurar(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"UPDATE Productos
                        SET Activo = 1,
                            FechaBaja = NULL
                        WHERE Id = @Id AND Activo = 0";

            var filas = await connection.ExecuteAsync(sql, new { Id = id });

            return filas > 0;
        }

        public async Task<bool> EliminarPermanentemente(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"DELETE FROM Productos
                        WHERE Id = @Id";

            var filas = await connection.ExecuteAsync(sql, new { Id = id });

            return filas > 0;
        }

        public async Task<bool> Actualizar(Producto producto)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"UPDATE Productos
                        SET Nombre = @Nombre,
                            Descripcion = @Descripcion,
                            Codigo = @Codigo,
                            CodigoBarra = @CodigoBarra,
                            IdCategoria = @IdCategoria,
                            IdMarca = @IdMarca,
                            PrecioCompra = @PrecioCompra,
                            PrecioVenta = @PrecioVenta,
                            StockMinimo = @StockMinimo,
                            ControlStock = @ControlStock,
                            StockActual = @StockActual,
                            UrlImagen = @UrlImagen
                        WHERE Id = @Id";

            var filas = await connection.ExecuteAsync(sql, producto);

            return filas > 0;
        }

        public async Task<bool> ActualizarPrecio(int idProducto, decimal nuevoPrecio)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> ActualizarPrecioPorMargen(int idProducto, decimal porcentajeMargen)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> AumentarPrecio(int idProducto, decimal porcentaje)
        {
            throw new NotImplementedException();
        }

        public async Task<int> ActualizarPrecioTodosPorMargen(decimal porcentajeMargen)
        {
            throw new NotImplementedException();
        }

        public async Task<int> AumentarPrecioTodos(decimal porcentaje)
        {
            throw new NotImplementedException();
        }

        public async Task<int> ActualizarPrecioPorCategoria(int idCategoria, decimal porcentajeMargen)
        {
            throw new NotImplementedException();
        }

        public async Task<int> AumentarPrecioPorCategoria(int idCategoria, decimal porcentaje)
        {
            throw new NotImplementedException();
        }

        public async Task<int> AumentarPrecioPorMarca(int idMarca, decimal porcentaje)
        {
            throw new NotImplementedException();
        }

        public async Task<int> ActualizarPrecioPorMarca(int idMarca, decimal porcentajeMargen)
        {
            throw new NotImplementedException();
        }
    }
}
