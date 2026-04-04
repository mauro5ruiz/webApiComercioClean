namespace Comercio.Application.Dtos.Categorias
{
    public class CategoriaDto
    {
        public int Id { get; set; }
        public required string Nombre { get; set; }
        public int CantidadProductos { get; set; }
    }
}
