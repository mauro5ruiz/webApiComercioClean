using Comercio.Application.Dtos.Categorias;
using Comercio.Application.Interfaces;
using Comercio.Domain.Entidades;
using Comercio.Domain.Interfaces;

namespace Comercio.Application.Servicios
{
    public class CategoriasServicio : ICategoriasServicio
    {
        private readonly ICategoriasRepository _categoriasRepository;

        public CategoriasServicio(ICategoriasRepository categoriasRepository)
        {
            _categoriasRepository = categoriasRepository;
        }

        public async Task<IEnumerable<CategoriaDto>> ObtenerTodas()
        {
            var categorias = await _categoriasRepository.ObtenerTodas();
            return categorias.Select(MapToDto);
        }

        public async Task<CategoriaDto?> ObtenerPorId(int id)
        {
            if (id <= 0) return null;

            var categoria = await _categoriasRepository.ObtenerPorId(id);
            return categoria is null ? null : MapToDto(categoria);
        }

        public async Task<int> Crear(CrearCategoriaDto dto)
        {
            Validar(dto);
            var categoriaBd = await _categoriasRepository.ObtenerPorNombre(dto.Nombre.Trim());

            if(categoriaBd is not null && categoriaBd.Id > 0)
                throw new ArgumentException($"Ya existe una categoría con el nombre: {categoriaBd.Nombre}.");

            var categoria = new Categoria
            {
                Nombre = dto.Nombre.Trim()
            };

            return await _categoriasRepository.Crear(categoria);
        }

        public async Task<bool> Actualizar(int id, CrearCategoriaDto dto)
        {
            if (id <= 0) return false;
            Validar(dto);

            var existente = await _categoriasRepository.ObtenerPorId(id);
            if (existente is null)
                throw new ArgumentException($"No se encontró la ccategoria. Por favor, intente nuevamente.");

            var existentePorNombre = await _categoriasRepository.ObtenerPorNombre(dto.Nombre);
            if (existentePorNombre is not null && existentePorNombre.Id != id)
                throw new ArgumentException($"Ya existe una categoría con el nombre: {dto.Nombre}.");

            existente.Nombre = dto.Nombre.Trim();
            await _categoriasRepository.Actualizar(existente);

            return true;
        }

        public async Task<bool> Eliminar(int id)
        {
            if (id <= 0) return false;

            var existente = await _categoriasRepository.ObtenerPorId(id);
            if (existente is null) return false;

            await _categoriasRepository.Eliminar(id);
            return true;
        }

        private static CategoriaDto MapToDto(Categoria categoria) => new() { Id = categoria.Id, Nombre = categoria.Nombre };

        private static void Validar(CrearCategoriaDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.Nombre))
                throw new ArgumentException("El nombre es obligatorio.", nameof(dto.Nombre));

            if (dto.Nombre.Trim().Length > 50)
                throw new ArgumentException("El nombre no puede superar 50 caracteres.", nameof(dto.Nombre));
        }
    }
}
