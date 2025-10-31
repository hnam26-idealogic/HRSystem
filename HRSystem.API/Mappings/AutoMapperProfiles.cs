using AutoMapper;
using HRSystem.API.Models.Domain;
using HRSystem.API.Models.DTO;

namespace HRSystem.API.Mappings
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            // User mappings
            CreateMap<User, UserDto>().ReverseMap();
            CreateMap<AddUserRequestDto, User>();
            CreateMap<UpdateUserRequestDto, User>();

            // Candidate mappings
            CreateMap<Candidate, CandidateDto>().ReverseMap();
            CreateMap<AddCandidateRequestDto, Candidate>();
            CreateMap<UpdateCandidateRequestDto, Candidate>();

            // Interview mappings
            CreateMap<Interview, InterviewDto>().ReverseMap();
            CreateMap<AddInterviewRequestDto, Interview>();
            CreateMap<UpdateInterviewRequestDto, Interview>();
        }
    }
}
