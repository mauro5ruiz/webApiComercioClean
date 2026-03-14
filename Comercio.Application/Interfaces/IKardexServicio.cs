using Comercio.Domain.Entidades;

namespace Comercio.Application.Interfaces
{
    public interface IKardexServicio
    {
        Task<IEnumerable<MovimientoStock>> ObtenerKardex(int idProducto);
    }
}
