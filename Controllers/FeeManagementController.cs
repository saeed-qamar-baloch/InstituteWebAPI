using InstituteWebAPI.CustomActionFilters;
using InstituteWebAPI.Data;
using InstituteWebAPI.Models.DTO.FeeManagement;
using InstituteWebAPI.Services.FeeManagement;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Controllers
{
    [Route("api/fee-management")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class FeeManagementController : ControllerBase
    {
        private readonly IFeeManagementService service;
        private readonly RozhnInstituteDbContext dbContext;
        private readonly InstituteWebAPI.Services.Audit.IAuditService audit;

        public FeeManagementController(IFeeManagementService service, RozhnInstituteDbContext dbContext, InstituteWebAPI.Services.Audit.IAuditService audit)
        {
            this.service   = service;
            this.dbContext = dbContext;
            this.audit     = audit;
        }

        [HttpGet("students")]
        public async Task<IActionResult> SearchStudents([FromQuery] string? searchTerm)
        {
            var students = await service.SearchStudentsAsync(searchTerm);
            return Ok(students);
        }

        // ── POST api/fee-management/generate-monthly-dues/bulk ───────────────
        // Runs monthly-due generation for ALL active admissions in one call.
        // Returns a summary: processed count, admissions with new dues, total
        // dues created, and any per-admission errors (e.g. missing DueDate).
        [HttpPost("generate-monthly-dues/bulk")]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public async Task<IActionResult> BulkGenerateMonthlyDues()
        {
            var result = await service.BulkGenerateMonthlyDuesAsync();
            return Ok(result);
        }

        [HttpPost("students/{studentId:guid}/generate-monthly-dues")]
        public async Task<IActionResult> GenerateMonthlyDues(Guid studentId)
        {
            try
            {
                var dues = await service.GenerateMonthlyDuesAsync(studentId);
                return Ok(dues);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("students/{studentId:guid}/unpaid-dues")]
        public async Task<IActionResult> GetUnpaidDues(Guid studentId)
        {
            var dues = await service.GetUnpaidDuesAsync(studentId);
            return Ok(dues);
        }

        [HttpPost("collect")]
        [ValidateModel]
        public async Task<IActionResult> CollectFee([FromBody] CollectFeeRequestDto request)
        {
            try
            {
                var payment = await service.CollectFeeAsync(request);
                await audit.LogAsync("Fees", "Fee Collected", "A fee payment was recorded.", "Payment", null);
                return Ok(payment);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("fee-dues/{feeDueId:guid}/waive-late-fee")]
        public async Task<IActionResult> WaiveLateFee(Guid feeDueId)
        {
            try
            {
                var due = await service.WaiveLateFeeAsync(feeDueId);
                if (due == null) return NotFound();
                return Ok(due);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("fee-dues/{feeDueId:guid}/waive-admission-fee")]
        public async Task<IActionResult> WaiveAdmissionFee(Guid feeDueId)
        {
            try
            {
                var due = await service.WaiveAdmissionFeeAsync(feeDueId);
                if (due == null) return NotFound();
                return Ok(due);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("students/{studentId:guid}/generate-card-fee")]
        public async Task<IActionResult> GenerateCardFee(Guid studentId)
        {
            try
            {
                var due = await service.GenerateCardFeeAsync(studentId);
                return Ok(due);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("students/{studentId:guid}/generate-admission-fee")]
        public async Task<IActionResult> GenerateAdmissionFee(Guid studentId)
        {
            try
            {
                var due = await service.GenerateAdmissionFeeAsync(studentId);
                return Ok(due);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("payments")]
        public async Task<IActionResult> GetPayments([FromQuery] string? searchTerm, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, [FromQuery] PaymentMethod? paymentMethod)
        {
            var payments = await service.GetPaymentsAsync(searchTerm, fromDate, toDate, paymentMethod);
            return Ok(payments);
        }

        [HttpGet("settings")]
        public async Task<IActionResult> GetFeeSettings()
        {
            var settings = await service.GetFeeSettingsAsync();
            return Ok(settings);
        }

        [HttpPut("settings")]
        public async Task<IActionResult> UpdateFeeSettings([FromBody] FeeSettingsDto request)
        {
            try
            {
                var settings = await service.UpdateFeeSettingsAsync(request);
                return Ok(settings);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ── POST api/fee-management/waive-months ─────────────────────────────
        // Records a leave (full fee waiver) for a student over a month range and
        // waives existing unpaid dues / creates zero waived dues for those months.
        [HttpPost("waive-months")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> WaiveMonths([FromBody] WaiveMonthsRequestDto request)
        {
            try
            {
                var result = await service.WaiveMonthsAsync(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ── POST api/fee-management/award-scholarship ────────────────────────
        [HttpPost("award-scholarship")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AwardScholarship([FromBody] AwardScholarshipRequestDto request)
        {
            try
            {
                var id = await service.AwardScholarshipAsync(request);
                return Ok(new { scholarshipId = id });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ── GET api/fee-management/fee-matrix ────────────────────────────────
        // Returns the monthly fee matrix for the active term.
        // Query params: classId, teacherId, status ("unpaid" to show only defaulters)
        [HttpGet("fee-matrix")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetFeeMatrix(
            [FromQuery] Guid? classId,
            [FromQuery] Guid? teacherId,
            [FromQuery] string? status)
        {
            var matrix = await service.GetFeeMatrixAsync(classId, teacherId, status);
            return Ok(matrix);
        }

        [HttpDelete("fee-dues/{feeDueId:guid}")]
        public async Task<IActionResult> DeleteFeeDue(Guid feeDueId)
        {
            try
            {
                await service.DeleteFeeDueAsync(feeDueId);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ── GET api/fee-management/payments/{paymentId}/receipt ───────────────
        // Returns a structured receipt payload for a single payment.
        // The frontend renders / prints this — no PDF generation on the server.
        [HttpGet("payments/{paymentId:guid}/receipt")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetReceipt([FromRoute] Guid paymentId)
        {
            var payment = await dbContext.Payments
                .AsNoTracking()
                .Include(p => p.Student)
                    .ThenInclude(s => s.Village)
                .Include(p => p.PaymentDetails)
                    .ThenInclude(pd => pd.FeeDue)
                        .ThenInclude(d => d.Admission)
                            .ThenInclude(a => a.Course)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

            if (payment == null) return NotFound("Payment not found.");

            var student = payment.Student;

            // Build one receipt line per fee due settled in this payment
            var lines = payment.PaymentDetails.Select(pd =>
            {
                var due        = pd.FeeDue;
                var totalDue   = due.BaseAmount + (due.IsLateFeeWaived ? 0m : due.LateFeeAmount);
                var label      = due.FeeType switch
                {
                    FeeDueType.Monthly   => due.FeeMonth.HasValue
                                            ? $"Monthly Fee – {due.FeeMonth.Value:MMM yyyy}"
                                            : "Monthly Fee",
                    FeeDueType.Admission => "Admission Fee",
                    FeeDueType.Card      => "Card Fee",
                    _                   => due.FeeType.ToString()
                };

                return new
                {
                    Description    = label,
                    CourseName     = due.Admission?.Course?.CourseName,
                    BaseAmount     = due.BaseAmount,
                    LateFee        = due.IsLateFeeWaived ? 0m : due.LateFeeAmount,
                    LateFeeWaived  = due.IsLateFeeWaived,
                    TotalDue       = totalDue,
                    AmountPaid     = pd.PaidAmount,
                    Balance        = totalDue - pd.PaidAmount,
                    DueDate        = due.DueDate,
                    FeeMonth       = due.FeeMonth
                };
            }).ToList();

            return Ok(new
            {
                // ── Receipt header ─────────────────────────────────────────
                ReceiptNo      = payment.PaymentId.ToString("N")[..8].ToUpper(), // short ref
                PaymentID      = payment.PaymentId,
                PaymentDate    = payment.PaymentDate,
                PaymentMethod  = payment.PaymentMethod.ToString(),
                Remarks        = payment.Remarks,

                // ── Student info ───────────────────────────────────────────
                Student = new
                {
                    student.StudentID,
                    student.RegistrationNo,
                    student.StudentName,
                    student.FatherName,
                    student.FatherContact,
                    Village    = student.Village?.VillageName,
                    student.City
                },

                // ── Line items ─────────────────────────────────────────────
                Lines          = lines,

                // ── Totals ─────────────────────────────────────────────────
                GrandTotal     = payment.TotalAmount,
                GeneratedAt    = DateTime.UtcNow
            });
        }
    }
}
