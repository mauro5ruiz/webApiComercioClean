using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comercio.Application.Dtos.Compras
{
    public class PagarProveedorDto
    {
        public int IdProveedor { get; set; }
        public decimal Importe { get; set; }
        public int IdFormaPago { get; set; }
    }
}
