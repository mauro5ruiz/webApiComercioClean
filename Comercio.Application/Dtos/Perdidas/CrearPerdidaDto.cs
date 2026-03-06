

namespace Comercio.Application.Dtos.Perdidas
{
    public class CrearPerdidaDto
    {
        public DateTime Fecha { get; set; }

        public string Motivo { get; set; }

        public string Observacion { get; set; }

        public int IdUsuario { get; set; }

        public int IdEstado { get; set; }

        public List<DetallePerdidaDto> Detalles { get; set; } = new();
    }
}
