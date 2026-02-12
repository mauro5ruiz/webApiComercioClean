using Microsoft.AspNetCore.Http;

namespace Comercio.Application.Interfaces
{
    public interface IArchivosServicio
    {
        Task<string?> GuardarImagen(IFormFile? archivo, string carpeta, string? rutaActual = null);

        void EliminarImagen(string ruta);
    }
}
