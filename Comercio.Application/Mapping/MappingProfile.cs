using AutoMapper;
using Comercio.Application.Dtos.Clientes;
using Comercio.Application.Dtos.Productos;
using Comercio.Application.Dtos.Proveedores;
using Comercio.Domain.Entidades;

namespace Comercio.Application.Mapping
{
    public class MappingProfile: Profile
    {
        public MappingProfile()
        {
            //Proveedores
            CreateMap<CrearProveedorDto, Proveedor>();
            CreateMap<ActualizarProveedorDto, Proveedor>();

            //Clientes
            CreateMap<CrearClienteDto, Cliente>();
            CreateMap<ActualizarClienteDto, Cliente>();

            //Productos
            CreateMap<CrearProductoDto, Producto>();
            CreateMap<ActualizarProductoDto, Producto>();
        }
    }
}
