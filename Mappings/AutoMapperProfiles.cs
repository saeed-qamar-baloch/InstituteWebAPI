using AutoMapper;
using InstituteWebAPI.Models.DTO.Admissions;
using InstituteWebAPI.Models.DTO.Classes;
using InstituteWebAPI.Models.DTO.ClassStudents;
using InstituteWebAPI.Models.DTO.Courses;
using InstituteWebAPI.Models.DTO.CurrentClasses;
using InstituteWebAPI.Models.DTO.FeeManagement;
using InstituteWebAPI.Models.DTO.FeeType;
using InstituteWebAPI.Models.DTO.Section;
using InstituteWebAPI.Models.DTO.Sessions;
using InstituteWebAPI.Models.DTO.Slots;
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
    public class AutoMapperProfiles : Profile
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

            // Slots (renamed from Sections)
            CreateMap<Slots, SlotsDto>().ReverseMap();
            CreateMap<AddSlotsDto, Slots>().ReverseMap();
            CreateMap<SlotsUpdateDto, Slots>().ReverseMap();

            // Section (simple: SectionID, Name, CurrentClassID)
            CreateMap<Section, SectionDto>().ReverseMap();
            CreateMap<AddSectionDto, Section>().ReverseMap();
            CreateMap<UpdateSectionDto, Section>().ReverseMap();

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

            CreateMap<AddFeeTypeDto, InstituteWebApp.Models.Domain.FeeType>().ReverseMap();
            CreateMap<UpdateFeeTypeDto, InstituteWebApp.Models.Domain.FeeType>().ReverseMap();
            CreateMap<InstituteWebApp.Models.Domain.FeeType, InstituteWebAPI.Models.DTO.FeeType.FeeTypeDto>().ReverseMap();
            CreateMap<AddAdmissionDto, Admissions>().ReverseMap();
            CreateMap<UpdateAdmissionDto, Admissions>().ReverseMap();
            CreateMap<Admissions, AdmissionDto>().ReverseMap();

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

            CreateMap<FeeDue, FeeDueDto>()
                .ForMember(d => d.FeeType, opt => opt.MapFrom(s => s.FeeType.ToString()))
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.TotalAmount, opt => opt.MapFrom(s => s.BaseAmount + (s.IsLateFeeWaived ? 0m : s.LateFeeAmount)))
                .ForMember(d => d.PaidAmount, opt => opt.MapFrom(s => s.PaymentDetails.Sum(p => p.PaidAmount)))
                .ForMember(d => d.RemainingAmount, opt => opt.MapFrom(s => (s.BaseAmount + (s.IsLateFeeWaived ? 0m : s.LateFeeAmount)) - s.PaymentDetails.Sum(p => p.PaidAmount)));
            CreateMap<Payment, PaymentDto>();
            CreateMap<PaymentDetail, PaymentDetailDto>();
            CreateMap<Payment, PaymentSummaryDto>()
                .ForMember(d => d.StudentId, opt => opt.MapFrom(s => s.StudentId))
                .ForMember(d => d.RegistrationNo, opt => opt.MapFrom(s => s.Student.RegistrationNo))
                .ForMember(d => d.StudentName, opt => opt.MapFrom(s => s.Student.StudentName))
                .ForMember(d => d.FatherName, opt => opt.MapFrom(s => s.Student.FatherName));
        }
    }
}
