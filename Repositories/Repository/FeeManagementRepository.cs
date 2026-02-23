using InstituteWebAPI.Data;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace InstituteWebAPI.Repositories.Repository
{
    public class FeeManagementRepository : IFeeManagementRepository
    {
        private readonly RozhnInstituteDbContext dbContext;

        public FeeManagementRepository(RozhnInstituteDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Admissions?> GetActiveAdmissionByStudentIdAsync(Guid studentId)
        {
            return await dbContext.Admissions
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.StudentID == studentId && a.IsActive);
        }

        public async Task<Students?> GetStudentAsync(Guid studentId)
        {
            return await dbContext.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.StudentID == studentId);
        }

        public async Task<List<FeeDue>> GetUnpaidDuesByStudentAsync(Guid studentId)
        {
            return await dbContext.FeeDues
                .Include(d => d.Admission)
                .Include(d => d.PaymentDetails)
                .Where(d => d.Admission.StudentID == studentId)
                .ToListAsync();
        }

        public async Task<List<FeeDue>> GetUnpaidDuesByIdsAsync(Guid studentId, IEnumerable<Guid> feeDueIds)
        {
            var ids = feeDueIds.Distinct().ToList();
            return await dbContext.FeeDues
                .Include(d => d.Admission)
                .Include(d => d.PaymentDetails)
                .Where(d => ids.Contains(d.FeeDueId) && d.Admission.StudentID == studentId)
                .ToListAsync();
        }

        public async Task<List<DateTime>> GetExistingMonthlyFeeMonthsAsync(Guid admissionId, DateTime startMonth, DateTime endMonth)
        {
            return await dbContext.FeeDues
                .AsNoTracking()
                .Where(d => d.AdmissionId == admissionId
                            && d.FeeType == FeeDueType.Monthly
                            && d.FeeMonth >= startMonth
                            && d.FeeMonth <= endMonth)
                .Select(d => d.FeeMonth!.Value)
                .ToListAsync();
        }

        public async Task<FeeDue?> GetExistingOneTimeFeeDueAsync(Guid admissionId, FeeDueType feeType)
        {
            return await dbContext.FeeDues
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.AdmissionId == admissionId
                                          && d.FeeType == feeType
                                          && d.FeeMonth == null);
        }

        public async Task AddFeeDuesAsync(IEnumerable<FeeDue> feeDues)
        {
            await dbContext.FeeDues.AddRangeAsync(feeDues);
        }

        public async Task<FeeDue?> GetFeeDueAsync(Guid feeDueId)
        {
            return await dbContext.FeeDues
                .Include(d => d.PaymentDetails)
                .FirstOrDefaultAsync(d => d.FeeDueId == feeDueId);
        }

        public async Task AddPaymentAsync(Payment payment)
        {
            await dbContext.Payments.AddAsync(payment);
        }

        public async Task<FeeSettings?> GetFeeSettingsAsync()
        {
            return await dbContext.FeeSettings.AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task SaveFeeSettingsAsync(FeeSettings settings)
        {
            var existing = await dbContext.FeeSettings.FirstOrDefaultAsync();
            if (existing == null)
            {
                await dbContext.FeeSettings.AddAsync(settings);
            }
            else
            {
                existing.LateFeeAmount = settings.LateFeeAmount;
                existing.AdmissionFeeAmount = settings.AdmissionFeeAmount;
                existing.CardFeeAmount = settings.CardFeeAmount;
                existing.UpdatedAt = settings.UpdatedAt;
            }
        }

        public Task DeleteFeeDueAsync(FeeDue feeDue)
        {
            dbContext.FeeDues.Remove(feeDue);
            return Task.CompletedTask;
        }

        public async Task<List<Payment>> GetPaymentsAsync(string? searchTerm, DateTime? fromDate, DateTime? toDate, PaymentMethod? paymentMethod)
        {
            var query = dbContext.Payments
                .AsNoTracking()
                .Include(p => p.Student)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p => p.Student.StudentName.Contains(searchTerm)
                                         || p.Student.FatherName.Contains(searchTerm)
                                         || p.Student.RegistrationNo.Contains(searchTerm));
            }

            if (fromDate.HasValue)
            {
                var start = fromDate.Value.Date;
                query = query.Where(p => p.PaymentDate.Date >= start);
            }

            if (toDate.HasValue)
            {
                var end = toDate.Value.Date;
                query = query.Where(p => p.PaymentDate.Date <= end);
            }

            if (paymentMethod.HasValue)
            {
                query = query.Where(p => p.PaymentMethod == paymentMethod.Value);
            }

            return await query
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await dbContext.Database.BeginTransactionAsync();
        }

        public async Task<List<Students>> SearchStudentsAsync(string? searchTerm)
        {
            var query = dbContext.Students.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(s => s.StudentName.Contains(searchTerm)
                                         || s.FatherName.Contains(searchTerm)
                                         || s.RegistrationNo.Contains(searchTerm));
            }

            return await query
                .OrderBy(s => s.StudentName)
                .ToListAsync();
        }
    }
}
