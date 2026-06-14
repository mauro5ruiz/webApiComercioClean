using Comercio.Application.Dtos.Clientes;
using Comercio.Application.Interfaces;
using Comercio.Domain.Entidades;
using Comercio.Domain.Interfaces;

namespace Comercio.Application.Servicios
{
    public class ClientesServicio : IClientesServicio
    {
        private readonly IClientesRepository _repository;
        private readonly IArchivosServicio _archivoServicio;
        private readonly IFormasDePagoRepository _formasDePagoRepository;
        private readonly IVentasRepository _ventasRepository;
        private readonly IVentasPagosRepository _ventasPagosRepository;
        private readonly ICreditoClienteRepository _creditoClienteRepository;

        public ClientesServicio(
            IClientesRepository repository,
            IArchivosServicio archivoService,
            IFormasDePagoRepository formasDePagoRepository,
            IVentasRepository ventasRepository,
            IVentasPagosRepository ventasPagosRepository,
            ICreditoClienteRepository creditoClienteRepository)
        {
            _repository = repository;
            _archivoServicio = archivoService;
            _formasDePagoRepository = formasDePagoRepository;
            _ventasRepository = ventasRepository;
            _ventasPagosRepository = ventasPagosRepository;
            _creditoClienteRepository = creditoClienteRepository;
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
            {
                string tipoIdentificador = dto.TipoCliente == 1 ? "DNI" : "Cuit";
                throw new InvalidOperationException($"Ya existe un cliente con ese {tipoIdentificador}.");
            }

            var rutaImagen = await _archivoServicio.GuardarImagen(dto.Imagen, "clientes");

            var cliente = new Cliente
            {
                TipoCliente = dto.TipoCliente,
                Nombre = dto.Nombre?.Trim(),
                Apellido = dto.Apellido?.Trim(),
                RazonSocial = dto.RazonSocial?.Trim(),
                NroDocumento = dto.NroDocumento,
                Cuit = dto.Cuit,
                Direccion = dto.Direccion,
                NroTelefono = dto.NroTelefono,
                Email = dto.Email,
                Localidad = dto.Localidad,
                Provincia = dto.Provincia,
                CodigoPostal = dto.CodigoPostal,
                Observaciones = dto.Observaciones,
                CondicionIva = dto.CondicionIva,
                Activo = dto.Activo,
                FechaAlta = DateTime.Now,
                UrlImagen = rutaImagen
            };

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

            var cliente = new Cliente
            {
                TipoCliente = dto.TipoCliente,
                Nombre = dto.Nombre?.Trim(),
                Apellido = dto.Apellido?.Trim(),
                RazonSocial = dto.RazonSocial?.Trim(),
                NroDocumento = dto.NroDocumento,
                Cuit = dto.Cuit,
                Direccion = dto.Direccion,
                NroTelefono = dto.NroTelefono,
                Email = dto.Email,
                Localidad = dto.Localidad,
                Provincia = dto.Provincia,
                CodigoPostal = dto.CodigoPostal,
                Observaciones = dto.Observaciones,
                CondicionIva = dto.CondicionIva,
                Activo = dto.Activo,
                FechaAlta = DateTime.Now,
                UrlImagen = rutaImagen
            };

            if (dto.EliminarImagen && !string.IsNullOrEmpty(rutaImagen))
            {
                _archivoServicio.EliminarImagen(rutaImagen);
                cliente.UrlImagen = null;
            }

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
            catch (Exception)
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

        public async Task<ClienteCuentaCorrienteDto> ObtenerCuentaCorriente(int idCliente, DateTime? desde, DateTime? hasta)
        {
            if (idCliente <= 0)
                throw new ArgumentException("Id incorrecto.");

            if (desde.HasValue && hasta.HasValue && desde.Value.Date > hasta.Value.Date)
                throw new ArgumentException("La fecha desde no puede ser mayor que la fecha hasta.");

            var cliente = await _repository.ObtenerPorId(idCliente);

            if (cliente is null)
                throw new InvalidOperationException("Cliente no encontrado.");

            var ventas = (await _ventasRepository.ObtenerCuentaCorrientePorCliente(idCliente))
                .Where(v =>
                    (!desde.HasValue || v.Fecha.Date >= desde.Value.Date) &&
                    (!hasta.HasValue || v.Fecha.Date <= hasta.Value.Date))
                .OrderBy(v => v.Fecha)
                .ToList();

            foreach (var venta in ventas)
                venta.Pagos = (await _ventasPagosRepository.ObtenerPorVenta(venta.Id)).ToList();

            var creditosCliente = (await _creditoClienteRepository.ObtenerPorCliente(idCliente)).ToList();
            var formasDePago = await _formasDePagoRepository.ObtenerTodas();

            var ventasPendientes = ventas
                .Where(v => v.Estado != "Anulada" && v.SaldoPendiente > 0)
                .Select(v => new ClienteVentaPendienteDto
                {
                    IdVenta = v.Id,
                    Fecha = v.Fecha,
                    Comprobante = v.NumeroComprobante,
                    Total = v.Total,
                    Cobrado = v.TotalPagado,
                    CreditoAplicado = v.CreditoAplicado,
                    SaldoPendiente = v.SaldoPendiente
                })
                .ToList();

            var movimientos = new List<ClienteMovimientoDto>();

            foreach (var venta in ventas)
            {
                movimientos.Add(new ClienteMovimientoDto
                {
                    Tipo = "Venta",
                    IdVenta = venta.Id,
                    Fecha = venta.Fecha,
                    Comprobante = venta.NumeroComprobante,
                    Importe = venta.Total,
                    TotalVenta = venta.Total,
                    CobradoVenta = venta.TotalPagado,
                    CreditoAplicadoVenta = venta.CreditoAplicado,
                    SaldoPendienteVenta = venta.SaldoPendiente
                });

                if (venta.CreditoAplicado > 0)
                {
                    movimientos.Add(new ClienteMovimientoDto
                    {
                        Tipo = "AplicacionCredito",
                        IdVenta = venta.Id,
                        Fecha = venta.Fecha,
                        Comprobante = venta.NumeroComprobante,
                        Referencia = "Aplicacion de saldo a favor en venta",
                        Importe = -venta.CreditoAplicado
                    });
                }

                foreach (var pago in venta.Pagos
                    .Where(p => p.Estado == "Activo")
                    .OrderBy(p => p.FechaPago))
                {
                    movimientos.Add(new ClienteMovimientoDto
                    {
                        Tipo = "Cobro",
                        IdVenta = venta.Id,
                        IdPago = pago.Id,
                        Fecha = pago.FechaPago,
                        Comprobante = venta.NumeroComprobante,
                        FormaPago = formasDePago.FirstOrDefault(f => f.Id == pago.IdFormaPago)?.Nombre,
                        Referencia = pago.Referencia,
                        Importe = pago.Importe
                    });
                }
            }

            foreach (var credito in creditosCliente
                .Where(c =>
                    (!desde.HasValue || c.Fecha.Date >= desde.Value.Date) &&
                    (!hasta.HasValue || c.Fecha.Date <= hasta.Value.Date))
                .OrderBy(c => c.Fecha))
            {
                movimientos.Add(new ClienteMovimientoDto
                {
                    Tipo = "Credito",
                    IdDevolucionVenta = credito.IdDevolucionVenta,
                    Fecha = credito.Fecha,
                    Comprobante = $"DEV-{credito.IdDevolucionVenta}",
                    Referencia = "Credito a favor por devolucion/anulacion de venta",
                    Importe = credito.Importe,
                    SaldoCredito = credito.Saldo
                });
            }

            var creditoDisponible = creditosCliente
                .Where(c => c.Saldo > 0)
                .Sum(c => c.Saldo);

            var ventasActivas = ventas.Where(v => v.Estado != "Anulada").ToList();
            var saldoTotalPendiente = ventasActivas.Sum(v => v.SaldoPendiente);

            return new ClienteCuentaCorrienteDto
            {
                IdCliente = cliente.Id,
                Cliente = ObtenerNombreCliente(cliente),
                SaldoTotalPendiente = saldoTotalPendiente,
                CreditoDisponible = creditoDisponible,
                SaldoNeto = saldoTotalPendiente - creditoDisponible,
                TotalVendido = ventasActivas.Sum(v => v.Total),
                TotalCobrado = ventasActivas.Sum(v => v.TotalPagado),
                VentasPendientes = ventasPendientes,
                Movimientos = movimientos.OrderBy(m => m.Fecha).ToList()
            };
        }

        public async Task CobrarCliente(int idCliente, decimal importe, int idFormaPago, string referencia)
        {
            if (idCliente <= 0)
                throw new ArgumentException("Cliente inválido.");

            if (importe <= 0)
                throw new ArgumentException("El importe debe ser mayor a cero.");

            var cliente = await _repository.ObtenerPorId(idCliente);

            if (cliente is null)
                throw new InvalidOperationException("Cliente no encontrado.");

            var ventasPendientes = (await _ventasRepository.ObtenerPorCliente(idCliente, true))
                .OrderBy(v => v.Fecha)
                .ToList();

            if (!ventasPendientes.Any())
                throw new InvalidOperationException("El cliente no tiene ventas pendientes.");

            decimal restante = importe;

            var creditos = (await _creditoClienteRepository.ObtenerPorCliente(idCliente))
                .Where(c => c.Saldo > 0)
                .OrderBy(c => c.Fecha)
                .ToList();

            foreach (var venta in ventasPendientes)
            {
                if (venta.SaldoPendiente <= 0)
                    continue;

                decimal saldoVenta = venta.SaldoPendiente;

                foreach (var credito in creditos)
                {
                    if (saldoVenta <= 0)
                        break;

                    if (credito.Saldo <= 0)
                        continue;

                    var montoCredito = Math.Min(saldoVenta, credito.Saldo);

                    var creditoConsumido = await _creditoClienteRepository.ConsumirCredito(credito.Id, montoCredito);

                    if (!creditoConsumido)
                        throw new InvalidOperationException("No se pudo aplicar el saldo a favor del cliente. Verifique el credito disponible.");

                    credito.Saldo -= montoCredito;
                    saldoVenta -= montoCredito;

                    await _ventasPagosRepository.RecalcularTotalPagado(venta.Id);
                }

                if (saldoVenta > 0 && restante > 0)
                {
                    var montoCobro = Math.Min(saldoVenta, restante);

                    var pago = new VentaPago
                    {
                        IdVenta = venta.Id,
                        IdFormaPago = idFormaPago,
                        Importe = montoCobro,
                        Referencia = referencia,
                        Estado = "Activo"
                    };

                    await _ventasPagosRepository.Insertar(pago);
                    await _ventasPagosRepository.RecalcularTotalPagado(venta.Id);

                    restante -= montoCobro;
                }

                if (restante <= 0)
                    break;
            }
        }

        private static string ObtenerNombreCliente(Cliente cliente)
        {
            if (cliente.TipoCliente == 2 && !string.IsNullOrWhiteSpace(cliente.RazonSocial))
                return cliente.RazonSocial;

            return $"{cliente.Nombre} {cliente.Apellido}".Trim();
        }

    }
}
