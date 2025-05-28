using AutoMapper;
using InstituteWebAPI.Models.DTO.Admissions;
using InstituteWebAPI.Models.DTO.Classes;
using InstituteWebAPI.Models.DTO.ClassStudents;
using InstituteWebAPI.Models.DTO.Courses;
using InstituteWebAPI.Models.DTO.CurrentClasses;
using InstituteWebAPI.Models.DTO.Sections;
using InstituteWebAPI.Models.DTO.Sessions;
using InstituteWebAPI.Models.DTO.StudentMarks;
using InstituteWebAPI.Models.DTO.Students;
using InstituteWebAPI.Models.DTO.TeacherCourse;
using InstituteWebAPI.Models.DTO.TermMonths;
using InstituteWebAPI.Models.DTO.Terms;
using InstituteWebAPI.Models.DTO.Tests;
using InstituteWebAPI.Models.DTO.Villages;
using InstituteWebApp.Models.Domain;
using static System.Net.Mime.MediaTypeNames;

namespace InstituteWebAPI.Mappings
{
    public class AutoMapperProfiles:Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Term, TermDto>().ReverseMap();
            CreateMap<AddTermDto, Term>().ReverseMap();
            CreateMap<CourseDto, Courses>().ReverseMap();
            CreateMap<AddCourseDto, Courses>().ReverseMap();
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

            CreateMap<AddAdmissionDto, Admissions>();
            CreateMap<UpdateAdmissionDto, Admissions>();
            CreateMap<Admissions, AdmissionDto>();

            CreateMap<AddTestDto, Tests>();
            CreateMap<UpdateTestDto, Tests>();
            CreateMap<Tests, TestDto>();

            CreateMap<AddCurrentClassDto, CurrentClass>();
            CreateMap<UpdateCurrentClassDto, CurrentClass>();
            CreateMap<CurrentClass, CurrentClassDto>();

            CreateMap<AddStudentMarksDto, StudentMarks>();
            CreateMap<UpdateStudentMarksDto, StudentMarks>();
            CreateMap<StudentMarks, StudentMarksDto>();


            CreateMap<AddClassStudentDto, ClassStudents>();
            CreateMap<UpdateClassStudentDto, ClassStudents>();
            CreateMap<ClassStudents, ClassStudentDto>();
        }
    }
}
