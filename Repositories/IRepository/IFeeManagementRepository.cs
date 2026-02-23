using InstituteWebApp.Models.Domain;
using Microsoft.EntityFrameworkCore.Storage;

namespace InstituteWebAPI.Repositories.IRepository
{
    public interface IFeeManagementRepository
    {
        Task<Admissions?> GetActiveAdmissionByStudentIdAsync(Guid studentId);
        Task<Students?> GetStudentAsync(Guid studentId);
        Task<List<FeeDue>> GetUnpaidDuesByStudentAsync(Guid studentId);
        Task<List<FeeDue>> GetUnpaidDuesByIdsAsync(Guid studentId, IEnumerable<Guid> feeDueIds);
        Task<List<DateTime>> GetExistingMonthlyFeeMonthsAsync(Guid admissionId, DateTime startMonth, DateTime endMonth);
        Task<FeeDue?> GetExistingOneTimeFeeDueAsync(Guid admissionId, FeeDueType feeType);
        Task AddFeeDuesAsync(IEnumerable<FeeDue> feeDues);
        Task<FeeDue?> GetFeeDueAsync(Guid feeDueId);
        Task AddPaymentAsync(Payment payment);
        Task<FeeSettings?> GetFeeSettingsAsync();
        Task SaveFeeSettingsAsync(FeeSettings settings);
        Task DeleteFeeDueAsync(FeeDue feeDue);
        Task<List<Payment>> GetPaymentsAsync(string? searchTerm, DateTime? fromDate, DateTime? toDate, PaymentMethod? paymentMethod);
        Task SaveChangesAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task<List<Students>> SearchStudentsAsync(string? searchTerm);
    }
}
