using AutoMapper;
using InstituteWebAPI.Models.Configuration;
using InstituteWebAPI.Models.DTO.FeeManagement;
using InstituteWebAPI.Repositories.IRepository;
using Microsoft.Extensions.Options;
using InstituteWebApp.Models.Domain;

namespace InstituteWebAPI.Services.FeeManagement
{
    public class FeeManagementService : IFeeManagementService
    {
        private readonly IFeeManagementRepository repository;
        private readonly IMapper mapper;
        private readonly FeeManagementOptions options;

        public FeeManagementService(IFeeManagementRepository repository, IMapper mapper, IOptions<FeeManagementOptions> options)
        {
            this.repository = repository;
            this.mapper = mapper;
            this.options = options.Value;
        }

        public async Task<IReadOnlyList<FeeDueDto>> GenerateMonthlyDuesAsync(Guid studentId)
        {
            var admission = await repository.GetActiveAdmissionByStudentIdAsync(studentId);
            if (admission == null)
            {
                throw new InvalidOperationException("Active admission not found for the student.");
            }

            if (!admission.DueDate.HasValue)
            {
                throw new InvalidOperationException("Monthly due day is not configured for the admission.");
            }

            var startMonth = new DateTime(admission.RegistrationDate.Year, admission.RegistrationDate.Month, 1);
            var now = DateTime.UtcNow;
            var endMonth = new DateTime(now.Year, now.Month, 1);

            if (startMonth > endMonth)
            {
                return Array.Empty<FeeDueDto>();
            }

            var existingMonths = await repository.GetExistingMonthlyFeeMonthsAsync(admission.AdmissionID, startMonth, endMonth);
            var existingMonthSet = new HashSet<DateTime>(existingMonths.Select(m => new DateTime(m.Year, m.Month, 1)));

            var settings = await GetFeeSettingsValuesAsync();
            var created = new List<FeeDue>();
            var current = startMonth;
            var today = DateTime.UtcNow.Date;

            await AddOneTimeFeeIfMissingAsync(admission, FeeDueType.Admission, settings.AdmissionFeeAmount, settings.LateFeeAmount, today, created);

            while (current <= endMonth)
            {
                if (!existingMonthSet.Contains(current))
                {
                    var dueDay = Math.Min(admission.DueDate.Value, DateTime.DaysInMonth(current.Year, current.Month));
                    var dueDate = new DateTime(current.Year, current.Month, dueDay);
                    var isLate = today > dueDate.Date;

                    created.Add(new FeeDue
                    {
                        FeeDueId = Guid.NewGuid(),
                        AdmissionId = admission.AdmissionID,
                        FeeType = FeeDueType.Monthly,
                        FeeMonth = current,
                        BaseAmount = admission.MonthlyFee,
                        LateFeeAmount = isLate ? NormalizeAmount(settings.LateFeeAmount) : 0m,
                        DueDate = dueDate,
                        IsLateFeeWaived = false,
                        Status = FeeDueStatus.Unpaid,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                current = current.AddMonths(1);
            }

            if (created.Count > 0)
            {
                await repository.AddFeeDuesAsync(created);
                await repository.SaveChangesAsync();
            }

            return mapper.Map<List<FeeDueDto>>(created);
        }

        public async Task<IReadOnlyList<FeeDueDto>> GetUnpaidDuesAsync(Guid studentId)
        {
            var dues = await repository.GetUnpaidDuesByStudentAsync(studentId);
            var today = DateTime.UtcNow.Date;
            var hasChanges = false;
            var settings = await GetFeeSettingsValuesAsync();
            var remainingDues = new List<FeeDue>();

            foreach (var due in dues)
            {
                if (ApplyLateFeeIfNeeded(due, today, settings.LateFeeAmount))
                {
                    hasChanges = true;
                }

                var totalDue = due.BaseAmount + (due.IsLateFeeWaived ? 0m : due.LateFeeAmount);
                var paid = due.PaymentDetails.Sum(p => p.PaidAmount);
                if (paid <= 0m)
                {
                    if (due.Status != FeeDueStatus.Unpaid)
                    {
                        due.Status = FeeDueStatus.Unpaid;
                        hasChanges = true;
                    }

                    remainingDues.Add(due);
                    continue;
                }

                if (paid < totalDue)
                {
                    if (due.Status != FeeDueStatus.Partial)
                    {
                        due.Status = FeeDueStatus.Partial;
                        hasChanges = true;
                    }

                    remainingDues.Add(due);
                    continue;
                }

                if (due.Status != FeeDueStatus.Paid)
                {
                    due.Status = FeeDueStatus.Paid;
                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                await repository.SaveChangesAsync();
            }

            var ordered = remainingDues
                .OrderBy(d => d.FeeMonth ?? DateTime.MaxValue)
                .ThenBy(d => d.CreatedAt)
                .ToList();

            return mapper.Map<List<FeeDueDto>>(ordered);
        }

        public async Task<PaymentDto> CollectFeeAsync(CollectFeeRequestDto request)
        {
            if (request.FeePayments.Count == 0)
            {
                throw new InvalidOperationException("Select at least one due for payment.");
            }

            var student = await repository.GetStudentAsync(request.StudentId);
            if (student == null)
            {
                throw new InvalidOperationException("Student not found.");
            }

            var paymentItems = request.FeePayments
                .GroupBy(x => x.FeeDueId)
                .ToDictionary(g => g.Key, g => g.Last());

            var dues = await repository.GetUnpaidDuesByIdsAsync(request.StudentId, paymentItems.Keys);
            if (dues.Count != paymentItems.Count)
            {
                throw new InvalidOperationException("One or more dues are not available for payment.");
            }

            var today = DateTime.UtcNow.Date;
            var settings = await GetFeeSettingsValuesAsync();
            var paymentDetails = new List<PaymentDetail>();
            var totalAmount = 0m;

            foreach (var due in dues)
            {
                var item = paymentItems[due.FeeDueId];
                if (item.WaiveLateFee)
                {
                    due.IsLateFeeWaived = true;
                    due.LateFeeAmount = 0m;
                }
                else
                {
                    due.IsLateFeeWaived = false;
                    if (item.LateFeeAmount >= 0)
                    {
                        due.LateFeeAmount = item.LateFeeAmount;
                    }
                }

                if (!due.IsLateFeeWaived && due.DueDate.Date < today && due.LateFeeAmount <= 0m)
                {
                    due.LateFeeAmount = NormalizeAmount(settings.LateFeeAmount);
                }
                else
                {
                    ApplyLateFeeIfNeeded(due, today, settings.LateFeeAmount);
                }

                var totalDue = due.BaseAmount + (due.IsLateFeeWaived ? 0m : due.LateFeeAmount);
                var paidSoFar = due.PaymentDetails.Sum(p => p.PaidAmount);
                var remaining = totalDue - paidSoFar;

                if (remaining <= 0m)
                {
                    throw new InvalidOperationException("Selected fee due is already fully paid.");
                }

                if (item.PaidAmount <= 0m || item.PaidAmount > remaining)
                {
                    throw new InvalidOperationException("Invalid paid amount for selected due.");
                }

                paymentDetails.Add(new PaymentDetail
                {
                    PaymentDetailId = Guid.NewGuid(),
                    FeeDueId = due.FeeDueId,
                    PaidAmount = item.PaidAmount
                });

                totalAmount += item.PaidAmount;
                due.Status = item.PaidAmount >= remaining ? FeeDueStatus.Paid : FeeDueStatus.Partial;
            }

            var payment = new Payment
            {
                PaymentId = Guid.NewGuid(),
                StudentId = request.StudentId,
                PaymentDate = DateTime.UtcNow,
                PaymentMethod = request.PaymentMethod,
                Remarks = request.Remarks,
                TotalAmount = totalAmount,
                PaymentDetails = paymentDetails
            };

            await using var transaction = await repository.BeginTransactionAsync();

            foreach (var due in dues)
            {
                due.Status = FeeDueStatus.Paid;
            }

            await repository.AddPaymentAsync(payment);
            await repository.SaveChangesAsync();
            await transaction.CommitAsync();

            return mapper.Map<PaymentDto>(payment);
        }

        public async Task<FeeDueDto?> WaiveLateFeeAsync(Guid feeDueId)
        {
            var due = await repository.GetFeeDueAsync(feeDueId);
            if (due == null)
            {
                return null;
            }

            if (due.Status == FeeDueStatus.Paid)
            {
                throw new InvalidOperationException("Cannot waive late fee for a paid due.");
            }

            due.IsLateFeeWaived = true;
            due.LateFeeAmount = 0m;

            await repository.SaveChangesAsync();

            return mapper.Map<FeeDueDto>(due);
        }

        public async Task<FeeSettingsDto> GetFeeSettingsAsync()
        {
            var settings = await repository.GetFeeSettingsAsync();
            if (settings == null)
            {
                return new FeeSettingsDto
                {
                    LateFeeAmount = NormalizeAmount(options.LateFeeAmount),
                    AdmissionFeeAmount = NormalizeAmount(options.AdmissionFeeAmount),
                    CardFeeAmount = NormalizeAmount(options.CardFeeAmount)
                };
            }

            return new FeeSettingsDto
            {
                LateFeeAmount = NormalizeAmount(settings.LateFeeAmount),
                AdmissionFeeAmount = NormalizeAmount(settings.AdmissionFeeAmount),
                CardFeeAmount = NormalizeAmount(settings.CardFeeAmount)
            };
        }

        public async Task<FeeSettingsDto> UpdateFeeSettingsAsync(FeeSettingsDto request)
        {
            var settings = new FeeSettings
            {
                FeeSettingsId = Guid.NewGuid(),
                LateFeeAmount = NormalizeAmount(request.LateFeeAmount),
                AdmissionFeeAmount = NormalizeAmount(request.AdmissionFeeAmount),
                CardFeeAmount = NormalizeAmount(request.CardFeeAmount),
                UpdatedAt = DateTime.UtcNow
            };

            await repository.SaveFeeSettingsAsync(settings);
            await repository.SaveChangesAsync();

            return new FeeSettingsDto
            {
                LateFeeAmount = settings.LateFeeAmount,
                AdmissionFeeAmount = settings.AdmissionFeeAmount,
                CardFeeAmount = settings.CardFeeAmount
            };
        }

        public async Task DeleteFeeDueAsync(Guid feeDueId)
        {
            var due = await repository.GetFeeDueAsync(feeDueId);
            if (due == null)
            {
                throw new InvalidOperationException("Fee due not found.");
            }

            if (due.FeeType != FeeDueType.Admission && due.FeeType != FeeDueType.Card)
            {
                throw new InvalidOperationException("Only admission or card fees can be deleted.");
            }

            if (due.Status == FeeDueStatus.Paid || due.PaymentDetails.Any())
            {
                throw new InvalidOperationException("Cannot delete a fee that has been paid.");
            }

            await repository.DeleteFeeDueAsync(due);
            await repository.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<StudentLookupDto>> SearchStudentsAsync(string? searchTerm)
        {
            var students = await repository.SearchStudentsAsync(searchTerm);
            return students.Select(s => new StudentLookupDto
            {
                StudentId = s.StudentID,
                RegistrationNo = s.RegistrationNo,
                StudentName = s.StudentName,
                FatherName = s.FatherName
            }).ToList();
        }

        public async Task<FeeDueDto> GenerateCardFeeAsync(Guid studentId)
        {
            var admission = await repository.GetActiveAdmissionByStudentIdAsync(studentId);
            if (admission == null)
            {
                throw new InvalidOperationException("Active admission not found for the student.");
            }

            var settings = await GetFeeSettingsValuesAsync();
            if (settings.CardFeeAmount <= 0)
            {
                throw new InvalidOperationException("Card fee amount is not configured.");
            }

            var today = DateTime.UtcNow.Date;
            var due = BuildOneTimeFee(admission, FeeDueType.Card, settings.CardFeeAmount, settings.LateFeeAmount, today);

            await repository.AddFeeDuesAsync(new[] { due });
            await repository.SaveChangesAsync();

            return mapper.Map<FeeDueDto>(due);
        }

        public async Task<FeeDueDto> GenerateAdmissionFeeAsync(Guid studentId)
        {
            var admission = await repository.GetActiveAdmissionByStudentIdAsync(studentId);
            if (admission == null)
            {
                throw new InvalidOperationException("Active admission not found for the student.");
            }

            var settings = await GetFeeSettingsValuesAsync();
            if (settings.AdmissionFeeAmount <= 0)
            {
                throw new InvalidOperationException("Admission fee amount is not configured.");
            }

            var existing = await repository.GetExistingOneTimeFeeDueAsync(admission.AdmissionID, FeeDueType.Admission);
            if (existing != null)
            {
                throw new InvalidOperationException("Admission fee is already generated for this admission.");
            }

            var today = DateTime.UtcNow.Date;
            var due = BuildOneTimeFee(admission, FeeDueType.Admission, settings.AdmissionFeeAmount, settings.LateFeeAmount, today);

            await repository.AddFeeDuesAsync(new[] { due });
            await repository.SaveChangesAsync();

            return mapper.Map<FeeDueDto>(due);
        }

        public async Task<IReadOnlyList<PaymentSummaryDto>> GetPaymentsAsync(string? searchTerm, DateTime? fromDate, DateTime? toDate, PaymentMethod? paymentMethod)
        {
            var payments = await repository.GetPaymentsAsync(searchTerm, fromDate, toDate, paymentMethod);
            return mapper.Map<List<PaymentSummaryDto>>(payments);
        }

        private async Task AddOneTimeFeeIfMissingAsync(Admissions admission, FeeDueType feeType, decimal amount, decimal lateFeeAmount, DateTime today, List<FeeDue> created)
        {
            if (amount <= 0)
            {
                return;
            }

            var existing = await repository.GetExistingOneTimeFeeDueAsync(admission.AdmissionID, feeType);
            if (existing != null)
            {
                return;
            }

            created.Add(BuildOneTimeFee(admission, feeType, amount, lateFeeAmount, today));
        }

        private FeeDue BuildOneTimeFee(Admissions admission, FeeDueType feeType, decimal amount, decimal lateFeeAmount, DateTime today)
        {
            var dueDate = admission.RegistrationDate.Date;
            var isLate = today > dueDate;

            return new FeeDue
            {
                FeeDueId = Guid.NewGuid(),
                AdmissionId = admission.AdmissionID,
                FeeType = feeType,
                FeeMonth = null,
                BaseAmount = amount,
                LateFeeAmount = isLate ? NormalizeAmount(lateFeeAmount) : 0m,
                DueDate = feeType == FeeDueType.Card ? today : dueDate,
                IsLateFeeWaived = false,
                Status = FeeDueStatus.Unpaid,
                CreatedAt = DateTime.UtcNow
            };
        }

        private bool ApplyLateFeeIfNeeded(FeeDue due, DateTime today, decimal lateFeeAmount)
        {
            if (due.IsLateFeeWaived || due.LateFeeAmount > 0m)
            {
                return false;
            }

            if (today <= due.DueDate.Date)
            {
                return false;
            }

            due.LateFeeAmount = NormalizeAmount(lateFeeAmount);
            return true;
        }

        private async Task<(decimal LateFeeAmount, decimal AdmissionFeeAmount, decimal CardFeeAmount)> GetFeeSettingsValuesAsync()
        {
            var settings = await repository.GetFeeSettingsAsync();
            if (settings == null)
            {
                return (NormalizeAmount(options.LateFeeAmount), NormalizeAmount(options.AdmissionFeeAmount), NormalizeAmount(options.CardFeeAmount));
            }

            return (NormalizeAmount(settings.LateFeeAmount), NormalizeAmount(settings.AdmissionFeeAmount), NormalizeAmount(settings.CardFeeAmount));
        }

        private decimal NormalizeAmount(decimal amount)
        {
            return amount < 0 ? 0 : amount;
        }
    }
}
