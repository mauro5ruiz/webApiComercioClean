

namespace Comercio.Domain.Entidades
{
    public class Categoria
    {
        public int Id { get; set; }
        public required string Nombre { get; set; }
        public int CantidadProductos { get; set; }
    }
}
