

namespace Comercio.Application.Interfaces
{
    public interface IAjusteStockServicio
    {
        Task AjustarStock(int idProducto, int stockReal, string motivo);
    }
}
