using AutoMapper;
using Comercio.Application.Dtos.Productos;
using Comercio.Application.Interfaces;
using Comercio.Domain.Entidades;
using Comercio.Domain.Interfaces;

namespace Comercio.Application.Servicios
{
    public class ProductosServicio : IProductosServicio
    {
        private readonly IProductosRepository _productosRepository;
        private readonly IMarcasRepository _marcasRepository;
        private readonly ICategoriasRepository _categoriasRepository;
        private readonly IMapper _mapper;
        private readonly IArchivosServicio _archivoServicio;

        public ProductosServicio(IProductosRepository productosRepository, IMapper mapper, IArchivosServicio archivoService, 
            IMarcasRepository marcasRepository, ICategoriasRepository categoriasRepository)
        {
            _productosRepository = productosRepository;
            _mapper = mapper;
            _archivoServicio = archivoService;
            _marcasRepository = marcasRepository;
            _categoriasRepository = categoriasRepository;
        }

        public async Task<IEnumerable<Producto>> ObtenerTodos(bool incluirEliminados = false)
        {
            return await _productosRepository.ObtenerTodos(incluirEliminados);
        }

        public async Task<Producto?> ObtenerPorId(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Id incorrecto.");

            return await _productosRepository.ObtenerPorId(id);
        }

        public async Task<int> Crear(CrearProductoDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.Nombre))
                throw new ArgumentException("El nombre es obligatorio.");

            if (dto.StockActual < 0)
                throw new ArgumentException("El stock no puede ser negativo.");

            if (dto.PrecioCompra >= dto.PrecioVenta)
                throw new ArgumentException("El precio de venta debe ser mayor al precio de compra");

            var existePorCodigo = await _productosRepository.ExistePorCodigo(dto.Codigo);

            if (existePorCodigo)
                throw new InvalidOperationException($"Ya existe un producto con el còdigo: {dto.Codigo}");

            var existePorCodigoBarras = await _productosRepository.ExistePorCodigoBarra(dto.CodigoBarra);

            if (existePorCodigoBarras)
                throw new InvalidOperationException($"Ya existe un producto con el código de barras: {dto.CodigoBarra}");

            var marca = await _marcasRepository.ObtenerPorId(dto.IdMarca);
            if(marca == null)
                throw new InvalidOperationException("La marca ingresada no existe.");

            var categoria = await _categoriasRepository.ObtenerPorId(dto.IdCategoria);
            if (categoria == null)
                throw new InvalidOperationException("La categoría ingresada no existe.");

            var rutaImagen = await _archivoServicio.GuardarImagen(dto.Imagen, "productos");

            var producto = _mapper.Map<Producto>(dto);
            producto.Activo = true;
            producto.FechaAlta = DateTime.UtcNow;
            producto.UrlImagen = rutaImagen;

            return await _productosRepository.Crear(producto);
        }

        public async Task Actualizar(int id, ActualizarProductoDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (id <= 0)
                throw new ArgumentException("Id incorrecto.");

            if (string.IsNullOrWhiteSpace(dto.Nombre))
                throw new ArgumentException("El nombre es obligatorio.");

            if (dto.StockActual < 0)
                throw new ArgumentException("El stock no puede ser negativo.");

            var existente = await _productosRepository.ObtenerPorId(id);

            if (existente == null)
                throw new InvalidOperationException("Producto no encontrado.");

            if (dto.PrecioCompra >= dto.PrecioVenta)
                throw new ArgumentException("El precio de venta debe ser mayor al precio de compra");

            var existePorCodigo = await _productosRepository.ExistePorCodigo(dto.Codigo, id);

            if (existePorCodigo)
                throw new InvalidOperationException($"Ya existe un producto con el còdigo: {dto.Codigo}");

            var existePorCodigoBarras = await _productosRepository.ExistePorCodigoBarra(dto.CodigoBarra, id);

            if (existePorCodigoBarras)
                throw new InvalidOperationException($"Ya existe un producto con el código de barras: {dto.CodigoBarra}");

            var marca = await _marcasRepository.ObtenerPorId(dto.IdMarca);
            if (marca == null)
                throw new InvalidOperationException("La marca ingresada no existe.");

            var categoria = await _categoriasRepository.ObtenerPorId(dto.IdCategoria);
            if (categoria == null)
                throw new InvalidOperationException("La categoría ingresada no existe.");

            var rutaImagen = existente.UrlImagen;

            if (dto.Imagen is not null && dto.Imagen.Length > 0)
                rutaImagen = await _archivoServicio.GuardarImagen(dto.Imagen, "productos", existente.UrlImagen);

            var producto = _mapper.Map<Producto>(dto);
            producto.Id = id;
            producto.UrlImagen = rutaImagen;

            await _productosRepository.Actualizar(producto);
        }

        public async Task<bool> DarDeBaja(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Id erróneo.");

            return await _productosRepository.DarDeBaja(id);
        }

        public async Task<bool> Restaurar(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Id erróneo.");

            return await _productosRepository.Restaurar(id);
        }

        public async Task<bool> EliminarPermanentemente(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Id erróneo.");

            return await _productosRepository.EliminarPermanentemente(id);
        }

        public async Task<bool> ActualizarPrecioIndividual(int idProducto, ActualizacionPrecioIndividualDto dto)
        {
            if (dto.Valor < 0)
                throw new ArgumentException("El valor no puede ser negativo.");

            var tipoOperacion = dto.TipoOperacion.ToString().ToLower();

            return await _productosRepository.ActualizarPrecioIndividual(idProducto, dto.Valor, tipoOperacion);
        }

        public async Task<int> ActualizarPrecios(ActualizacionPrecioMasivamenteDto dto)
        {
            if (dto.Valor < 0)
                throw new ArgumentException("El valor no puede ser negativo.");

            if (!dto.IdCategoria.HasValue && !dto.IdMarca.HasValue)
            {
                // Si no tiene filtros y soloActivos = false,
                // estaría afectando TODOS los productos.
                // Podés decidir permitirlo o bloquearlo.
            }

            var tipoOperacion = dto.TipoOperacion.ToString().ToLower();

            return await _productosRepository.ActualizarPrecios(dto.Valor, tipoOperacion, dto.IdCategoria, dto.IdMarca, dto.SoloActivos);
        }
    }
}
