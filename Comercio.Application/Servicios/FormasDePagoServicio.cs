using Comercio.Application.Dtos.Categorias;
using Comercio.Application.Dtos.FormasDePago;
using Comercio.Application.Interfaces;
using Comercio.Domain.Entidades;
using Comercio.Domain.Interfaces;

namespace Comercio.Application.Servicios
{
    public class FormasDePagoServicio : IFormasDePagoServicio
    {
        private readonly IFormasDePagoRepository _formasDePagoRepository;

        public FormasDePagoServicio(IFormasDePagoRepository formasDePagoRepository)
        {
            _formasDePagoRepository = formasDePagoRepository;

        }
        public async Task<IEnumerable<FormaDePagoDto>> ObtenerTodas()
        {
            var formas = await _formasDePagoRepository.ObtenerTodas();
            return formas.Select(MapToDto);
        }

        public async Task<FormaDePagoDto?> ObtenerPorId(int id)
        {
            if (id <= 0) return null;

            var forma = await _formasDePagoRepository.ObtenerPorId(id);
            return forma is null ? null : MapToDto(forma);
        }

        public async Task<int> Crear(CrearFormaDePagoDto dto)
        {
            Validar(dto);
            var formaBd = await _formasDePagoRepository.ObtenerPorNombre(dto.Nombre.Trim());

            if (formaBd is not null && formaBd.Id > 0)
                throw new ArgumentException($"Ya existe una forma de pago con el nombre: {formaBd.Nombre}.");

            var formaDePago = new FormaDePago
            {
                Nombre = dto.Nombre.Trim()
            };

            return await _formasDePagoRepository.Crear(formaDePago);
        }

        public async Task<bool> Actualizar(int id, CrearFormaDePagoDto dto)
        {
            if (id <= 0) return false;
            Validar(dto);

            var existente = await _formasDePagoRepository.ObtenerPorId(id);
            if (existente is null)
                throw new ArgumentException($"No se encontró la forma de pago. Por favor, intente nuevamente.");

            var existentePorNombre = await _formasDePagoRepository.ObtenerPorNombre(dto.Nombre);
            if (existentePorNombre is not null && existentePorNombre.Id != id)
                throw new ArgumentException($"Ya existe una forma de pago con el nombre: {dto.Nombre}.");

            existente.Nombre = dto.Nombre.Trim();
            await _formasDePagoRepository.Actualizar(existente);

            return true;
        }

        public async Task<bool> Eliminar(int id)
        {
            if (id <= 0) return false;

            var existente = await _formasDePagoRepository.ObtenerPorId(id);
            if (existente is null) return false;

            await _formasDePagoRepository.Eliminar(id);
            return true;
        }

        private static FormaDePagoDto MapToDto(FormaDePago forma) => new() { Id = forma.Id, Nombre = forma.Nombre };

        private static void Validar(CrearFormaDePagoDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.Nombre))
                throw new ArgumentException("El nombre es obligatorio.", nameof(dto.Nombre));

            if (dto.Nombre.Trim().Length > 50)
                throw new ArgumentException("El nombre no puede superar 50 caracteres.", nameof(dto.Nombre));
        }
    }
}
