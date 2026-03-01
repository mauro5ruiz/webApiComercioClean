using Comercio.Application.Interfaces;
using Comercio.Domain.Entidades;
using Comercio.Domain.Enums;
using Comercio.Domain.Interfaces;

namespace Comercio.Application.Servicios
{
    public class OfertasServicio : IOfertasServicio
    {
        private readonly IOfertasRepository _repository;
        private readonly IProductosRepository _productosRepository;
        public OfertasServicio(IOfertasRepository ofertasRepository, IProductosRepository productosRepository)
        {
            _repository = ofertasRepository;
            _productosRepository = productosRepository;
        }

        public async Task<IEnumerable<Oferta>> ObtenerTodas(bool incluirVencidas = false)
        {
            return await _repository.ObtenerTodas(incluirVencidas);
        }

        public async Task<Oferta?> ObtenerPorId(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Id incorrecto.");

            return await _repository.ObtenerPorId(id);
        }

        public async Task<Oferta?> ObtenerOfertaActivaVigente(int productoId)
        {
            if (productoId <= 0)
                throw new ArgumentException("Id incorrecto.");

            Producto? producto = await _productosRepository.ObtenerPorId(productoId);
            if (producto == null || producto.Id <= 0)
                throw new ArgumentException("Producto no encontrado");

            return await _repository.ObtenerOfertaActivaVigente(productoId);
        }

        public async Task<int> CrearOferta(Oferta oferta)
        {
            ValidarOferta(oferta);

            await ValidarSuperposicion(oferta);

            await _repository.DesactivarOfertasPorProducto(oferta.IdProducto);

            oferta.Activa = true;

            return await _repository.Crear(oferta);
        }

        public async Task ActualizarOferta(Oferta oferta)
        {
            ValidarOferta(oferta);

            await ValidarSuperposicion(oferta);

            await _repository.Actualizar(oferta);
        }

        public decimal CalcularPrecioFinal(decimal precioBase, Oferta? oferta)
        {
            if (oferta == null)
                return precioBase;

            if (!oferta.Activa)
                return precioBase;

            if (DateTime.Now < oferta.FechaInicio || DateTime.Now > oferta.FechaFin)
                return precioBase;

            decimal precioFinal = precioBase;

            switch (oferta.TipoDescuento)
            {
                case TipoDescuento.Porcentaje: 
                    if (oferta.ValorDescuento > 100)
                        throw new Exception("El porcentaje no puede ser mayor a 100.");

                    precioFinal = precioBase - (precioBase * (oferta.ValorDescuento / 100m));
                    break;

                case TipoDescuento.Fijo: 
                    precioFinal = precioBase - oferta.ValorDescuento;
                    break;

                case TipoDescuento.PrecioFinal:
                    precioFinal = oferta.ValorDescuento;
                    break;

                default:
                    throw new Exception("Tipo de descuento inválido.");
            }

            return precioFinal < 0 ? 0 : precioFinal;
        }

        public async Task DesactivarOfertasPorProducto(int productoId)
        {
            await _repository.DesactivarOfertasPorProducto(productoId);
        }

        private void ValidarOferta(Oferta oferta)
        {
            if (oferta.FechaFin < oferta.FechaInicio)
                throw new Exception("La fecha fin no puede ser menor a la fecha inicio.");

            if (oferta.ValorDescuento <= 0)
                throw new Exception("El valor del descuento debe ser mayor a cero.");

            if (oferta.TipoDescuento == TipoDescuento.Porcentaje && oferta.ValorDescuento > 100)
                throw new Exception("El porcentaje no puede ser mayor a 100.");
        }

        private async Task ValidarSuperposicion(Oferta nuevaOferta)
        {
            var ofertasExistentes = await _repository.ObtenerPorProducto(nuevaOferta.IdProducto, incluirVencidas: true);

            foreach (var oferta in ofertasExistentes)
            {
                if (oferta.Id == nuevaOferta.Id)
                    continue;

                bool seSuperpone = nuevaOferta.FechaInicio <= oferta.FechaFin && nuevaOferta.FechaFin >= oferta.FechaInicio;

                if (seSuperpone && oferta.Activa)
                    throw new Exception("Ya existe una oferta activa que se superpone en fechas.");
            }
        }
    }
}
