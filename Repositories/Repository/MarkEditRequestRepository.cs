using InstituteWebAPI.Data;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebAPI.Services.StudentMonthlyResults;
using InstituteWebApp.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Repositories.Repository
{
    public class MarkEditRequestRepository : IMarkEditRequestRepository
    {
        private readonly RozhnInstituteDbContext _db;
        private readonly IStudentMonthlyResultService _monthlyResultService;

        public MarkEditRequestRepository(
            RozhnInstituteDbContext db,
            IStudentMonthlyResultService monthlyResultService)
        {
            _db = db;
            _monthlyResultService = monthlyResultService;
        }

        private IQueryable<MarkEditRequest> BaseQuery() =>
            _db.MarkEditRequests
               .Include(r => r.Teacher)
               .Include(r => r.StudentMark)
                   .ThenInclude(sm => sm.Student)
               .Include(r => r.StudentMark)
                   .ThenInclude(sm => sm.Test)
               .AsNoTracking();

        public async Task<List<MarkEditRequest>> GetAllAsync(Guid? teacherId = null, int? status = null)
        {
            var q = BaseQuery();

            if (teacherId.HasValue)
                q = q.Where(r => r.TeacherID == teacherId.Value);

            if (status.HasValue)
                q = q.Where(r => (int)r.Status == status.Value);

            return await q
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<MarkEditRequest?> GetByIdAsync(Guid id) =>
            await BaseQuery().FirstOrDefaultAsync(r => r.RequestID == id);

        public async Task<MarkEditRequest> AddAsync(MarkEditRequest request)
        {
            // Snapshot current marks at submission time
            var mark = await _db.StudentMarks.FindAsync(request.StudentMarkID);
            if (mark == null)
                throw new InvalidOperationException("StudentMark not found.");

            request.RequestID    = Guid.NewGuid();
            request.CurrentMarks = mark.ObtainedMarks;
            request.Status       = MarkEditRequestStatus.Pending;
            request.CreatedAt    = DateTime.UtcNow;
            request.ModifiedAt   = DateTime.UtcNow;

            await _db.MarkEditRequests.AddAsync(request);
            await _db.SaveChangesAsync();

            return (await GetByIdAsync(request.RequestID))!;
        }

        public async Task<MarkEditRequest?> ReviewAsync(
            Guid requestId,
            MarkEditRequestStatus status,
            string reviewedByUserId,
            string? reviewRemarks)
        {
            var request = await _db.MarkEditRequests
                .Include(r => r.StudentMark)
                    .ThenInclude(sm => sm.Test)
                .FirstOrDefaultAsync(r => r.RequestID == requestId);

            if (request == null) return null;
            if (request.Status != MarkEditRequestStatus.Pending)
                throw new InvalidOperationException("Only pending requests can be reviewed.");

            request.Status           = status;
            request.ReviewedByUserID = reviewedByUserId;
            request.ReviewedAt       = DateTime.UtcNow;
            request.ReviewRemarks    = reviewRemarks;
            request.ModifiedAt       = DateTime.UtcNow;

            if (status == MarkEditRequestStatus.Approved)
            {
                // Apply the mark change
                var mark = request.StudentMark;
                mark.ObtainedMarks = request.RequestedMarks;
                mark.Percentage    = mark.TotalMarks > 0
                    ? (mark.ObtainedMarks / mark.TotalMarks * 100f)
                    : 0f;

                // Recalculate the monthly aggregate for this student
                var test = mark.Test;
                await _db.SaveChangesAsync();   // persist mark first so recalc reads updated value

                await _monthlyResultService.RecalculateAsync(
                    termId:       mark.TermID,
                    currentClassId: test.CurrentClassID,
                    termMonthId:  test.TermMonthID,
                    studentIds:   new[] { mark.StudentID });
            }
            else
            {
                await _db.SaveChangesAsync();
            }

            return (await GetByIdAsync(requestId))!;
        }
    }
}
