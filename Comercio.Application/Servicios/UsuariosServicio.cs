using AutoMapper;
using Comercio.Application.Dtos.Usuarios;
using Comercio.Application.Interfaces;
using Comercio.Domain.Entidades;
using Comercio.Domain.Interfaces;

namespace Comercio.Application.Servicios
{
    public class UsuariosServicio : IUsuariosServicio
    {
        private readonly IUsuariosRepository _usuariosRepository;
        private readonly IMapper _mapper;

        public UsuariosServicio(IUsuariosRepository usuariosRepository, IMapper mapper)
        {
            _usuariosRepository = usuariosRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Usuario>> ObtenerTodos(bool incluirEliminados = false)
        {
            return await _usuariosRepository.ObtenerTodos(incluirEliminados);
        }

        public async Task<Usuario?> ObtenerPorId(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Id incorrecto.");

            return await _usuariosRepository.ObtenerPorId(id);
        }

        public async Task<IEnumerable<Usuario>> ObtenerPorSucursal(int sucursalId)
        {
            if (sucursalId <= 0)
                throw new ArgumentException("Sucursal incorrecta.");

            return await _usuariosRepository.ObtenerPorSucursal(sucursalId);
        }

        public async Task<int> Crear(CrearUsuarioDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.Nombre))
                throw new ArgumentException("El nombre es obligatorio.");

            if (string.IsNullOrWhiteSpace(dto.Apellido))
                throw new ArgumentException("El apellido es obligatorio.");

            if (string.IsNullOrWhiteSpace(dto.UsuarioLogin))
                throw new ArgumentException("El usuario es obligatorio.");

            if (string.IsNullOrWhiteSpace(dto.Clave))
                throw new ArgumentException("La clave es obligatoria.");

            var existeUsuario = await _usuariosRepository.ExisteUsuario(dto.UsuarioLogin);
            if (existeUsuario)
                throw new InvalidOperationException("Ya existe un usuario con ese nombre.");

            var existeEmail = await _usuariosRepository.ExisteEmail(dto.Email); 
            if (existeEmail)
                throw new InvalidOperationException("Ya existe un usuario con ese email.");

            // Si NO es admin, debe tener sucursal
            if (dto.RolId != 1 && dto.SucursalId == null)
                throw new InvalidOperationException("Debe asignarse una sucursal al usuario.");

            var hash = BCrypt.Net.BCrypt.HashPassword(dto.Clave);

            var usuario = _mapper.Map<Usuario>(dto);
            usuario.ClaveHash = hash;
            usuario.Activo = true;
            usuario.FechaCreacion = DateTime.UtcNow;

            return await _usuariosRepository.Crear(usuario);
        }

        public async Task Actualizar(int id, ActualizarUsuarioDto dto)
        {
            if (id <= 0)
                throw new ArgumentException("Id incorrecto.");

            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var existente = await _usuariosRepository.ObtenerPorId(id);

            if (existente == null)
                throw new InvalidOperationException("Usuario no encontrado.");

            if (dto.RolId != 1 && dto.SucursalId == null)
                throw new InvalidOperationException("Debe asignarse una sucursal al usuario.");

            _mapper.Map(dto, existente);

            await _usuariosRepository.Actualizar(existente);
        }

        public async Task CambiarClave(CambiarClaveDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.NuevaClave))
                throw new ArgumentException("La nueva clave es obligatoria.");

            var usuario = await _usuariosRepository.ObtenerPorId(dto.UsuarioId);

            if (usuario == null)
                throw new InvalidOperationException("Usuario no encontrado.");

            var claveCorrecta = BCrypt.Net.BCrypt.Verify(dto.ClaveActual, usuario.ClaveHash);

            if (!claveCorrecta)
                throw new InvalidOperationException("La clave actual es incorrecta.");

            var nuevoHash = BCrypt.Net.BCrypt.HashPassword(dto.NuevaClave);

            await _usuariosRepository.CambiarClave(usuario.Id, nuevoHash);
        }

        public async Task<bool> Activar(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Id incorrecto.");

            return await _usuariosRepository.Activar(id);
        }

        public async Task<bool> Desactivar(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Id incorrecto.");

            return await _usuariosRepository.Desactivar(id);
        }

        public async Task<Usuario?> Login(LoginDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var usuario = await _usuariosRepository.ObtenerPorUsuario(dto.Usuario);

            if (usuario == null)
                return null;

            if (!usuario.Activo)
                return null;

            var claveValida = BCrypt.Net.BCrypt.Verify(dto.Clave, usuario.ClaveHash);

            if (!claveValida)
                return null;

            await _usuariosRepository.ActualizarUltimoAcceso(usuario.Id);

            return usuario;
        }
    }
}
