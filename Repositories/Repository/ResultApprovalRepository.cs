using InstituteWebAPI.Data;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Repositories.Repository
{
    public class ResultApprovalRepository : IResultApprovalRepository
    {
        private readonly RozhnInstituteDbContext _db;

        public ResultApprovalRepository(RozhnInstituteDbContext db)
        {
            _db = db;
        }

        private IQueryable<ResultApproval> BaseQuery() =>
            _db.ResultApprovals
               .Include(r => r.Term)
               .Include(r => r.CurrentClass)
                   .ThenInclude(cc => cc.Class)
               .AsNoTracking();

        public async Task<List<ResultApproval>> GetAllAsync(Guid? termId = null, Guid? currentClassId = null)
        {
            var q = BaseQuery();

            if (termId.HasValue)
                q = q.Where(r => r.TermID == termId.Value);

            if (currentClassId.HasValue)
                q = q.Where(r => r.CurrentClassID == currentClassId.Value);

            return await q
                .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt)
                .ToListAsync();
        }

        public async Task<ResultApproval?> GetByIdAsync(Guid id) =>
            await BaseQuery().FirstOrDefaultAsync(r => r.ApprovalID == id);

        public async Task<ResultApproval?> GetByClassAndTermAsync(Guid termId, Guid currentClassId) =>
            await BaseQuery()
                .FirstOrDefaultAsync(r => r.TermID == termId && r.CurrentClassID == currentClassId);

        public async Task<bool> IsApprovedAsync(Guid termId, Guid currentClassId) =>
            await _db.ResultApprovals
                .AnyAsync(r => r.TermID == termId
                            && r.CurrentClassID == currentClassId
                            && r.IsApproved);

        public async Task<ResultApproval> UpsertAsync(ResultApproval approval, string approvedByUserId)
        {
            var existing = await _db.ResultApprovals
                .FirstOrDefaultAsync(r => r.TermID == approval.TermID
                                       && r.CurrentClassID == approval.CurrentClassID);

            if (existing == null)
            {
                // Create new record
                approval.ApprovalID = Guid.NewGuid();
                approval.CreatedAt  = DateTime.UtcNow;
                approval.UpdatedAt  = null;

                if (approval.IsApproved)
                {
                    approval.ApprovedByUserID = approvedByUserId;
                    approval.ApprovedAt       = DateTime.UtcNow;
                }

                await _db.ResultApprovals.AddAsync(approval);
            }
            else
            {
                // Update existing record
                existing.IsApproved = approval.IsApproved;
                existing.Remarks    = approval.Remarks;
                existing.UpdatedAt  = DateTime.UtcNow;

                if (approval.IsApproved)
                {
                    existing.ApprovedByUserID = approvedByUserId;
                    existing.ApprovedAt       = DateTime.UtcNow;
                }
                else
                {
                    // Unlocking — clear the approval stamp
                    existing.ApprovedByUserID = null;
                    existing.ApprovedAt       = null;
                }
            }

            await _db.SaveChangesAsync();

            var savedId = existing?.ApprovalID ?? approval.ApprovalID;
            return (await GetByIdAsync(savedId))!;
        }
    }
}
