namespace InstituteWebAPI.Models.DTO.StudentMarks
{
    public class MonthlyResultDto
    {
        public Guid CurrentClassID { get; set; }
        public Guid TermMonthID { get; set; }
        public float PassingMarks { get; set; }
        public float TotalMarks { get; set; }
        public List<MonthlyTestHeaderDto> Tests { get; set; } = new();
        public List<MonthlyStudentResultRowDto> Students { get; set; } = new();
    }

    public class MonthlyTestHeaderDto
    {
        public Guid TestID { get; set; }
        public string TestType { get; set; }
        public float TotalMarks { get; set; }
    }

    public class MonthlyStudentResultRowDto
    {
        public Guid StudentID { get; set; }
        public string? RegistrationNo { get; set; }
        public string? StudentName { get; set; }

        // Key = TestID
        public Dictionary<Guid, float?> MarksByTest { get; set; } = new();

        public float ObtainedTotal { get; set; }

        // New fields
        public float Percentage { get; set; }
        public float? AccumulatedPercentage { get; set; }

        public string Grade { get; set; }

        // Result could be Pass/Fail/1st/2nd/3rd
        public string Result { get; set; }

        // If promotion overrides fail, set this to "Promoted" and Result should be "Promoted"
        public string? ModifiedResult { get; set; }
    }
}
