using AutoMapper;
using Comercio.Application.Dtos.Proveedores;
using Comercio.Application.Interfaces;
using Comercio.Domain.Entidades;
using Comercio.Domain.Interfaces;

namespace Comercio.Application.Servicios
{
    public class ProveedoresServicio : IProveedoresServicio
    {
        private readonly IProveedoresRepostory _repository;
        private readonly IMapper _mapper;
        private readonly IArchivosServicio _archivoServicio;

        public ProveedoresServicio(IProveedoresRepostory repository, IMapper mapper, IArchivosServicio archivoService)
        {
            _repository = repository;
            _mapper = mapper;
            _archivoServicio = archivoService;
        }

        public async Task<Proveedor?> ObtenerPorId(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Id incorrecto.");

            return await _repository.ObtenerPorId(id);
        }

        public async Task<IEnumerable<Proveedor>> ObtenerTodos(bool incluirEliminados = false)
        {
            return await _repository.ObtenerTodos(incluirEliminados);
        }

        public async Task<int> Crear(CrearProveedorDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.RazonSocial))
                throw new ArgumentException("La razón social es obligatoria.");

            if (string.IsNullOrWhiteSpace(dto.Cuit))
                throw new ArgumentException("El CUIT es obligatorio.");

            dto.Cuit = NormalizarCuit(dto.Cuit);

            var existe = await _repository.ExistePorCuit(dto.Cuit);

            if (existe)
                throw new InvalidOperationException("Ya existe un proveedor con ese CUIT.");

            var rutaImagen = await _archivoServicio.GuardarImagen(dto.Imagen, "proveedores");

            var proveedor = _mapper.Map<Proveedor>(dto);
            proveedor.Activo = true;
            proveedor.FechaCreacion = DateTime.UtcNow;
            proveedor.UrlImagen = rutaImagen;

            return await _repository.Crear(proveedor);
        }

        public async Task Actualizar(int id, ActualizarProveedorDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (id <= 0)
                throw new ArgumentException("Id incorrecto.");

            dto.Cuit = NormalizarCuit(dto.Cuit);

            var existente = await _repository.ObtenerPorId(id);

            if (existente == null)
                throw new InvalidOperationException("Proveedor no encontrado.");

            var existeRepetidoPorCuit = await _repository.ExistePorCuit(dto.Cuit, id);
            if(existeRepetidoPorCuit)
                throw new InvalidOperationException($"Ya existe otro proveedor con el Cuit: {dto.Cuit}.");

            var rutaImagen = existente.UrlImagen;

            if (dto.Imagen is not null && dto.Imagen.Length > 0)
                rutaImagen = await _archivoServicio.GuardarImagen(dto.Imagen, "proveedores", existente.UrlImagen);

            var proveedor = _mapper.Map<Proveedor>(dto);
            proveedor.Id = id;
            proveedor.UrlImagen = rutaImagen;

            await _repository.Actualizar(proveedor);
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

            return await _repository.DarDeBaja(id);
        }

        private string NormalizarCuit(string cuit)
        {
            return cuit.Replace("-", "")
                       .Replace(" ", "")
                       .Trim();
        }
    }
}
