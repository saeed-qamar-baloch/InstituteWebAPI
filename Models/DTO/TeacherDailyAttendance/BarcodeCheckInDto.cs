using System.ComponentModel.DataAnnotations;

namespace InstituteWebAPI.Models.DTO.TeacherDailyAttendance
{
    /// <summary>
    /// Payload sent by the reception barcode scanner.
    /// The API resolves the teacher from the barcode and marks them Present (or Late).
    /// </summary>
    public class BarcodeCheckInDto
    {
        [Required]
        public string Barcode { get; set; }

        /// <summary>
        /// Optional override status. Defaults to Present.
        /// Reception staff can set Late (3) if scanning after the grace period.
        /// </summary>
        [Range(0, 3)]
        public int Status { get; set; } = 0;   // Present
    }
}
