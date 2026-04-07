

namespace Comercio.Domain.Entidades
{
    public class FormaDePago
    {
        public int Id { get; set; }
        public required string Nombre { get; set; }
        public int CantidadCompras { get; set; }
        public int CantidadVentas { get; set; }
    }
}
