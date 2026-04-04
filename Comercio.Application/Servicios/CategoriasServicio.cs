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

        public async Task<CategoriaDto> Crear(CrearCategoriaDto dto)
        {
            Validar(dto);
            var categoriaBd = await _categoriasRepository.ObtenerPorNombre(dto.Nombre.Trim());

            if(categoriaBd is not null && categoriaBd.Id > 0)
                throw new ArgumentException($"Ya existe una categoría con el nombre: {categoriaBd.Nombre}.");

            var categoria = new Categoria
            {
                Nombre = dto.Nombre.Trim()
            };

            var id = await _categoriasRepository.Crear(categoria);

            return new CategoriaDto
            {
                Id = id,
                Nombre = categoria.Nombre
            };
        }

        public async Task<CategoriaDto> Actualizar(int id, CrearCategoriaDto dto)
        {
            if (id <= 0)
                throw new ArgumentException("Id inválido");

            Validar(dto);

            var existente = await _categoriasRepository.ObtenerPorId(id);
            if (existente is null)
                throw new ArgumentException("No se encontró la categoría.");

            var existentePorNombre = await _categoriasRepository.ObtenerPorNombre(dto.Nombre.Trim());
            if (existentePorNombre is not null && existentePorNombre.Id != id)
                throw new ArgumentException($"Ya existe una categoría con el nombre: {dto.Nombre}.");

            existente.Nombre = dto.Nombre.Trim();

            await _categoriasRepository.Actualizar(existente);

            return MapToDto(existente);
        }

        public async Task Eliminar(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Id inválido");

            var categoria = await _categoriasRepository.ObtenerPorId(id);
            if (categoria is null)
                throw new ArgumentException("No se encontró la categoría.");

            if (categoria.CantidadProductos > 0)
                throw new ArgumentException($"No se puede borrar la categoría '{categoria.Nombre}' porque tiene {categoria.CantidadProductos} producto(s) asociados.");
            
            await _categoriasRepository.Eliminar(id);
        }

        private static CategoriaDto MapToDto(Categoria categoria) => new() { Id = categoria.Id, Nombre = categoria.Nombre, CantidadProductos = categoria.CantidadProductos };

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
