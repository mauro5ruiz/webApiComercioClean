using AutoMapper;
using Comercio.Application.Dtos.Proveedores;
using Comercio.Domain.Entidades;

namespace Comercio.Application.Mapping
{
    public class MappingProfile: Profile
    {
        public MappingProfile()
        {
            CreateMap<CrearProveedorDto, Proveedor>();
            CreateMap<ActualizarProveedorDto, Proveedor>();
        }
    }
}
