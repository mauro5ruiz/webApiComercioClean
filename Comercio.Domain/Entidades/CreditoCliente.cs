using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comercio.Domain.Entidades
{
    public class CreditoCliente
    {
        public int Id { get; set; }

        public int IdCliente { get; set; }

        public int IdDevolucionVenta { get; set; }

        public decimal Importe { get; set; }

        public decimal Saldo { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        public Cliente Cliente { get; set; }

        public DevolucionVenta DevolucionVenta { get; set; }
    }
}
