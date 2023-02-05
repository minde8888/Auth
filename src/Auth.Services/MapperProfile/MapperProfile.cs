using Auth.Domain.Entities;
using AutoMapper;

namespace Auth.Services.MapperProfile
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<SuperAdmin, Signup>().ReverseMap();
            CreateMap<SuperAdmin, UserResponse>().ReverseMap();
        }
    }
}
