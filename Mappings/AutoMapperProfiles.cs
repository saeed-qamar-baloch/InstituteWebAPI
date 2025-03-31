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
        }
    }
}
