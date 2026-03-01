using Comercio.Domain.Enums;

namespace Comercio.Domain.Entidades
{
    public class Oferta
    {
        public int Id { get; set; }

        public int IdProducto { get; set; }

        public TipoDescuento TipoDescuento { get; set; }

        public decimal ValorDescuento { get; set; }

        public DateTime FechaInicio { get; set; }

        public DateTime FechaFin { get; set; }

        public bool Activa { get; set; }
    }
}
