using AutoMapper;
using Comercio.Application.Dtos.Clientes;
using Comercio.Application.Dtos.Productos;
using Comercio.Application.Dtos.Proveedores;
using Comercio.Application.Dtos.Usuarios;
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

            //Usuarios
            CreateMap<CrearUsuarioDto, Usuario>();

            CreateMap<ActualizarUsuarioDto, Usuario>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ClaveHash, opt => opt.Ignore())
                .ForMember(dest => dest.FechaCreacion, opt => opt.Ignore())
                .ForMember(dest => dest.UltimoAcceso, opt => opt.Ignore())
                .ForMember(dest => dest.NombreCompleto, opt => opt.Ignore());
        }
    }
}
