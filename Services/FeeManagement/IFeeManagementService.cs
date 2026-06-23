using InstituteWebAPI.Models.DTO.FeeManagement;
using InstituteWebApp.Models.Domain;

namespace InstituteWebAPI.Services.FeeManagement
{
    public interface IFeeManagementService
    {
        Task<IReadOnlyList<FeeDueDto>> GenerateMonthlyDuesAsync(Guid studentId);
        Task<IReadOnlyList<FeeDueDto>> GetUnpaidDuesAsync(Guid studentId);
        Task<PaymentDto> CollectFeeAsync(CollectFeeRequestDto request);
        Task<FeeDueDto?> WaiveLateFeeAsync(Guid feeDueId);

        /// <summary>
        /// Fully waives an Admission fee due: zeroes the base and late fee amounts
        /// and marks the due as Waived. Throws if the due is already paid or is
        /// not an Admission-type due.
        /// </summary>
        Task<FeeDueDto?> WaiveAdmissionFeeAsync(Guid feeDueId);
        Task<IReadOnlyList<StudentLookupDto>> SearchStudentsAsync(string? searchTerm);
        Task<IReadOnlyList<PaymentSummaryDto>> GetPaymentsAsync(string? searchTerm, DateTime? fromDate, DateTime? toDate, PaymentMethod? paymentMethod);
        Task<FeeDueDto> GenerateCardFeeAsync(Guid studentId);

        /// <summary>
        /// Creates an unpaid Card fee due for a card request, without throwing if it
        /// can't (no active admission, or no amount available). Uses <paramref name="requestedAmount"/>
        /// when greater than zero, otherwise the configured card fee amount.
        /// Returns null when no due was created.
        /// </summary>
        Task<FeeDueDto?> TryGenerateCardFeeAsync(Guid studentId, decimal requestedAmount);
        Task<FeeDueDto> GenerateAdmissionFeeAsync(Guid studentId);
        Task<FeeSettingsDto> GetFeeSettingsAsync();
        Task<FeeSettingsDto> UpdateFeeSettingsAsync(FeeSettingsDto request);
        Task DeleteFeeDueAsync(Guid feeDueId);
        Task<BulkGenerateResultDto> BulkGenerateMonthlyDuesAsync();

        /// <summary>
        /// Records a leave (full fee waiver) for a student over a month range and waives
        /// any existing unpaid monthly dues in that range. Months without a due are
        /// recorded as zero waived dues so they show as "on leave".
        /// </summary>
        Task<WaiveMonthsResultDto> WaiveMonthsAsync(WaiveMonthsRequestDto request);

        /// <summary>
        /// Awards a merit scholarship (percentage discount) over a month range. The discount
        /// is applied when monthly dues for those months are generated.
        /// </summary>
        Task<Guid> AwardScholarshipAsync(AwardScholarshipRequestDto request);

        /// <summary>
        /// Returns the fee matrix for the active term, optionally filtered by class/teacher/status.
        /// status: null = all, "unpaid" = has any Unpaid/Partial months.
        /// </summary>
        Task<FeeMatrixDto> GetFeeMatrixAsync(Guid? classId, Guid? teacherId, string? status);
    }
}
