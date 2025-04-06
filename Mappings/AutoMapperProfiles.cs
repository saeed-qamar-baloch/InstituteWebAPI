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



            CreateMap<AddClassesDto, Classes>().ReverseMap();
            CreateMap<Classes, ClassesDto>().ReverseMap();
            CreateMap<ClassUpdateRequestDto, Classes>().ReverseMap();

            CreateMap<Sections, SectionsDto>().ReverseMap();
            CreateMap<AddSectionsDto, Sections>().ReverseMap();
            CreateMap<SectionsUpdateDto, Sections>().ReverseMap();

            CreateMap<Sessions, SessionsDto>().ReverseMap();
            CreateMap<Sessions, AddSessionsDto>().ReverseMap();
            CreateMap<Sessions, SessionUpdateDto>().ReverseMap();

            CreateMap<AddTeacherDto, Teachers>();
            CreateMap<UpdateTeacherDto, Teachers>();
            CreateMap<Teachers, TeacherDto>();


            CreateMap<AddTeacherCourseDto, TeacherCourses>();
            CreateMap<UpdateTeacherCourseDto, TeacherCourses>();
            CreateMap<TeacherCourses, TeacherCourseDto>();

            CreateMap<Students, AddStudentDto>().ReverseMap();
            CreateMap<Students, StudentDto>().ReverseMap();
            CreateMap<Students, UpdateStudentDto>().ReverseMap();
        }
    }
}
