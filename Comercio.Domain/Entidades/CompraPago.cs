using Comercio.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comercio.Domain.Entidades
{
    public class CompraPago
    {
        public int Id { get; set; }
        public int IdCompra { get; set; }
        public int IdFormaPago { get; set; }

        public decimal Importe { get; set; }
        public int? Cuotas { get; set; }
        public string? Referencia { get; set; }
        public DateTime FechaPago { get; set; }

        public EstadoComprobante Estado { get; set; } = EstadoComprobante.Activa;
    }
}
