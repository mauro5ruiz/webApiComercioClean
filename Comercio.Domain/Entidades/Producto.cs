using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comercio.Domain.Entidades
{
    public class Producto
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = null!;

        public string? Descripcion { get; set; }

        public string? Codigo { get; set; }

        public string? CodigoBarra { get; set; }

        public int IdCategoria { get; set; }
        public int IdMarca { get; set; }

        public decimal PrecioCompra { get; set; }
        public decimal PrecioVenta { get; set; }

        public int StockMinimo { get; set; }
        public bool ControlStock { get; set; }
        public int StockActual { get; set; }

        public string? UrlImagen { get; set; }

        public bool Activo { get; set; }

        public DateTime FechaAlta { get; set; }
        public DateTime? FechaBaja { get; set; }
    }
}
