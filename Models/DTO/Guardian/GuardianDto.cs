namespace InstituteWebAPI.Models.DTO.Guardian
{
    public class GuardianDto
    {
        public Guid GuardianID { get; set; }
        public Guid StudentID { get; set; }
        public string? StudentName { get; set; }      // flattened from nav property
        public string GuardianName { get; set; }
        public string Relation { get; set; }
        public string Contact { get; set; }
        public string? Cnic { get; set; }
        public string? Address { get; set; }
        public string? Occupation { get; set; }
        public string? Remarks { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
