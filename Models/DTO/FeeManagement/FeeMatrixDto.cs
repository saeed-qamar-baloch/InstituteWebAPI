namespace InstituteWebAPI.Models.DTO.FeeManagement
{
    /// <summary>
    /// Returned by GET /api/fee-management/fee-matrix.
    /// Contains the list of month columns and one row per enrolled student.
    /// </summary>
    public class FeeMatrixDto
    {
        /// <summary>First-of-month dates that form the column headers.</summary>
        public List<DateTime> Months { get; set; } = new();

        public List<StudentFeeRowDto> Rows { get; set; } = new();
    }

    public class StudentFeeRowDto
    {
        public Guid   StudentId      { get; set; }
        public string StudentName    { get; set; } = string.Empty;
        public string RegistrationNo { get; set; } = string.Empty;
        public string FatherName     { get; set; } = string.Empty;
        public string ClassName      { get; set; } = string.Empty;
        public string TeacherName    { get; set; } = string.Empty;
        public string SectionName    { get; set; } = string.Empty;

        /// <summary>Per-month base fee amount.</summary>
        public decimal MonthlyFeeAmount { get; set; }

        /// <summary>Sum of all unpaid/partial remaining amounts.</summary>
        public decimal TotalRemaining { get; set; }

        /// <summary>Count of months with status Unpaid or Partial.</summary>
        public int UnpaidMonths { get; set; }

        /// <summary>Earliest upcoming due date among unpaid/partial dues.</summary>
        public DateTime? NextDueDate { get; set; }

        /// <summary>
        /// Key = "yyyy-MM" (e.g. "2026-01").
        /// Value = "Paid" | "Unpaid" | "Partial" | null (no due generated yet).
        /// </summary>
        public Dictionary<string, string?> MonthlyStatus { get; set; } = new();
    }
}
