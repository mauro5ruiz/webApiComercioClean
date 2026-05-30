namespace Comercio.Application.Dtos.Clientes
{
    public class CobrarClienteDto
    {
        public int IdCliente { get; set; }
        public decimal Importe { get; set; }
        public int IdFormaPago { get; set; }
        public string Referencia { get; set; } = string.Empty;
    }
}
