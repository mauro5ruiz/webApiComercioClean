using Comercio.Domain.Entidades;

namespace Comercio.Domain.Interfaces
{
    public interface IProductosRepository
    {
        Task<IEnumerable<Producto>> ObtenerTodos(bool incluirEliminados = false);
        Task<Producto?> ObtenerPorId(int id);
        Task<bool> ExistePorCodigo(string? codigo, int? excluirId = null);
        Task<bool> ExistePorCodigoBarra(string? codigoBarra, int? excluirId = null);
        Task<int> Crear(Producto producto);
        Task<bool> Actualizar(Producto producto);
        Task<bool> DarDeBaja(int id);
        Task<bool> Restaurar(int id);
        Task<bool> EliminarPermanentemente(int id);
        
        // ===============================
        // 🔹 ACTUALIZACIÓN INDIVIDUAL
        // ===============================

        /// <summary>
        /// Actualiza el precio de venta de un producto con un valor directo.
        /// Ejemplo: el producto vale 100 y le envío 180 → queda en 180.
        /// </summary>
        Task<bool> ActualizarPrecio(int idProducto, decimal nuevoPrecio);


        /// <summary>
        /// Recalcula el precio de venta en base al costo y un margen.
        /// Fórmula: PrecioVenta = PrecioCompra * (1 + margen/100)
        /// Ejemplo: costo 100, margen 30% → precio = 130.
        /// </summary>
        Task<bool> ActualizarPrecioPorMargen(int idProducto, decimal porcentajeMargen);


        /// <summary>
        /// Aumenta el precio actual en un porcentaje.
        /// Fórmula: PrecioVenta = PrecioVenta * (1 + porcentaje/100)
        /// Ejemplo: precio actual 100, aumento 10% → queda en 110.
        /// </summary>
        Task<bool> AumentarPrecio(int idProducto, decimal porcentaje);


        // ===============================
        // 🔹 ACTUALIZACIÓN MASIVA
        // ===============================

        /// <summary>
        /// Recalcula el precio de TODOS los productos en base al costo y un margen.
        /// Ejemplo: aplicar margen 25% a todos los productos.
        /// </summary>
        Task<int> ActualizarPrecioTodosPorMargen(decimal porcentajeMargen);


        /// <summary>
        /// Aumenta el precio actual de TODOS los productos en un porcentaje.
        /// Ejemplo: subir 5% por inflación.
        /// </summary>
        Task<int> AumentarPrecioTodos(decimal porcentaje);


        /// <summary>
        /// Recalcula el precio por margen para una categoría específica.
        /// Ejemplo: todos los productos de categoría 3 con margen 40%.
        /// </summary>
        Task<int> ActualizarPrecioPorCategoria(int idCategoria, decimal porcentajeMargen);


        /// <summary>
        /// Aumenta el precio actual por porcentaje para una categoría específica.
        /// Ejemplo: subir 8% la categoría 3.
        /// </summary>
        Task<int> AumentarPrecioPorCategoria(int idCategoria, decimal porcentaje);


        /// <summary>
        /// Recalcula el precio por margen para una marca específica.
        /// Ejemplo: todos los productos de la marca 5 con margen 35%.
        /// </summary>
        Task<int> ActualizarPrecioPorMarca(int idMarca, decimal porcentajeMargen);


        /// <summary>
        /// Aumenta el precio actual por porcentaje para una marca específica.
        /// Ejemplo: subir 12% la marca 5.
        /// </summary>
        Task<int> AumentarPrecioPorMarca(int idMarca, decimal porcentaje);
    }
}
