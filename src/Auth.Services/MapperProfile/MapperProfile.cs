using Auth.Domain.Entities;
using Auth.Services.Dtos;
using AutoMapper;

namespace Auth.Services.MapperProfile
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<SuperAdmin, Signup>().ReverseMap();
            CreateMap<SuperAdmin, User>().ReverseMap();
        }
    }
}
