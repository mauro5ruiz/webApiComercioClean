using AutoMapper;
using Comercio.Application.Dtos.Clientes;
using Comercio.Application.Interfaces;
using Comercio.Domain.Entidades;
using Comercio.Domain.Interfaces;

namespace Comercio.Application.Servicios
{
    public class ClientesServicio : IClientesServicio
    {
        private readonly IClientesRepository _repository;
        private readonly IMapper _mapper;
        private readonly IArchivosServicio _archivoServicio;

        public ClientesServicio(IClientesRepository repository, IMapper mapper, IArchivosServicio archivoService)
        {
            _repository = repository;
            _mapper = mapper;
            _archivoServicio = archivoService;
        }

        public async Task<IEnumerable<Cliente>> ObtenerTodos(bool incluirEliminados = false)
        {
            return await _repository.ObtenerTodos(incluirEliminados);
        }

        public async Task<Cliente?> ObtenerPorId(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Id incorrecto.");

            return await _repository.ObtenerPorId(id);
        }

        public async Task<int> Crear(CrearClienteDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            string identificador = string.Empty;

            if(dto.TipoCliente == 1)
            {
                if (string.IsNullOrWhiteSpace(dto.NroDocumento))
                    throw new ArgumentException("El número de documento es obligatorio.");

                if (string.IsNullOrWhiteSpace(dto.Nombre))
                    throw new ArgumentException("El nombre es obligatorio.");

                if (string.IsNullOrWhiteSpace(dto.Apellido))
                    throw new ArgumentException("El apellido es obligatorio.");

                identificador = dto.NroDocumento.Trim();

                dto.NroDocumento = NormalizarDni(dto.NroDocumento);
            }
            else if (dto.TipoCliente == 2)
            {
                if (string.IsNullOrWhiteSpace(dto.Cuit))
                    throw new ArgumentException("El CUIT es obligatorio.");

                if (string.IsNullOrWhiteSpace(dto.RazonSocial))
                    throw new ArgumentException("La razón social es obligatoria.");

                identificador = NormalizarCuit(dto.Cuit);

                dto.Cuit = NormalizarCuit(dto.Cuit);
            }
            else
            {
                throw new ArgumentException("Tipo de cliente no válido.");
            }

            var existe = await _repository.ExisteRepetido(dto.TipoCliente, identificador);

            if (existe)
                throw new InvalidOperationException("Ya existe un cliente con ese CUIT.");

            var rutaImagen = await _archivoServicio.GuardarImagen(dto.Imagen, "clientes");

            var cliente = _mapper.Map<Cliente>(dto);
            cliente.Activo = true;
            cliente.FechaAlta = DateTime.UtcNow;
            cliente.UrlImagen = rutaImagen;

            return await _repository.Crear(cliente);
        }

        public async Task Actualizar(int id, ActualizarClienteDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (id <= 0)
                throw new ArgumentException("Id incorrecto.");

            var existente = await _repository.ObtenerPorId(id);

            if (existente == null)
                throw new InvalidOperationException("Cliente no encontrado.");

            string identificador = string.Empty;

            if (dto.TipoCliente == 1) 
            {
                if (string.IsNullOrWhiteSpace(dto.NroDocumento))
                    throw new ArgumentException("El número de documento es obligatorio.");

                if (string.IsNullOrWhiteSpace(dto.Nombre))
                    throw new ArgumentException("El nombre es obligatorio.");

                if (string.IsNullOrWhiteSpace(dto.Apellido))
                    throw new ArgumentException("El apellido es obligatorio.");

                dto.NroDocumento = NormalizarDni(dto.NroDocumento);
                identificador = dto.NroDocumento;
            }
            else if (dto.TipoCliente == 2)
            {
                if (string.IsNullOrWhiteSpace(dto.Cuit))
                    throw new ArgumentException("El CUIT es obligatorio.");

                if (string.IsNullOrWhiteSpace(dto.RazonSocial))
                    throw new ArgumentException("La razón social es obligatoria.");

                dto.Cuit = NormalizarCuit(dto.Cuit);
                identificador = dto.Cuit;
            }
            else
            {
                throw new ArgumentException("Tipo de cliente no válido.");
            }

            var existe = await _repository.ExisteRepetido(dto.TipoCliente, identificador, id);

            if (existe)
                throw new InvalidOperationException(
                    dto.TipoCliente == 1
                        ? "Ya existe otro cliente con ese número de documento."
                        : "Ya existe otro cliente con ese CUIT."
                );

            var rutaImagen = existente.UrlImagen;

            if (dto.Imagen is not null && dto.Imagen.Length > 0)
                rutaImagen = await _archivoServicio.GuardarImagen(dto.Imagen, "clientes", existente.UrlImagen);

            var cliente = _mapper.Map<Cliente>(dto);
            cliente.Id = id;
            cliente.UrlImagen = rutaImagen;
            cliente.Activo = existente.Activo;
            cliente.FechaAlta = existente.FechaAlta;

            await _repository.Actualizar(cliente);
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
            try
            {
                if (id <= 0)
                    throw new ArgumentException("Id erróneo.");

                await _repository.EliminarPermanentemente(id);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private string NormalizarCuit(string cuit)
        {
            return cuit.Replace("-", "")
                       .Replace(" ", "")
                       .Trim();
        }

        private string NormalizarDni(string dni)
        {
            if (string.IsNullOrWhiteSpace(dni))
                return string.Empty;

            return new string(dni
                .Where(char.IsDigit)
                .ToArray());
        }

    }
}
