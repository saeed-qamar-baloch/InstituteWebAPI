using InstituteWebAPI.Models.DTO.CurrentClasses;
using InstituteWebAPI.Models.DTO.TermMonths;

namespace InstituteWebAPI.Models.DTO.Tests
{
    public class TestDto
    {
        public Guid TestID { get; set; }
        public string TestType { get; set; }
        public float TotalMarks { get; set; }

        public TermMonthsDto TermMonth { get; set; }
        public CurrentClassDto CurrentClass { get; set; }
    }
}
