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
        Task<IReadOnlyList<StudentLookupDto>> SearchStudentsAsync(string? searchTerm);
        Task<IReadOnlyList<PaymentSummaryDto>> GetPaymentsAsync(string? searchTerm, DateTime? fromDate, DateTime? toDate, PaymentMethod? paymentMethod);
        Task<FeeDueDto> GenerateCardFeeAsync(Guid studentId);
        Task<FeeDueDto> GenerateAdmissionFeeAsync(Guid studentId);
        Task<FeeSettingsDto> GetFeeSettingsAsync();
        Task<FeeSettingsDto> UpdateFeeSettingsAsync(FeeSettingsDto request);
        Task DeleteFeeDueAsync(Guid feeDueId);
    }
}
