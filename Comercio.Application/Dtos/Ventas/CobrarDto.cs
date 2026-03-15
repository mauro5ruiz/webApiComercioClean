using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comercio.Application.Dtos.Ventas
{
    public class CobrarDto
    {
        public int IdCliente { get; set; }
        public decimal Importe { get; set; }
        public int IdFormaPago { get; set; }
        public string Referencia { get; set; }
    }
}
