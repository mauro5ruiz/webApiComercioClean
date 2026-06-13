namespace Comercio.Domain.Entidades
{
    public class NotaCredito
    {
        public int Id { get; set; }
        public int IdDevolucionVenta { get; set; }
        public string Codigo { get; set; } = null!;
        public decimal Importe { get; set; }
        public decimal Saldo { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
        public DateTime? FechaVencimiento { get; set; }
        public string Estado { get; set; } = "Activa";
        public string? Observaciones { get; set; }
        public DevolucionVenta DevolucionVenta { get; set; } = null!;
    }
}
