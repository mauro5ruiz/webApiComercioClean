using Comercio.Application.Dtos.Marcas;
using Comercio.Application.Dtos.Sucursales;
using Comercio.Application.Interfaces;
using Comercio.Domain.Entidades;
using Comercio.Domain.Interfaces;

namespace Comercio.Application.Servicios
{
    public class SucursalesServicio : ISucursalesServicio
    {
        private readonly ISucursalesRepository _sucursalesRepository;

        public SucursalesServicio(ISucursalesRepository sucursalesRepository)
        {
            _sucursalesRepository = sucursalesRepository;
        }

        public async Task<IEnumerable<SucursalDto>> ObtenerTodas()
        {
            var sucursales = await _sucursalesRepository.ObtenerTodas();
            return sucursales.Select(MapToDto);
        }

        public async Task<SucursalDto?> ObtenerPorId(int id)
        {
            if (id <= 0) return null;

            var sucursal = await _sucursalesRepository.ObtenerPorId(id);
            return sucursal is null ? null : MapToDto(sucursal);
        }

        public async Task<int> Crear(CrearSucursalDto dto)
        {
            Validar(dto);

            var sucursalBd = await _sucursalesRepository.ObtenerPorNombre(dto.Nombre.Trim());

            if (sucursalBd is not null && sucursalBd.Id > 0)
                throw new ArgumentException($"Ya existe una sucursal con el nombre: {sucursalBd.Nombre}.");

            var existePorCodigo = await _sucursalesRepository.ExistePorCodigo(dto.Codigo);
            if(existePorCodigo)
                throw new ArgumentException($"Ya existe una sucursal con el código: {dto.Codigo.Trim()}.");

            var sucursal = new Sucursal
            {
                Nombre = dto.Nombre.Trim(),
                Direccion = dto.Direccion,
                Localidad = dto.Localidad,
                NroTelefono = dto.NroTelefono,
                Email = dto.Email,  
                Codigo = dto.Codigo,
                Activa = dto.Activa
            };

            return await _sucursalesRepository.Crear(sucursal);
        }

        public async Task<bool> Actualizar(int id, CrearSucursalDto dto)
        {
            if (id <= 0)
                throw new ArgumentException("Id erróneo.", nameof(id));

            Validar(dto);

            var existente = await _sucursalesRepository.ObtenerPorId(id);
            if (existente is null)
                return false;

            var sucursalConMismoNombre = await _sucursalesRepository.ObtenerPorNombre(dto.Nombre.Trim());

            if (sucursalConMismoNombre is not null &&
                sucursalConMismoNombre.Id != id)
                throw new ArgumentException($"Ya existe una sucursal con el nombre: {dto.Nombre}.");

            existente.Nombre = dto.Nombre.Trim();
            existente.Direccion = dto.Direccion?.Trim();
            existente.NroTelefono = dto.NroTelefono?.Trim();
            existente.Email = dto.Email?.Trim();
            existente.Localidad = dto.Localidad?.Trim();
            existente.Codigo = dto.Codigo?.Trim();
            existente.Activa = dto.Activa;

            await _sucursalesRepository.Actualizar(existente);

            return true;
        }


        public async Task<bool> Eliminar(int id)
        {
            if (id <= 0) return false;

            var existente = await _sucursalesRepository.ObtenerPorId(id);
            if (existente is null) return false;

            await _sucursalesRepository.Eliminar(id);
            return true;
        }

        public async Task<bool> DarDeBaja(int id)
        {
            if (id <= 0) return false;

            var existente = await _sucursalesRepository.ObtenerPorId(id);
            if (existente is null) return false;

            await _sucursalesRepository.DarDeBaja(id);
            return true;
        }

        public async Task<bool> Restaurar(int id)
        {
            if (id <= 0) return false;

            var existente = await _sucursalesRepository.ObtenerPorId(id);
            if (existente is null) return false;

            await _sucursalesRepository.Restaurar(id);
            return true;
        }

        private static SucursalDto MapToDto(Sucursal sucursal) => new() { Id = sucursal.Id, Nombre = sucursal.Nombre, Direccion = sucursal.Direccion, 
            Localidad = sucursal.Localidad, NroTelefono = sucursal.NroTelefono, Email = sucursal.Email, Codigo = sucursal.Codigo, Activa = sucursal.Activa };

        private static void Validar(CrearSucursalDto dto)
        {
            if (dto is null)
                throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.Nombre))
                throw new ArgumentException("El nombre es obligatorio.", nameof(dto.Nombre));

            if (dto.Nombre.Trim().Length > 100)
                throw new ArgumentException("El nombre no puede superar los 100 caracteres.", nameof(dto.Nombre));

            if (!string.IsNullOrWhiteSpace(dto.Direccion) && dto.Direccion.Length > 90)
                throw new ArgumentException("La dirección no puede superar los 90 caracteres.", nameof(dto.Direccion));

            if (!string.IsNullOrWhiteSpace(dto.NroTelefono))
            {
                if (dto.NroTelefono.Length > 20)
                    throw new ArgumentException("El número de teléfono no puede superar los 20 caracteres.", nameof(dto.NroTelefono));

                // Opcional: validar formato básico
                if (!System.Text.RegularExpressions.Regex.IsMatch(dto.NroTelefono, @"^[0-9+\-\s()]+$"))
                    throw new ArgumentException("El número de teléfono tiene un formato inválido.", nameof(dto.NroTelefono));
            }

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                if (dto.Email.Length > 150)
                    throw new ArgumentException("El email no puede superar los 150 caracteres.", nameof(dto.Email));

                if (!System.Text.RegularExpressions.Regex.IsMatch(dto.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                    throw new ArgumentException("El email tiene un formato inválido.", nameof(dto.Email));
            }

            if (!string.IsNullOrWhiteSpace(dto.Localidad) && dto.Localidad.Length > 100)
                throw new ArgumentException("La localidad no puede superar los 100 caracteres.", nameof(dto.Localidad));

            if (!string.IsNullOrWhiteSpace(dto.Codigo) && dto.Codigo.Length > 30)
                throw new ArgumentException("El código no puede superar los 30 caracteres.", nameof(dto.Codigo));
        }

    }
}
