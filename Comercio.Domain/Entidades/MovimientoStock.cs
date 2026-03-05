using Comercio.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Comercio.Domain.Entidades
{
    public class MovimientoStock
    {
        public int Id { get; set; }
        public int IdProducto { get; set; }
        public TipoMovimientoStock IdTipoMovimientoStock { get; set; }
        public int Cantidad { get; set; }
        public int IdReferencia { get; set; }
        public DateTime Fecha { get; set; }
        public string Observaciones { get; set; }
    }
}
