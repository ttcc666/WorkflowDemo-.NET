using AutoMapper;
using WorkFlowDemo.Models.Dtos;
using WorkFlowDemo.Models.Entities;

namespace WorkFlowDemo.Api.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User mappings
            CreateMap<User, UserDto>();
            CreateMap<CreateUserDto, User>();
            CreateMap<UpdateUserDto, User>();

            // Material mappings
            CreateMap<Material, MaterialDto>();
            CreateMap<CreateMaterialDto, Material>();
            CreateMap<UpdateMaterialDto, Material>();
            CreateMap<CreateMaterialDto, MaterialTemporaryScan>();
            CreateMap<AddMaterialDto, Material>();
            CreateMap<AddInventoryDto, MaterialInventory>();
            CreateMap<ScanDto, MaterialTemporaryScan>();
        }
    }
}