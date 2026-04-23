using AutoMapper;
using Comercio.Application.Dtos.Proveedores;
using Comercio.Application.Interfaces;
using Comercio.Domain.Entidades;
using Comercio.Domain.Interfaces;

namespace Comercio.Application.Servicios
{
    public class ProveedoresServicio : IProveedoresServicio
    {
        private readonly IProveedoresRepository _repository;
        private readonly IMapper _mapper;
        private readonly IArchivosServicio _archivoServicio;

        public ProveedoresServicio(IProveedoresRepository repository, IMapper mapper, IArchivosServicio archivoService)
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

        public async Task<ProveedorDto> Crear(CrearProveedorDto dto)
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
            proveedor.FechaCreacion = DateTime.UtcNow;
            proveedor.UrlImagen = rutaImagen;

            var id = await _repository.Crear(proveedor);

            return new ProveedorDto
            {
                Id = id,
                RazonSocial = proveedor.RazonSocial,
                Cuit = proveedor.CUIT,
                CondicionIva = proveedor.CondicionIVA,
                Telefono = proveedor.Telefono,
                Email = proveedor.Email,
                PersonaContacto = proveedor.PersonaContacto,
                Direccion = proveedor.Direccion,
                Provincia = proveedor.Provincia,
                Localidad = proveedor.Localidad,
                CodigoPostal = proveedor.CodigoPostal,
                Observaciones = proveedor.Observaciones,
                UrlImagen = proveedor.UrlImagen,
                Activo = proveedor.Activo
            };
        }

        public async Task<ProveedorDto> Actualizar(int id, ActualizarProveedorDto dto)
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

            if (dto.Imagen is not null && dto.Imagen.Length > 0) { }
                rutaImagen = await _archivoServicio.GuardarImagen(dto.Imagen, "proveedores", existente.UrlImagen);

            var proveedor = _mapper.Map<Proveedor>(dto);
            proveedor.Id = id;
            proveedor.UrlImagen = rutaImagen;


            if (dto.EliminarImagen && !string.IsNullOrEmpty(rutaImagen))
            {
                _archivoServicio.EliminarImagen(rutaImagen);
                proveedor.UrlImagen = null;
            }

            await _repository.Actualizar(proveedor);

            return new ProveedorDto
            {
                Id = id,
                RazonSocial = proveedor.RazonSocial,
                Cuit = proveedor.CUIT,
                CondicionIva = proveedor.CondicionIVA,
                Telefono = proveedor.Telefono,
                Email = proveedor.Email,
                PersonaContacto = proveedor.PersonaContacto,
                Direccion = proveedor.Direccion,
                Provincia = proveedor.Provincia,
                Localidad = proveedor.Localidad,
                CodigoPostal = proveedor.CodigoPostal,
                Observaciones = proveedor.Observaciones,
                UrlImagen = proveedor.UrlImagen,
                Activo = proveedor.Activo
            };
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

        public async Task EliminarPermanentemente(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Id erróneo.");

            var existente = await _repository.ObtenerPorId(id);
            if (existente is null) throw new ArgumentException("No se encontró el proveedor");

            await _repository.EliminarPermanentemente(id);
        }

        private string NormalizarCuit(string cuit)
        {
            return cuit.Replace("-", "")
                       .Replace(" ", "")
                       .Trim();
        }
    }
}
