

namespace Comercio.Domain.Entidades
{
    public class MedioDePago
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public int IdFormaDePago { get; set; }

        public bool PermiteCuotas { get; set; }
        public bool RequiereReferencia { get; set; }
        public bool EsCuentaCorriente { get; set; }
        public bool Activa { get; set; }

        public DateTime FechaCreacion { get; set; }
    }
}
