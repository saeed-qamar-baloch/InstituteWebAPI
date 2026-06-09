using InstituteWebAPI.Data;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Repositories.Repository
{
    public class GuardianRepository : IGuardianRepository
    {
        private readonly RozhnInstituteDbContext _db;

        public GuardianRepository(RozhnInstituteDbContext db)
        {
            _db = db;
        }

        public async Task<List<Guardian>> GetAllAsync(Guid? studentId = null)
        {
            var query = _db.Guardians
                .Include(g => g.Student)
                .AsNoTracking()
                .AsQueryable();

            if (studentId.HasValue)
                query = query.Where(g => g.StudentID == studentId.Value);

            return await query
                .OrderBy(g => g.Student.StudentName)
                .ThenBy(g => g.GuardianName)
                .ToListAsync();
        }

        public async Task<Guardian?> GetByIdAsync(Guid id)
        {
            return await _db.Guardians
                .Include(g => g.Student)
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.GuardianID == id);
        }

        public async Task<List<Guardian>> GetByStudentIdAsync(Guid studentId)
        {
            return await _db.Guardians
                .Include(g => g.Student)
                .AsNoTracking()
                .Where(g => g.StudentID == studentId)
                .OrderBy(g => g.GuardianName)
                .ToListAsync();
        }

        public async Task<Guardian> AddAsync(Guardian guardian)
        {
            guardian.GuardianID = Guid.NewGuid();
            guardian.CreatedAt = DateTime.UtcNow;
            guardian.ModifiedAt = DateTime.UtcNow;

            await _db.Guardians.AddAsync(guardian);
            await _db.SaveChangesAsync();

            // Return with navigation property loaded
            return (await GetByIdAsync(guardian.GuardianID))!;
        }

        public async Task<Guardian?> UpdateAsync(Guid id, Guardian guardian)
        {
            var existing = await _db.Guardians.FindAsync(id);
            if (existing == null) return null;

            existing.GuardianName = guardian.GuardianName;
            existing.Relation     = guardian.Relation;
            existing.Contact      = guardian.Contact;
            existing.Cnic         = guardian.Cnic;
            existing.Address      = guardian.Address;
            existing.Occupation   = guardian.Occupation;
            existing.Remarks      = guardian.Remarks;
            existing.ModifiedAt   = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return (await GetByIdAsync(id))!;
        }

        public async Task<Guardian?> DeleteAsync(Guid id)
        {
            var existing = await _db.Guardians.FindAsync(id);
            if (existing == null) return null;

            _db.Guardians.Remove(existing);
            await _db.SaveChangesAsync();
            return existing;
        }
    }
}
