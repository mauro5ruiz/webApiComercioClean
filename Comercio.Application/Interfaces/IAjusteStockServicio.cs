using Comercio.Application.Dtos.AjustesStock;

namespace Comercio.Application.Interfaces
{
    public interface IAjusteStockServicio
    {
        Task<IEnumerable<AjusteStockLecturaDto>> ObtenerTodas();
        Task AjustarStock(int idProducto, int stockReal, string motivo);
    }
}
