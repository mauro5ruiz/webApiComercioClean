using AutoMapper;
using Comercio.Application.Dtos.Vendedores;
using Comercio.Application.Interfaces;
using Comercio.Domain.Entidades;
using Comercio.Domain.Interfaces;

namespace Comercio.Application.Servicios
{
    public class VendedoresServicio: IVendedoresServicio
    {
        private readonly IVendedoresRepository _vendedoresRepository;

        public VendedoresServicio(IVendedoresRepository vendedoresRepository)
        {
            _vendedoresRepository = vendedoresRepository;
            
        }

        public async Task<VendedorDto?> ObtenerPorId(int id)
        {
            if (id <= 0) throw new ArgumentException("Id incorrecto.");

            var vendedor = await _vendedoresRepository.ObtenerPorId(id);
            return vendedor is null ? null : MapToDto(vendedor);
        }

        public async Task<IEnumerable<VendedorDto>> ObtenerTodos(bool incluirEliminados = false)
        {
            var vendedores = await _vendedoresRepository.ObtenerTodos(incluirEliminados);
            return vendedores.Select(MapToDto);
        }

        public async Task<int> Crear(CrearVendedorDto dto)
        {
            Validar(dto);

            var existe = await _vendedoresRepository.ExistePorNroDni(dto.NroDni.Trim());
            if (existe)
                throw new ArgumentException($"Ya existe un vendedor con el DNI {dto.NroDni}.");

            var vendedor = new Vendedor()
            {
                Nombre = dto.Nombre.Trim(),
                Apellido = dto.Apellido.Trim(),
                NroDni = dto.NroDni.Trim(),
                Email = dto.Email?.Trim(),
                Telefono = dto.Telefono?.Trim(),
                Direccion = dto.Direccion?.Trim(),
                FechaNacimiento = dto.FechaNacimiento,
                IdSucursal = dto.IdSucursal,
                FechaAlta = DateTime.UtcNow,
                Activo = true,
                Observaciones = dto.Observaciones?.Trim(),
                PinHash = dto.PinHash?.Trim() ?? string.Empty
            };

            return await _vendedoresRepository.Crear(vendedor);
        }

        public async Task<bool> Actualizar(int id, CrearVendedorDto dto)
        {
            if (id <= 0)
                throw new ArgumentException("Id inválido.", nameof(id));

            Validar(dto);

            var existente = await _vendedoresRepository.ObtenerPorId(id);
            if (existente is null)
                return false;

            var existeConMismoDni = await _vendedoresRepository.ExistePorNroDni(dto.NroDni.Trim(), id);
            if (existeConMismoDni)
                throw new ArgumentException($"Ya existe otro vendedor con el DNI {dto.NroDni}.");

            var vendedor = new Vendedor()
            {
                Id = id,
                Nombre = dto.Nombre.Trim(),
                Apellido = dto.Apellido.Trim(),
                NroDni = dto.NroDni.Trim(),
                Email = dto.Email?.Trim(),
                Telefono = dto.Telefono?.Trim(),
                Direccion = dto.Direccion?.Trim(),
                FechaNacimiento = dto.FechaNacimiento,
                IdSucursal = dto.IdSucursal,
                FechaAlta = DateTime.UtcNow,
                Activo = true,
                Observaciones = dto.Observaciones?.Trim(),
                PinHash = dto.PinHash?.Trim() ?? string.Empty
            };
            
            await _vendedoresRepository.Actualizar(vendedor);
            return true;
        }

        public async Task<bool> DarDeBaja(int id)
        {
            if (id <= 0) return false;

            var existente = await _vendedoresRepository.ObtenerPorId(id);
            if (existente is null) return false;

            return await _vendedoresRepository.DarDeBaja(id);
        }

        public async Task<bool> Restaurar(int id)
        {
            if (id <= 0) return false;

            var existente = await _vendedoresRepository.ObtenerPorId(id);
            if (existente is null) return false;

            return await _vendedoresRepository.Restaurar(id);
        }

        public async Task<bool> EliminarPermanentemente(int id)
        {
            if (id <= 0) return false;

            var existente = await _vendedoresRepository.ObtenerPorId(id);
            if (existente is null) return false;

            await _vendedoresRepository.EliminarPermanentemente(id);
            return true;
        }

        private static VendedorDto MapToDto(Vendedor vendedor) => new()
        {
            Id = vendedor.Id,
            Nombre = vendedor.Nombre,
            Apellido = vendedor.Apellido,
            NroDni = vendedor.NroDni,
            Email = vendedor.Email,
            Telefono = vendedor.Telefono,
            Direccion = vendedor.Direccion,
            FechaNacimiento = vendedor.FechaNacimiento,
            Activo = vendedor.Activo,
            FechaAlta = vendedor.FechaAlta,
            FechaEliminado = vendedor.FechaEliminado,
            Observaciones = vendedor.Observaciones,
            IdSucursal = vendedor.IdSucursal
        };

        private static void Validar(CrearVendedorDto dto)
        {
            if (dto is null)
                throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.Nombre))
                throw new ArgumentException("El nombre es obligatorio.", nameof(dto.Nombre));

            if (string.IsNullOrWhiteSpace(dto.Apellido))
                throw new ArgumentException("El apellido es obligatorio.", nameof(dto.Apellido));

            if (string.IsNullOrWhiteSpace(dto.NroDni))
                throw new ArgumentException("El DNI es obligatorio.", nameof(dto.NroDni));

            if (dto.Nombre.Trim().Length > 50)
                throw new ArgumentException("El nombre no puede superar 50 caracteres.");

            if (dto.Apellido.Trim().Length > 50)
                throw new ArgumentException("El apellido no puede superar 50 caracteres.");
        }
    }
}
