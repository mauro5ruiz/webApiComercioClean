using AutoMapper;
using Comercio.Application.Dtos.Productos;
using Comercio.Application.Interfaces;
using Comercio.Domain.Entidades;
using Comercio.Domain.Interfaces;

namespace Comercio.Application.Servicios
{
    public class ProductosServicio : IProductosServicio
    {
        private readonly IProductosRepository _repository;
        private readonly IMarcasRepository _marcasRepository;
        private readonly ICategoriasRepository _categoriasRepository;
        private readonly IMapper _mapper;
        private readonly IArchivosServicio _archivoServicio;

        public ProductosServicio(IProductosRepository repository, IMapper mapper, IArchivosServicio archivoService, 
            IMarcasRepository marcasRepository, ICategoriasRepository categoriasRepository)
        {
            _repository = repository;
            _mapper = mapper;
            _archivoServicio = archivoService;
            _marcasRepository = marcasRepository;
            _categoriasRepository = categoriasRepository;
        }

        public async Task<IEnumerable<Producto>> ObtenerTodos(bool incluirEliminados = false)
        {
            return await _repository.ObtenerTodos(incluirEliminados);
        }

        public async Task<Producto?> ObtenerPorId(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Id incorrecto.");

            return await _repository.ObtenerPorId(id);
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

            var existePorCodigo = await _repository.ExistePorCodigo(dto.Codigo);

            if (existePorCodigo)
                throw new InvalidOperationException($"Ya existe un producto con el còdigo: {dto.Codigo}");

            var existePorCodigoBarras = await _repository.ExistePorCodigoBarra(dto.CodigoBarra);

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

            return await _repository.Crear(producto);
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

            var existente = await _repository.ObtenerPorId(id);

            if (existente == null)
                throw new InvalidOperationException("Producto no encontrado.");

            if (dto.PrecioCompra >= dto.PrecioVenta)
                throw new ArgumentException("El precio de venta debe ser mayor al precio de compra");

            var existePorCodigo = await _repository.ExistePorCodigo(dto.Codigo, id);

            if (existePorCodigo)
                throw new InvalidOperationException($"Ya existe un producto con el còdigo: {dto.Codigo}");

            var existePorCodigoBarras = await _repository.ExistePorCodigoBarra(dto.CodigoBarra, id);

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

            await _repository.Actualizar(producto);
        }

        public async Task<bool> DarDeBaja(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Id erróneo.");

            return await _repository.DarDeBaja(id);
        }

        public async Task<bool> Restaurar(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Id erróneo.");

            return await _repository.Restaurar(id);
        }

        public async Task<bool> EliminarPermanentemente(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Id erróneo.");

            return await _repository.EliminarPermanentemente(id);
        }
    }
}
