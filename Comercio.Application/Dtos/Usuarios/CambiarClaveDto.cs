

namespace Comercio.Application.Dtos.Usuarios
{
    public class CambiarClaveDto
    {
        public int UsuarioId { get; set; }
        public string ClaveActual { get; set; } = string.Empty;
        public string NuevaClave { get; set; } = string.Empty;
    }
}
