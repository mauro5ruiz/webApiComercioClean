namespace Comercio.Application.Dtos.Dashboard
{
    public class DashboardDto
    {
        public DashboardCardsDto Cards { get; set; } = new();
        public DashboardModulosDto Modulos { get; set; } = new();
        public DashboardAlertasDto Alertas { get; set; } = new();
        public DashboardResumenOperativoDto ResumenOperativo { get; set; } = new();
        public List<DashboardUltimaVentaDto> UltimasVentas { get; set; } = new();
    }

    public class DashboardCardsDto
    {
        public decimal VentasMesActual { get; set; }
        public decimal VentasMesAnterior { get; set; }
        public decimal DevolucionesVentasMesActual { get; set; }
        public decimal DevolucionesVentasMesAnterior { get; set; }
        public decimal ComprasMesActual { get; set; }
        public decimal ComprasMesAnterior { get; set; }
        public decimal DevolucionesComprasMesActual { get; set; }
        public decimal DevolucionesComprasMesAnterior { get; set; }
        public decimal PerdidasMesActual { get; set; }
        public decimal PerdidasMesAnterior { get; set; }
    }

    public class DashboardModulosDto
    {
        public int Productos { get; set; }
        public int Categorias { get; set; }
        public int Marcas { get; set; }
        public int OfertasActivas { get; set; }
        public int Perdidas { get; set; }
        public int Ventas { get; set; }
        public int DevolucionesVentas { get; set; }
        public int Compras { get; set; }
        public int DevolucionesCompras { get; set; }
        public int Clientes { get; set; }
        public int Proveedores { get; set; }
        public int Vendedores { get; set; }
    }

    public class DashboardAlertasDto
    {
        public int ProductosBajoStock { get; set; }
        public int OfertasPorVencer { get; set; }
        public int PerdidasPendientes { get; set; }
    }

    public class DashboardResumenOperativoDto
    {
        public int ProductosActivos { get; set; }
        public int ProductosInactivos { get; set; }
        public int ClientesActivos { get; set; }
        public int ProveedoresActivos { get; set; }
        public int VendedoresActivos { get; set; }
        public int OfertasActivas { get; set; }
    }

    public class DashboardUltimaVentaDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public string Vendedor { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}
