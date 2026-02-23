using InstituteWebAPI.CustomActionFilters;
using InstituteWebAPI.Models.DTO.FeeManagement;
using InstituteWebAPI.Services.FeeManagement;
using InstituteWebApp.Models.Domain;
using Microsoft.AspNetCore.Mvc;

namespace InstituteWebAPI.Controllers
{
    [Route("api/fee-management")]
    [ApiController]
    public class FeeManagementController : ControllerBase
    {
        private readonly IFeeManagementService service;

        public FeeManagementController(IFeeManagementService service)
        {
            this.service = service;
        }

        [HttpGet("students")]
        public async Task<IActionResult> SearchStudents([FromQuery] string? searchTerm)
        {
            var students = await service.SearchStudentsAsync(searchTerm);
            return Ok(students);
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
    }
}
