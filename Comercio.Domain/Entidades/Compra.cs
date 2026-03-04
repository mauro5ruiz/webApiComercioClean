using Comercio.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comercio.Domain.Entidades
{
    public class Compra
    {
        public int Id { get; set; }
        public string NumeroComprobante { get; set; } = null!;
        public DateTime Fecha { get; set; }

        public int IdProveedor { get; set; }
        public int IdSucursal { get; set; }

        public decimal Total { get; set; }
        public decimal TotalPagado { get; set; }
        public decimal SaldoPendiente { get; set; }

        public EstadoComprobante Estado { get; set; } = EstadoComprobante.Activa;
        public string? Observaciones { get; set; }
        public DateTime? FechaAnulacion { get; set; }

        public List<DetalleCompra> Detalles { get; set; } = new();
        public List<CompraPago> Pagos { get; set; } = new();
    }
}
