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

        /// <summary>
        /// Returns tracked Monthly fee dues (with PaymentDetails) for an admission whose
        /// FeeMonth falls within [startMonth, endMonth]. Used for waiving months.
        /// </summary>
        Task<List<FeeDue>> GetTrackedMonthlyDuesInRangeAsync(Guid admissionId, DateTime startMonth, DateTime endMonth);
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
        Task<List<Admissions>> GetAllActiveAdmissionsAsync();

        /// <summary>
        /// Returns all ClassStudents enrolled in the given term, with Student, Class,
        /// Teacher, Section navigations loaded. Optionally filtered by class or teacher.
        /// </summary>
        Task<List<ClassStudents>> GetEnrolledStudentsForMatrixAsync(
            Guid termId, Guid? classId, Guid? teacherId);

        /// <summary>
        /// Returns all Monthly fee dues for the given admission IDs whose FeeMonth
        /// falls within [startMonth, endMonth], with PaymentDetails loaded.
        /// </summary>
        Task<List<FeeDue>> GetMonthlyDuesForMatrixAsync(
            IEnumerable<Guid> admissionIds, DateTime startMonth, DateTime endMonth);
    }
}
