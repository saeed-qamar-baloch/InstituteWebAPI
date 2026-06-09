namespace InstituteWebAPI.Models.DTO.FeeManagement
{
    public class BulkGenerateResultDto
    {
        /// <summary>Total number of active admissions processed.</summary>
        public int AdmissionsProcessed { get; set; }

        /// <summary>Number of admissions for which at least one new due was created.</summary>
        public int AdmissionsWithNewDues { get; set; }

        /// <summary>Total new fee due records created across all students.</summary>
        public int TotalDuesCreated { get; set; }

        /// <summary>Admissions skipped due to errors (e.g. missing DueDate config).</summary>
        public List<BulkGenerateErrorDto> Errors { get; set; } = new();
    }

    public class BulkGenerateErrorDto
    {
        public Guid AdmissionId { get; set; }
        public Guid StudentId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
