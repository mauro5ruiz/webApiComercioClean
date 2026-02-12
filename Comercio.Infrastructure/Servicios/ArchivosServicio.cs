using Comercio.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Comercio.Infrastructure.Servicios
{
    public class ArchivosServicio : IArchivosServicio
    {
        private readonly IWebHostEnvironment _env;

        public ArchivosServicio(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string?> GuardarImagen(IFormFile? archivo, string carpeta, string? rutaActual = null)
        {
            if (archivo == null || archivo.Length == 0)
                return rutaActual;

            var carpetaFisica = Path.Combine(
                _env.WebRootPath,
                "images",
                carpeta
            );

            Directory.CreateDirectory(carpetaFisica);

            if (!string.IsNullOrEmpty(rutaActual))
                EliminarImagen(rutaActual);

            var extension = Path.GetExtension(archivo.FileName);
            var nombreArchivo = $"{Guid.NewGuid()}{extension}";

            var rutaCompleta = Path.Combine(carpetaFisica, nombreArchivo);

            using var stream = new FileStream(rutaCompleta, FileMode.Create);
            await archivo.CopyToAsync(stream);

            return $"/images/{carpeta}/{nombreArchivo}";
        }

        public void EliminarImagen(string ruta)
        {
            var rutaFisica = Path.Combine(_env.WebRootPath, ruta.TrimStart('/'));

            if (File.Exists(rutaFisica))
                File.Delete(rutaFisica);
        }
    }
}
