using AutoMapper;
using HRSystem.API.Models.Domain;
using HRSystem.API.Models.DTO;

namespace HRSystem.API.Mappings
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<User, UserDto>().ReverseMap();
            CreateMap<AddUserDto, User>();
        }
    }
}
