namespace InstituteWebAPI.Models.DTO.StudentMarks.Terminal
{
    public class TerminalResultDto
    {
        public Guid CurrentClassID { get; set; }
        public Guid TermID { get; set; }

        public Guid Month1TermMonthID { get; set; }
        public string Month1Label { get; set; } = "1st";
        public float Month1TotalMarks { get; set; }

        public Guid Month2TermMonthID { get; set; }
        public string Month2Label { get; set; } = "2nd";
        public float Month2TotalMarks { get; set; }

        public Guid Month3TermMonthID { get; set; }
        public string Month3Label { get; set; } = "3rd";
        public float Month3TotalMarks { get; set; }

        public List<TerminalStudentRowDto> Students { get; set; } = new();
    }

    public class TerminalStudentRowDto
    {
        public Guid StudentID { get; set; }
        public string? RegistrationNo { get; set; }
        public string? StudentName { get; set; }

        public float? Month1Obtained { get; set; }
        public float? Month1TotalMarks { get; set; }

        public float? Month2Obtained { get; set; }
        public float? Month2TotalMarks { get; set; }

        public float? Month3Obtained { get; set; }
        public float? Month3TotalMarks { get; set; }

        public bool IncludeMonth1 { get; set; }
        public bool IncludeMonth2 { get; set; }

        public float TotalObtained { get; set; }
        public float TotalMarksConsidered { get; set; }
        public float Percentage { get; set; }

        public string Grade { get; set; } = "";

        // Pass/Fail/1st/2nd/3rd/Promoted
        public string Result { get; set; } = "";

        // True when an admin manually overrode the result.
        public bool IsResultManual { get; set; }

        // Flags for UI display
        public bool NI_Month1 { get; set; }
        public bool NI_Month2 { get; set; }
        public bool NI_Month3 { get; set; }
    }
}
