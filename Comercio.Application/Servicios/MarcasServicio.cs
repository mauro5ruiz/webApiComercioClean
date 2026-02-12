using Comercio.Application.Dtos.Marcas;
using Comercio.Application.Interfaces;
using Comercio.Domain.Entidades;
using Comercio.Domain.Interfaces;

namespace Comercio.Application.Servicios
{
    public class MarcasServicio : IMarcasServicio
    {
        private readonly IMarcasRepository _marcasRepository;
        private readonly IArchivosServicio _archivoServicio;

        public MarcasServicio(IMarcasRepository marcasRepository, IArchivosServicio archivoService)
        {
            _marcasRepository = marcasRepository;
            _archivoServicio = archivoService;
        }

        public async Task<MarcaDto?> ObtenerPorId(int id)
        {
            if (id <= 0) return null;

            var marca = await _marcasRepository.ObtenerPorId(id);
            return marca is null ? null : MapToDto(marca);
        }

        public async Task<IEnumerable<MarcaDto>> ObtenerTodas()
        {
            var marcas = await _marcasRepository.ObtenerTodas();
            return marcas.Select(MapToDto);
        }

        public async Task<int> Crear(CrearMarcaDto dto)
        {
            Validar(dto);

            var marcaBd = await _marcasRepository.ObtenerPorNombre(dto.Nombre.Trim());

            if (marcaBd is not null && marcaBd.Id > 0)
                throw new ArgumentException($"Ya existe una marca con el nombre: {marcaBd.Nombre}.");

            var rutaImagen = await _archivoServicio.GuardarImagen(dto.Imagen, "marcas");

            var marca = new Marca
            {
                Nombre = dto.Nombre.Trim(),
                Imagen = rutaImagen,
                Activa = dto.Activa
            };

            return await _marcasRepository.Crear(marca);
        }


        public async Task<bool> Actualizar(int id, CrearMarcaDto dto)
        {
            if (id <= 0)
                throw new ArgumentException("Id inválido.", nameof(id));

            if (dto is null)
                throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.Nombre))
                throw new ArgumentException("El nombre es obligatorio.", nameof(dto.Nombre));

            if (dto.Nombre.Trim().Length > 50)
                throw new ArgumentException("El nombre no puede superar 50 caracteres.", nameof(dto.Nombre));

            var existente = await _marcasRepository.ObtenerPorId(id);
            if (existente is null)
                return false;

            var marcaConMismoNombre = await _marcasRepository.ObtenerPorNombre(dto.Nombre.Trim());

            if (marcaConMismoNombre is not null && marcaConMismoNombre.Id != id)
                throw new ArgumentException($"Ya existe una marca con el nombre: {dto.Nombre}.");

            var rutaImagen = existente.Imagen;

            if (dto.Imagen is not null && dto.Imagen.Length > 0)
                rutaImagen = await _archivoServicio.GuardarImagen(dto.Imagen,"marcas",existente.Imagen);

            existente.Nombre = dto.Nombre.Trim();
            existente.Imagen = rutaImagen;
            existente.Activa = dto.Activa;

            await _marcasRepository.Actualizar(existente);
            return true;
        }


        public async Task<bool> Eliminar(int id)
        {
            if (id <= 0) return false;

            var existente = await _marcasRepository.ObtenerPorId(id);
            if (existente is null) return false;

            if (!string.IsNullOrEmpty(existente.Imagen))
                _archivoServicio.EliminarImagen(existente.Imagen);


            await _marcasRepository.Eliminar(id);
            return true;
        }

        private static MarcaDto MapToDto(Marca marca) => new() { Id = marca.Id, Nombre = marca.Nombre, Imagen = marca.Imagen };

        private static void Validar(CrearMarcaDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.Nombre))
                throw new ArgumentException("El nombre es obligatorio.", nameof(dto.Nombre));

            if (dto.Nombre.Trim().Length > 50)
                throw new ArgumentException("El nombre no puede superar 50 caracteres.", nameof(dto.Nombre));

        }
    }
}
