using AutoMapper;
using InstituteWebAPI.Models.DTO;
using InstituteWebApp.Models.Domain;

namespace InstituteWebAPI.Mappings
{
    public class AutoMapperProfiles:Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Term, TermDto>().ReverseMap();
            CreateMap<AddTermDto, Term>().ReverseMap();
            CreateMap<CourseDto, Courses>().ReverseMap();
            CreateMap<AddTermDto, Courses>().ReverseMap();
            CreateMap<CourseUpdateRequestDto, Courses>().ReverseMap();
            CreateMap<TermUpdateRequestDto, Term>().ReverseMap();
            CreateMap<TermMonths, TermMonthsDto>().ReverseMap();
            CreateMap<AddTermMonthsDto, TermMonths>().ReverseMap();
            CreateMap<TermMonthsUpdateRequestDto, TermMonths>().ReverseMap();
            CreateMap<Village, VillageDto>().ReverseMap();
            CreateMap<AddVillageDto, Village>().ReverseMap();
            CreateMap<VillageUpdateRequestDto, Village>().ReverseMap();

            CreateMap<Sessions, SessionDto>().ReverseMap();
            CreateMap<AddSessionDto, Sessions>().ReverseMap();
            CreateMap<SessionUpdateRequestDto, Sessions>().ReverseMap();
        }
    }
}
