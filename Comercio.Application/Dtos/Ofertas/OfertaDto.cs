using Comercio.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comercio.Application.Dtos.Ofertas
{
    public class OfertaDto
    {
        public int Id { get; set; }
        public int IdProducto { get; set; }
        public TipoDescuento TipoDescuento { get; set; }
        public decimal ValorDescuento { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public bool Activa { get; set; }
    }
}
