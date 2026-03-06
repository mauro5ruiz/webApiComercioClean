

namespace Comercio.Domain.Entidades
{
    public class Perdida
    {
        public int Id { get; set; }

        public DateTime Fecha { get; set; }

        public string Motivo { get; set; }

        public string Observacion { get; set; }

        public int IdUsuario { get; set; }

        public int IdEstado { get; set; }

        public List<DetallePerdida> Detalles { get; set; } = new List<DetallePerdida>();
    }
}
