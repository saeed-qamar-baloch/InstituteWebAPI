using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Models.DTO.ResultApproval
{
    /// <summary>
    /// Used for both creating and toggling a ResultApproval record.
    /// If no record exists for the (TermID, CurrentClassID) pair it is created;
    /// otherwise the existing record is updated.
    /// </summary>
    public class UpsertResultApprovalDto
    {
        [Required]
        public Guid TermID { get; set; }

        [Required]
        public Guid CurrentClassID { get; set; }

        [Required]
        public bool IsApproved { get; set; }

        public string? Remarks { get; set; }
    }
}
