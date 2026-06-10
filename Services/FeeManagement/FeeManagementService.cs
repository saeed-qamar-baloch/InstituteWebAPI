using AutoMapper;
using InstituteWebAPI.Helpers;
using InstituteWebAPI.Models.Configuration;
using InstituteWebAPI.Models.DTO.FeeManagement;
using InstituteWebAPI.Repositories.IRepository;
using Microsoft.Extensions.Options;
using InstituteWebApp.Models.Domain;
using InstituteWebAPI.Data;

namespace InstituteWebAPI.Services.FeeManagement
{
    public class FeeManagementService : IFeeManagementService
    {
        private readonly IFeeManagementRepository repository;
        private readonly ITermRepository termRepository;
        private readonly IScholarshipRepository scholarshipRepository;
        private readonly IMapper mapper;
        private readonly FeeManagementOptions options;

        public FeeManagementService(
            IFeeManagementRepository repository,
            ITermRepository termRepository,
            IScholarshipRepository scholarshipRepository,
            IMapper mapper,
            IOptions<FeeManagementOptions> options)
        {
            this.repository            = repository;
            this.termRepository        = termRepository;
            this.scholarshipRepository = scholarshipRepository;
            this.mapper                = mapper;
            this.options               = options.Value;
        }

        /// <summary>
        /// Applies any active scholarship/leave for the given month to a base monthly fee.
        /// Returns the discounted base, the resulting status, and whether a late fee applies.
        /// </summary>
        private async Task<(decimal BaseAmount, FeeDueStatus Status, bool ApplyLateFee)> ApplyConcessionAsync(
            Guid admissionId, decimal monthlyFee, DateTime labelMonth)
        {
            var scholarship = await scholarshipRepository.GetActiveForMonthAsync(admissionId, labelMonth);
            if (scholarship == null)
                return (monthlyFee, FeeDueStatus.Unpaid, true);

            // Delegate to the testable pure function in FeeCalculator.
            return FeeCalculator.ApplyConcession(monthlyFee, scholarship.DiscountPercent);
        }

        public async Task<IReadOnlyList<FeeDueDto>> GenerateMonthlyDuesAsync(Guid studentId)
        {
            var admission = await repository.GetActiveAdmissionByStudentIdAsync(studentId);
            if (admission == null)
            {
                throw new InvalidOperationException("Active admission not found for the student.");
            }

            // Free students are never charged — no dues at all.
            if (admission.IsFree)
            {
                return Array.Empty<FeeDueDto>();
            }

            // No monthly fee configured → nothing to charge (no due day expected either).
            if (admission.MonthlyFee <= 0m)
            {
                return Array.Empty<FeeDueDto>();
            }

            // Due day may be unset on older/imported admissions — fall back to the
            // registration day rather than failing the whole generation.
            var effectiveDueDay = admission.DueDate ?? admission.RegistrationDate.Day;

            var startMonth = new DateTime(admission.RegistrationDate.Year, admission.RegistrationDate.Month, 1);
            var now = DateTime.UtcNow;
            var endMonth = new DateTime(now.Year, now.Month, 1);

            var settings = await GetFeeSettingsValuesAsync();
            // Never generate dues before the configured fee-start month.
            if (settings.FeeStartMonth.HasValue && startMonth < settings.FeeStartMonth.Value)
            {
                startMonth = settings.FeeStartMonth.Value;
            }

            if (startMonth > endMonth)
            {
                return Array.Empty<FeeDueDto>();
            }

            // Look one month past endMonth: the 25th-rule labels the last month's due
            // as the NEXT month, so it must be matched to avoid a duplicate insert
            // (unique index on AdmissionId+FeeType+FeeMonth).
            var existingMonths = await repository.GetExistingMonthlyFeeMonthsAsync(admission.AdmissionID, startMonth, endMonth.AddMonths(1));
            // existingMonthSet uses the LABEL month (FeeMonth stored in DB, already 25th-rule-adjusted)
            var existingMonthSet = new HashSet<DateTime>(existingMonths.Select(m => new DateTime(m.Year, m.Month, 1)));
            var created = new List<FeeDue>();
            var current = startMonth;
            var today = DateTime.UtcNow.Date;

            await AddOneTimeFeeIfMissingAsync(admission, FeeDueType.Admission, settings.AdmissionFeeAmount, settings.LateFeeAmount, today, created);

            while (current <= endMonth)
            {
                var dueDay    = Math.Min(effectiveDueDay, DateTime.DaysInMonth(current.Year, current.Month));
                var dueDate   = new DateTime(current.Year, current.Month, dueDay);

                // 25th-rule: if the due day falls on or after the 25th, this payment
                // is considered an advance for the NEXT calendar month.
                var labelMonth = dueDay >= 25 ? current.AddMonths(1) : current;

                if (!existingMonthSet.Contains(labelMonth))
                {
                    var (baseAmt, status, applyLate) = await ApplyConcessionAsync(admission.AdmissionID, admission.MonthlyFee, labelMonth);
                    var isLate = applyLate && today > dueDate.Date;

                    created.Add(new FeeDue
                    {
                        FeeDueId        = Guid.NewGuid(),
                        AdmissionId     = admission.AdmissionID,
                        FeeType         = FeeDueType.Monthly,
                        FeeMonth        = labelMonth,   // label shown to users
                        BaseAmount      = baseAmt,
                        LateFeeAmount   = isLate ? NormalizeAmount(settings.LateFeeAmount) : 0m,
                        DueDate         = dueDate,      // physical collection date
                        IsLateFeeWaived = false,
                        Status          = status,
                        CreatedAt       = DateTime.UtcNow
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
                // Self-heal: clear any stray late fee on one-time admission/card dues.
                if (due.FeeType != FeeDueType.Monthly && !due.IsLateFeeWaived && due.LateFeeAmount > 0m)
                {
                    due.LateFeeAmount = 0m;
                    hasChanges = true;
                }

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

                if (due.FeeType == FeeDueType.Monthly && !due.IsLateFeeWaived && due.DueDate.Date < today && due.LateFeeAmount <= 0m)
                {
                    due.LateFeeAmount = NormalizeAmount(settings.LateFeeAmount);
                }
                else
                {
                    ApplyLateFeeIfNeeded(due, today, settings.LateFeeAmount);
                }

                // One-time fees never carry a late fee.
                if (due.FeeType != FeeDueType.Monthly)
                {
                    due.LateFeeAmount = 0m;
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
                CardFeeAmount = NormalizeAmount(settings.CardFeeAmount),
                FeeStartYear = settings.FeeStartMonth?.Year,
                FeeStartMonth = settings.FeeStartMonth?.Month
            };
        }

        public async Task<FeeSettingsDto> UpdateFeeSettingsAsync(FeeSettingsDto request)
        {
            DateTime? feeStart = null;
            if (request.FeeStartYear is int y && request.FeeStartMonth is int m && m >= 1 && m <= 12)
            {
                feeStart = new DateTime(y, m, 1);
            }

            var settings = new FeeSettings
            {
                FeeSettingsId = Guid.NewGuid(),
                LateFeeAmount = NormalizeAmount(request.LateFeeAmount),
                AdmissionFeeAmount = NormalizeAmount(request.AdmissionFeeAmount),
                CardFeeAmount = NormalizeAmount(request.CardFeeAmount),
                FeeStartMonth = feeStart,
                UpdatedAt = DateTime.UtcNow
            };

            await repository.SaveFeeSettingsAsync(settings);
            await repository.SaveChangesAsync();

            return new FeeSettingsDto
            {
                LateFeeAmount = settings.LateFeeAmount,
                AdmissionFeeAmount = settings.AdmissionFeeAmount,
                CardFeeAmount = settings.CardFeeAmount,
                FeeStartYear = settings.FeeStartMonth?.Year,
                FeeStartMonth = settings.FeeStartMonth?.Month
            };
        }

        public async Task<WaiveMonthsResultDto> WaiveMonthsAsync(WaiveMonthsRequestDto request)
        {
            var admission = await repository.GetActiveAdmissionByStudentIdAsync(request.StudentId);
            if (admission == null)
                throw new InvalidOperationException("Active admission not found for the student.");

            var from = new DateTime(request.FromMonth.Year, request.FromMonth.Month, 1);
            var to   = new DateTime(request.ToMonth.Year, request.ToMonth.Month, 1);
            if (to < from)
                throw new InvalidOperationException("ToMonth must be on or after FromMonth.");

            // Record the leave as a 100% scholarship so future generation also honors it.
            var leave = await scholarshipRepository.AddAsync(new Scholarship
            {
                StudentID       = request.StudentId,
                AdmissionID     = admission.AdmissionID,
                DiscountPercent = 100,
                IsLeave         = true,
                FromMonth       = from,
                ToMonth         = to,
                Reason          = string.IsNullOrWhiteSpace(request.Reason) ? "Leave" : request.Reason,
                Status          = ScholarshipStatus.Active
            }, string.Empty);

            // Waive existing unpaid dues / create zero waived dues for months without one.
            var existing = await repository.GetTrackedMonthlyDuesInRangeAsync(admission.AdmissionID, from, to);
            var byMonth = existing
                .Where(d => d.FeeMonth.HasValue)
                .ToDictionary(d => new DateTime(d.FeeMonth!.Value.Year, d.FeeMonth.Value.Month, 1));

            var result = new WaiveMonthsResultDto { ScholarshipId = leave.ScholarshipID };
            var toCreate = new List<FeeDue>();
            var now = DateTime.UtcNow;

            for (var m = from; m <= to; m = m.AddMonths(1))
            {
                if (byMonth.TryGetValue(m, out var due))
                {
                    var paid = due.PaymentDetails.Sum(p => p.PaidAmount);
                    if (due.Status == FeeDueStatus.Paid || paid > 0m)
                    {
                        result.MonthsSkipped++;   // already paid — never touch
                        continue;
                    }
                    due.BaseAmount      = 0m;
                    due.LateFeeAmount   = 0m;
                    due.IsLateFeeWaived = true;
                    due.Status          = FeeDueStatus.Waived;
                    result.MonthsWaived++;
                }
                else
                {
                    var dueDay  = admission.DueDate.HasValue
                        ? Math.Min(admission.DueDate.Value, DateTime.DaysInMonth(m.Year, m.Month))
                        : 1;
                    toCreate.Add(new FeeDue
                    {
                        FeeDueId        = Guid.NewGuid(),
                        AdmissionId     = admission.AdmissionID,
                        FeeType         = FeeDueType.Monthly,
                        FeeMonth        = m,
                        BaseAmount      = 0m,
                        LateFeeAmount   = 0m,
                        DueDate         = new DateTime(m.Year, m.Month, dueDay),
                        IsLateFeeWaived = true,
                        Status          = FeeDueStatus.Waived,
                        CreatedAt       = now
                    });
                    result.MonthsCreated++;
                }
            }

            if (toCreate.Count > 0)
                await repository.AddFeeDuesAsync(toCreate);

            await repository.SaveChangesAsync();
            return result;
        }

        public async Task<Guid> AwardScholarshipAsync(AwardScholarshipRequestDto request)
        {
            var admission = await repository.GetActiveAdmissionByStudentIdAsync(request.StudentId);
            if (admission == null)
                throw new InvalidOperationException("Active admission not found for the student.");

            var from = new DateTime(request.FromMonth.Year, request.FromMonth.Month, 1);
            var to   = new DateTime(request.ToMonth.Year, request.ToMonth.Month, 1);
            if (to < from)
                throw new InvalidOperationException("ToMonth must be on or after FromMonth.");

            var pct = Math.Clamp(request.Percent, 1, 100);
            var created = await scholarshipRepository.AddAsync(new Scholarship
            {
                StudentID       = request.StudentId,
                AdmissionID     = admission.AdmissionID,
                DiscountPercent = pct,
                IsLeave         = false,
                FromMonth       = from,
                ToMonth         = to,
                Reason          = request.Reason,
                Status          = ScholarshipStatus.Active
            }, string.Empty);

            return created.ScholarshipID;
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

        public async Task<FeeDueDto?> TryGenerateCardFeeAsync(Guid studentId, decimal requestedAmount)
        {
            var admission = await repository.GetActiveAdmissionByStudentIdAsync(studentId);
            if (admission == null) return null;

            var settings = await GetFeeSettingsValuesAsync();
            var amount = requestedAmount > 0 ? requestedAmount : settings.CardFeeAmount;
            amount = NormalizeAmount(amount);
            if (amount <= 0) return null;

            var today = DateTime.UtcNow.Date;
            var due = BuildOneTimeFee(admission, FeeDueType.Card, amount, settings.LateFeeAmount, today);

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

            if (admission.IsFree)
            {
                throw new InvalidOperationException("This student is marked as free — no fee is charged.");
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

            return new FeeDue
            {
                FeeDueId = Guid.NewGuid(),
                AdmissionId = admission.AdmissionID,
                FeeType = feeType,
                FeeMonth = null,
                BaseAmount = amount,
                LateFeeAmount = 0m,   // admission/card fees never carry a late fee
                DueDate = feeType == FeeDueType.Card ? today : dueDate,
                IsLateFeeWaived = false,
                Status = FeeDueStatus.Unpaid,
                CreatedAt = DateTime.UtcNow
            };
        }

        private static bool ApplyLateFeeIfNeeded(FeeDue due, DateTime today, decimal lateFeeAmount) =>
            FeeCalculator.ApplyLateFeeIfNeeded(due, today, lateFeeAmount);

        private async Task<(decimal LateFeeAmount, decimal AdmissionFeeAmount, decimal CardFeeAmount, DateTime? FeeStartMonth)> GetFeeSettingsValuesAsync()
        {
            var settings = await repository.GetFeeSettingsAsync();
            if (settings == null)
            {
                return (NormalizeAmount(options.LateFeeAmount), NormalizeAmount(options.AdmissionFeeAmount), NormalizeAmount(options.CardFeeAmount), null);
            }

            return (NormalizeAmount(settings.LateFeeAmount), NormalizeAmount(settings.AdmissionFeeAmount), NormalizeAmount(settings.CardFeeAmount), settings.FeeStartMonth);
        }

        private static decimal NormalizeAmount(decimal amount) =>
            FeeCalculator.NormalizeAmount(amount);

        // ── Fee matrix ────────────────────────────────────────────────────────
        public async Task<FeeMatrixDto> GetFeeMatrixAsync(Guid? classId, Guid? teacherId, string? status)
        {
            var activeTerm = await termRepository.GetActiveAsync();
            if (activeTerm == null)
                return new FeeMatrixDto();

            var termId     = activeTerm.TermID;
            var today      = DateTime.UtcNow;
            var currentMonth = new DateTime(today.Year, today.Month, 1); // don't show future months

            // All class-students in this term (optionally filtered)
            var classStudents = await repository.GetEnrolledStudentsForMatrixAsync(termId, classId, teacherId);

            // Deduplicate: one student may appear in multiple classes — take the first
            var seen     = new HashSet<Guid>();
            var uniqueCS = new List<ClassStudents>();
            foreach (var cs in classStudents)
                if (seen.Add(cs.StudentID)) uniqueCS.Add(cs);

            if (uniqueCS.Count == 0)
                return new FeeMatrixDto();

            // Get active admissions for all these students
            var studentIds   = uniqueCS.Select(cs => cs.StudentID).ToList();
            var admissions   = await repository.GetAllActiveAdmissionsAsync();
            var admissionMap = admissions
                .Where(a => studentIds.Contains(a.StudentID))
                .ToDictionary(a => a.StudentID);

            // ── Month columns: earliest admission date → current month ──────────
            // Each student's admission start determines when their fee history begins.
            // The overall column range starts at whichever student was admitted earliest.
            DateTime matrixStart;
            if (admissionMap.Count > 0)
            {
                var earliest = admissionMap.Values.Min(a => a.RegistrationDate);
                matrixStart  = new DateTime(earliest.Year, earliest.Month, 1);
            }
            else
            {
                // Fallback: start from term start if no admissions found
                matrixStart = new DateTime(activeTerm.TermStart.Year, activeTerm.TermStart.Month, 1);
            }

            // Don't show months before the configured fee-start month.
            var feeSettings = await GetFeeSettingsValuesAsync();
            if (feeSettings.FeeStartMonth.HasValue && matrixStart < feeSettings.FeeStartMonth.Value)
            {
                matrixStart = feeSettings.FeeStartMonth.Value;
            }

            var months = new List<DateTime>();
            for (var m = matrixStart; m <= currentMonth; m = m.AddMonths(1))
                months.Add(m);

            if (months.Count == 0)
                return new FeeMatrixDto();

            var admissionIds = admissionMap.Values.Select(a => a.AdmissionID).ToList();

            // Load all monthly dues in the full matrix date range
            var allDues = await repository.GetMonthlyDuesForMatrixAsync(
                admissionIds, matrixStart, currentMonth);

            // Group dues by admissionId → dict of labelMonth → due
            var duesByAdmission = allDues
                .GroupBy(d => d.AdmissionId)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToDictionary(d => new DateTime(d.FeeMonth!.Value.Year, d.FeeMonth.Value.Month, 1))
                );

            // Build rows
            var rows = new List<StudentFeeRowDto>();
            foreach (var cs in uniqueCS)
            {
                admissionMap.TryGetValue(cs.StudentID, out var admission);
                var admId = admission?.AdmissionID;

                // The month from which this student's fees start
                var studentStart = admission != null
                    ? new DateTime(admission.RegistrationDate.Year, admission.RegistrationDate.Month, 1)
                    : currentMonth; // no admission → treat all months as NR

                var monthlyDues = admId.HasValue && duesByAdmission.TryGetValue(admId.Value, out var dm)
                    ? dm : new Dictionary<DateTime, FeeDue>();

                var monthlyStatus = new Dictionary<string, string?>();
                var unpaidCount   = 0;
                var totalRemaining = 0m;
                DateTime? nextDueDate = null;

                foreach (var month in months)
                {
                    var key = month.ToString("yyyy-MM");

                    // Before this student's admission month → Not Registered
                    if (month < studentStart)
                    {
                        monthlyStatus[key] = "NR";
                        continue;
                    }

                    // Free student → no fee charged for any enrolled month
                    if (admission != null && admission.IsFree)
                    {
                        monthlyStatus[key] = "Free";
                        continue;
                    }

                    if (monthlyDues.TryGetValue(month, out var due))
                    {
                        monthlyStatus[key] = due.Status.ToString(); // "Paid"/"Unpaid"/"Partial"/"Waived"

                        if (due.Status != FeeDueStatus.Paid && due.Status != FeeDueStatus.Waived)
                        {
                            var totalDue    = due.BaseAmount + (due.IsLateFeeWaived ? 0m : due.LateFeeAmount);
                            var paid        = due.PaymentDetails.Sum(p => p.PaidAmount);
                            var remaining   = totalDue - paid;
                            totalRemaining += remaining;
                            unpaidCount++;

                            if (nextDueDate == null || due.DueDate < nextDueDate)
                                nextDueDate = due.DueDate;
                        }
                    }
                    else
                    {
                        monthlyStatus[key] = null; // enrolled but due not yet generated
                    }
                }

                rows.Add(new StudentFeeRowDto
                {
                    StudentId        = cs.StudentID,
                    StudentName      = cs.Student?.StudentName ?? string.Empty,
                    RegistrationNo   = cs.Student?.RegistrationNo ?? string.Empty,
                    FatherName       = cs.Student?.FatherName ?? string.Empty,
                    ClassName        = cs.CurrentClass?.Class?.ClassName ?? string.Empty,
                    TeacherName      = cs.CurrentClass?.Teacher?.TeacherName ?? string.Empty,
                    SectionName      = cs.CurrentClass?.Section?.Name ?? string.Empty,
                    MonthlyFeeAmount = admission?.MonthlyFee ?? 0m,
                    TotalRemaining   = totalRemaining,
                    UnpaidMonths     = unpaidCount,
                    NextDueDate      = nextDueDate,
                    MonthlyStatus    = monthlyStatus,
                });
            }

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(status) &&
                status.Equals("unpaid", StringComparison.OrdinalIgnoreCase))
            {
                rows = rows.Where(r => r.UnpaidMonths > 0).ToList();
            }

            return new FeeMatrixDto { Months = months, Rows = rows };
        }

        // ── Bulk monthly due generation ───────────────────────────────────────
        // Iterates every active admission and runs the same per-student logic.
        // Errors for individual admissions are collected and returned rather than
        // aborting the whole batch.
        public async Task<BulkGenerateResultDto> BulkGenerateMonthlyDuesAsync()
        {
            var admissions = await repository.GetAllActiveAdmissionsAsync();
            var result = new BulkGenerateResultDto { AdmissionsProcessed = admissions.Count };

            var settings = await GetFeeSettingsValuesAsync();
            var today = DateTime.UtcNow.Date;
            var now = DateTime.UtcNow;

            foreach (var admission in admissions)
            {
                // Free students are never charged — skip silently.
                if (admission.IsFree)
                {
                    continue;
                }

                // No monthly fee configured → nothing to charge. Skip silently
                // (these students often have no due day either, and that's expected).
                if (admission.MonthlyFee <= 0m)
                {
                    continue;
                }

                // Due day may be unset on older/imported admissions — fall back to the
                // registration day rather than reporting an error.
                var effectiveDueDay = admission.DueDate ?? admission.RegistrationDate.Day;

                try
                {
                    var startMonth = new DateTime(admission.RegistrationDate.Year, admission.RegistrationDate.Month, 1);
                    var endMonth   = new DateTime(now.Year, now.Month, 1);

                    // Never generate dues before the configured fee-start month.
                    if (settings.FeeStartMonth.HasValue && startMonth < settings.FeeStartMonth.Value)
                    {
                        startMonth = settings.FeeStartMonth.Value;
                    }

                    if (startMonth > endMonth) continue;

                    // Look one month past endMonth: with the 25th-rule, the last month's
                    // due is labelled the NEXT month, so it must be matched to avoid
                    // re-inserting it (unique index on AdmissionId+FeeType+FeeMonth).
                    var existingMonths = await repository.GetExistingMonthlyFeeMonthsAsync(admission.AdmissionID, startMonth, endMonth.AddMonths(1));
                    // existingSet uses label months (FeeMonth in DB, 25th-rule-adjusted)
                    var existingSet    = new HashSet<DateTime>(existingMonths.Select(m => new DateTime(m.Year, m.Month, 1)));

                    var created = new List<FeeDue>();

                    // Ensure one-time admission fee exists
                    await AddOneTimeFeeIfMissingAsync(admission, FeeDueType.Admission, settings.AdmissionFeeAmount, settings.LateFeeAmount, today, created);

                    var current = startMonth;
                    while (current <= endMonth)
                    {
                        var dueDay  = Math.Min(effectiveDueDay, DateTime.DaysInMonth(current.Year, current.Month));
                        var dueDate = new DateTime(current.Year, current.Month, dueDay);

                        // 25th-rule: payment on or after the 25th covers the NEXT month.
                        var labelMonth = dueDay >= 25 ? current.AddMonths(1) : current;

                        if (!existingSet.Contains(labelMonth))
                        {
                            var (baseAmt, status, applyLate) = await ApplyConcessionAsync(admission.AdmissionID, admission.MonthlyFee, labelMonth);
                            var isLate  = applyLate && today > dueDate.Date;

                            created.Add(new FeeDue
                            {
                                FeeDueId         = Guid.NewGuid(),
                                AdmissionId      = admission.AdmissionID,
                                FeeType          = FeeDueType.Monthly,
                                FeeMonth         = labelMonth,   // label shown to users
                                BaseAmount       = baseAmt,
                                LateFeeAmount    = isLate ? NormalizeAmount(settings.LateFeeAmount) : 0m,
                                DueDate          = dueDate,      // physical collection date
                                IsLateFeeWaived  = false,
                                Status           = status,
                                CreatedAt        = now
                            });
                        }

                        current = current.AddMonths(1);
                    }

                    if (created.Count > 0)
                    {
                        await repository.AddFeeDuesAsync(created);
                        await repository.SaveChangesAsync();
                        result.AdmissionsWithNewDues++;
                        result.TotalDuesCreated += created.Count;
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add(new BulkGenerateErrorDto
                    {
                        AdmissionId = admission.AdmissionID,
                        StudentId   = admission.StudentID,
                        Reason      = ex.Message
                    });
                }
            }

            return result;
        }
    }
}
