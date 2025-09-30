using AutoMapper;
using SmartCVFilter.API.DTOs;
using SmartCVFilter.API.Models;

namespace SmartCVFilter.API.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<ApplicationUser, UserInfo>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.CompanyName));

        // Job Post mappings
        CreateMap<CreateJobPostRequest, JobPost>();
        CreateMap<UpdateJobPostRequest, JobPost>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Applicant mappings
        CreateMap<CreateApplicantRequest, Applicant>();
        CreateMap<UpdateApplicantRequest, Applicant>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // CV File mappings
        CreateMap<CVFile, CVFileResponse>();

        // Screening Result mappings
        CreateMap<ScreeningResult, ScreeningResultResponse>()
            .ForMember(dest => dest.Strengths, opt => opt.MapFrom(src => DeserializeStringList(src.Strengths)))
            .ForMember(dest => dest.Weaknesses, opt => opt.MapFrom(src => DeserializeStringList(src.Weaknesses)));
    }

    private static List<string> DeserializeStringList(string json)
    {
        if (string.IsNullOrEmpty(json))
            return new List<string>();

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }
}

