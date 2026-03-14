using AutoMapper;
using Comercio.Application.Interfaces;
using Comercio.Application.Mapping;
using Comercio.Application.Servicios;
using Comercio.Domain.Interfaces;
using Comercio.Infrastructure.Repositorios;
using Comercio.Infrastructure.Servicios; // <-- AÑADIR

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("Falta la connection string 'DefaultConnection' en appsettings.json.");

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(MappingProfile));

// ===== Servicios (Application) =====
builder.Services.AddScoped<ICategoriasServicio, CategoriasServicio>();
builder.Services.AddScoped<IMarcasServicio, MarcasServicio>();
builder.Services.AddScoped<IFormasDePagoServicio, FormasDePagoServicio>();
builder.Services.AddScoped<ISucursalesServicio, SucursalesServicio>();
builder.Services.AddScoped<IProveedoresServicio, ProveedoresServicio>();
builder.Services.AddScoped<IClientesServicio, ClientesServicio>();
builder.Services.AddScoped<IProductosServicio, ProductosServicio>();
builder.Services.AddScoped<IUsuariosServicio, UsuariosServicio>();
builder.Services.AddScoped<IVendedoresServicio, VendedoresServicio>();
builder.Services.AddScoped<IOfertasServicio, OfertasServicio>();
builder.Services.AddScoped<IVentasServicio, VentasServicio>();
builder.Services.AddScoped<IComprasServicio, ComprasServicio>();
builder.Services.AddScoped<IPerdidasServicio, PerdidasServicio>();
builder.Services.AddScoped<IDevolucionesVentasServicio, DevolucionesVentasServicio>();
builder.Services.AddScoped<IDevolucionesComprasServicio, DevolucionesComprasServicio>();
builder.Services.AddScoped<IAjusteStockServicio, AjusteStockServicio>();
builder.Services.AddScoped<IKardexServicio, KardexServicio>();

// ===== Servicios (Infra) =====
builder.Services.AddScoped<IArchivosServicio, ArchivosServicio>(); // <-- AÑADIR

// ===== Repositorios (Infrastructure) =====
builder.Services.AddScoped<ICategoriasRepository>(_ => new CategoriasRepositorio(connectionString));
builder.Services.AddScoped<IMarcasRepository>(_ => new MarcasRepositorio(connectionString));
builder.Services.AddScoped<IFormasDePagoRepository>(_ => new FormasDePagoRepositorio(connectionString));
builder.Services.AddScoped<ISucursalesRepository>(_ => new SucursalesRepositorio(connectionString));
builder.Services.AddScoped<IProveedoresRepository>(_ => new ProveedoresRepositorio(connectionString));
builder.Services.AddScoped<IClientesRepository>(_ => new ClientesRepostorio(connectionString));
builder.Services.AddScoped<IProductosRepository>(_ => new ProductosRepositorio(connectionString));
builder.Services.AddScoped<IUsuariosRepository>(_ => new UsuariosRepositorio(connectionString));
builder.Services.AddScoped<IVendedoresRepository>(_ => new VendedoresRepositorio(connectionString));
builder.Services.AddScoped<IOfertasRepository>(_ => new OfertasRepositorio(connectionString));
builder.Services.AddScoped<IVentasRepository>(_ => new VentasRepositorio(connectionString));
builder.Services.AddScoped<IDetalleVentasRepostory>(_ => new DetalleVentasRepositorio(connectionString));
builder.Services.AddScoped<IVentasPagosRepository>(_ => new VentasPagosRepositorio(connectionString));
builder.Services.AddScoped<IComprasRepostory>(_ => new ComprasRepositorio(connectionString));
builder.Services.AddScoped<IDetalleComprasRepository>(_ => new DetalleComprasRepositorio(connectionString));
builder.Services.AddScoped<IComprasPagosRepository>(_ => new ComprasPagosRepositorio(connectionString));
builder.Services.AddScoped<IMovimientosStockRepository>(_ => new MovimientosStockRepositorio(connectionString));
builder.Services.AddScoped<IPerdidasRepository>(_ => new PerdidasRepositorio(connectionString));
builder.Services.AddScoped<IDetallePerdidasRepository>(_ => new DetallePerdidasRepositorio(connectionString));
builder.Services.AddScoped<IDevolucionesVentasRepository>(_ => new DevolucionesVentasRepositorio(connectionString));
builder.Services.AddScoped<IDetalleDevolucionesVentasRepository>(_ => new DetalleDevolucionesVentasRepositorio(connectionString));
builder.Services.AddScoped<IDevolucionPagosRepository>(_ => new DevolucionVentaPagosRepositorio(connectionString));
builder.Services.AddScoped<IDevolucionComprasRepository>(_ => new DevolucionComprasRepositorio(connectionString));
builder.Services.AddScoped<IDevolucionCompraDetalleRepository>(_ => new DetalleDevolucionesComprasRepositorio(connectionString));
builder.Services.AddScoped<IPagoDevolucionCompraRepository>(_ => new DevolucionCompraPagosRepositorio(connectionString));
builder.Services.AddScoped<ICreditoProveedorRepository>(_ => new CreditoProveedorRepositorio(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseStaticFiles(); // importante para servir /carpeta/archivo desde wwwroot

app.MapControllers();

app.Run();
