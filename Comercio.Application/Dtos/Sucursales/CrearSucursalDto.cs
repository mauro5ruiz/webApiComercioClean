

namespace Comercio.Application.Dtos.Sucursales
{
    public class CrearSucursalDto
    {
        public string Nombre { get; set; }
        public string Codigo { get; set; }
        public string Direccion { get; set; }
        public string Localidad { get; set; }
        public string NroTelefono { get; set; }
        public string Email { get; set; }
        public bool Activa { get; set; }
    }
}
