using AutoMapper;
using Comercio.Application.Dtos.Proveedores;
using Comercio.Application.Interfaces;
using Comercio.Domain.Entidades;
using Comercio.Domain.Enums;
using Comercio.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Comercio.Application.Servicios
{
    public class ProveedoresServicio : IProveedoresServicio
    {
        private readonly IProveedoresRepository _repository;
        private readonly IFormasDePagoRepository _formasDePagoRepository;
        private readonly IComprasRepostory _comprasRepository;
        private readonly IComprasPagosRepository _pagosRepository;
        private readonly ICreditoProveedorRepository _creditoProveedorRepository;
        private readonly IMapper _mapper;
        private readonly IArchivosServicio _archivoServicio;

        public ProveedoresServicio(IProveedoresRepository repository, IFormasDePagoRepository formasDePagoRepository, IComprasRepostory comprasRepository, 
            ICreditoProveedorRepository creditoProveedorRepository, IComprasPagosRepository pagosRepository, IMapper mapper, IArchivosServicio archivoService)
        {
            _repository = repository;
            _mapper = mapper;
            _archivoServicio = archivoService;
            _formasDePagoRepository = formasDePagoRepository;
            _comprasRepository = comprasRepository;
            _creditoProveedorRepository = creditoProveedorRepository;
            _pagosRepository = pagosRepository;
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
            proveedor.FechaCreacion = DateTime.Now;
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

        public async Task<ProveedorCuentaCorrienteDto> ObtenerCuentaCorriente(int idProveedor, DateTime? desde, DateTime? hasta)
        {
            if (idProveedor <= 0)
                throw new ArgumentException("Id incorrecto.");

            if (desde.HasValue && hasta.HasValue && desde.Value.Date > hasta.Value.Date)
                throw new ArgumentException("La fecha desde no puede ser mayor que la fecha hasta.");

            var proveedor = await _repository.ObtenerPorId(idProveedor);

            if (proveedor is null)
                throw new InvalidOperationException("Proveedor no encontrado.");

            var compras = await _repository.ObtenerComprasCuentaCorriente(idProveedor, desde, hasta);
            var creditosProveedor = (await _creditoProveedorRepository.ObtenerPorProveedor(idProveedor)).ToList();

            var formasDePagoBd = await _formasDePagoRepository.ObtenerTodas();

            var comprasPendientes = compras
                .Where(c => (int)c.Estado != 2 && c.SaldoPendiente > 0)
                .Select(c => new ProveedorCompraPendienteDto
                {
                    IdCompra = c.Id,
                    Fecha = c.Fecha,
                    Comprobante = c.NumeroComprobante,
                    Total = c.Total,
                    Pagado = c.TotalPagado,
                    CreditoAplicado = c.CreditoAplicado,
                    SaldoPendiente = c.SaldoPendiente
                })
                .OrderBy(c => c.Fecha)
                .ToList();

            var movimientos = new List<ProveedorMovimientoDto>();

            foreach (var compra in compras.OrderBy(c => c.Fecha))
            {
                movimientos.Add(new ProveedorMovimientoDto
                {
                    Tipo = "Compra",
                    IdCompra = compra.Id,
                    Fecha = compra.Fecha,
                    Comprobante = compra.NumeroComprobante,
                    Importe = compra.Total,
                    TotalCompra = compra.Total,
                    PagadoCompra = compra.TotalPagado,
                    CreditoAplicadoCompra = compra.CreditoAplicado,
                    SaldoPendienteCompra = compra.SaldoPendiente
                });

                if (compra.CreditoAplicado > 0)
                {
                    movimientos.Add(new ProveedorMovimientoDto
                    {
                        Tipo = "AplicacionCredito",
                        IdCompra = compra.Id,
                        Fecha = compra.Fecha,
                        Comprobante = compra.NumeroComprobante,
                        Referencia = "Aplicacion de saldo a favor en compra",
                        Importe = -compra.CreditoAplicado
                    });
                }

                if (compra.Pagos is not null)
                {
                    foreach (var pago in compra.Pagos.OrderBy(p => p.FechaPago))
                    {
                        movimientos.Add(new ProveedorMovimientoDto
                        {
                            Tipo = "Pago",
                            IdCompra = compra.Id,
                            IdPago = pago.Id,
                            Fecha = pago.FechaPago,
                            Comprobante = compra.NumeroComprobante,
                            FormaPago = formasDePagoBd.FirstOrDefault(f => f.Id == pago.IdFormaPago)?.Nombre,
                            Referencia = pago.Referencia,
                            Importe = pago.Importe
                        });
                    }
                }
            }

            var creditosEnRango = creditosProveedor
                .Where(c =>
                    (!desde.HasValue || c.Fecha.Date >= desde.Value.Date) &&
                    (!hasta.HasValue || c.Fecha.Date <= hasta.Value.Date))
                .OrderBy(c => c.Fecha);

            foreach (var credito in creditosEnRango)
            {
                movimientos.Add(new ProveedorMovimientoDto
                {
                    Tipo = "Credito",
                    IdDevolucionCompra = credito.IdDevolucionCompra,
                    Fecha = credito.Fecha,
                    Comprobante = $"DEV-{credito.IdDevolucionCompra}",
                    Referencia = "Credito a favor por devolucion/anulacion de compra",
                    Importe = credito.Importe,
                    SaldoCredito = credito.Saldo
                });
            }

            var creditoDisponible = creditosProveedor
                .Where(c => c.Saldo > 0)
                .Sum(c => c.Saldo);

            var saldoTotalPendiente = compras
                .Where(c => (int)c.Estado != 2)
                .Sum(c => c.SaldoPendiente);

            return new ProveedorCuentaCorrienteDto
            {
                IdProveedor = proveedor.Id,
                Proveedor = proveedor.RazonSocial,
                SaldoTotalPendiente = saldoTotalPendiente,
                CreditoDisponible = creditoDisponible,
                SaldoNeto = saldoTotalPendiente - creditoDisponible,
                TotalComprado = compras.Where(c => (int)c.Estado != 2).Sum(c => c.Total),
                TotalPagado = compras.Where(c => (int)c.Estado != 2).Sum(c => c.TotalPagado),
                ComprasPendientes = comprasPendientes,
                Movimientos = movimientos
                    .OrderBy(m => m.Fecha)
                    .ToList()
            };
        }

        public async Task PagarProveedor(int idProveedor, decimal importe, int idFormaPago)
        {
            var compras = (await _comprasRepository.ObtenerPendientesPorProveedor(idProveedor))
                .OrderBy(c => c.Fecha)
                .ToList();

            if (!compras.Any())
                throw new Exception("El proveedor no tiene compras pendientes.");

            decimal restante = importe;

            var creditos = (await _creditoProveedorRepository.ObtenerPorProveedor(idProveedor))
                .Where(c => c.Saldo > 0)
                .OrderBy(c => c.Fecha)
                .ToList();

            foreach (var compra in compras)
            {
                if (compra.SaldoPendiente <= 0)
                    continue;

                decimal saldoCompra = compra.SaldoPendiente;

                // 2️⃣ aplicar créditos primero
                foreach (var credito in creditos)
                {
                    if (saldoCompra <= 0)
                        break;

                    if (credito.Saldo <= 0)
                        continue;

                    var montoCredito = Math.Min(saldoCompra, credito.Saldo);

                    var creditoConsumido = await _creditoProveedorRepository.ConsumirCredito(credito.Id, montoCredito);

                    if (!creditoConsumido)
                        throw new InvalidOperationException("No se pudo aplicar el saldo a favor del proveedor. Verifique el credito disponible.");

                    saldoCompra -= montoCredito;

                    await _pagosRepository.RecalcularTotalPagado(compra.Id);
                }

                // 3️⃣ si aún queda saldo, usar dinero
                if (saldoCompra > 0 && restante > 0)
                {
                    var montoPago = Math.Min(saldoCompra, restante);

                    var pago = new CompraPago
                    {
                        IdCompra = compra.Id,
                        IdFormaPago = idFormaPago,
                        Importe = montoPago,
                        Estado = EstadoComprobante.Activa
                    };

                    await _pagosRepository.Insertar(pago);

                    await _pagosRepository.RecalcularTotalPagado(compra.Id);

                    restante -= montoPago;
                }
            }
        }
    }
}
