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
