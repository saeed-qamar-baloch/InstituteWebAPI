using InstituteWebApp.Models.Domain;
using InstituteWebAPI.Data;
using InstituteWebAPI.Repositories.IRepository;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Repositories.Repository
{
    public class ClassStudentsRepository : IClassStudentsRepository
    {
        private readonly RozhnInstituteDbContext dbContext;

        public ClassStudentsRepository(RozhnInstituteDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<ClassStudents> AddAsync(ClassStudents classStudent)
        {
            var existing = await dbContext.ClassStudents
                .FirstOrDefaultAsync(cs => cs.CurrentClassID == classStudent.CurrentClassID
                                         && cs.StudentID       == classStudent.StudentID);

            if (existing != null)
            {
                // A row already exists for this student+class. If it's stuck on a
                // non-matching status (e.g. left over from a bad import, or a prior
                // "Inactive"/"Suspended" state), reactivate it instead of silently
                // leaving the student excluded from attendance/marks/reports.
                if (existing.Status == classStudent.Status)
                    throw new InvalidOperationException("This student is already enrolled in this class.");

                existing.Status = classStudent.Status;
                await dbContext.SaveChangesAsync();
                return existing;
            }

            await dbContext.ClassStudents.AddAsync(classStudent);
            await dbContext.SaveChangesAsync();
            return classStudent;
        }

        /// <summary>
        /// Assigns multiple students to a class in one call.
        /// If a student already has a row for this class whose Status doesn't match
        /// the target status (e.g. left over from a bad import, or "Inactive"/"Suspended"),
        /// it is reactivated/updated rather than silently skipped — otherwise the student
        /// stays permanently excluded from attendance/marks/reports even after being
        /// "added" again. Only rows already on the exact target status are true no-op skips.
        /// </summary>
        public async Task<(int assigned, List<string> skippedNames)> BulkAddAsync(
            Guid currentClassId, List<Guid> studentIds, string status)
        {
            // Fetch existing rows for this class among the requested students (tracked, so we can update)
            var existingRows = await dbContext.ClassStudents
                .Where(cs => cs.CurrentClassID == currentClassId && studentIds.Contains(cs.StudentID))
                .ToListAsync();
            var existingByStudent = existingRows.ToDictionary(cs => cs.StudentID);

            var skippedIds = new List<Guid>();
            var reactivatedOrAdded = 0;

            foreach (var studentId in studentIds)
            {
                if (existingByStudent.TryGetValue(studentId, out var row))
                {
                    if (string.Equals(row.Status, status, StringComparison.OrdinalIgnoreCase))
                    {
                        skippedIds.Add(studentId);
                    }
                    else
                    {
                        row.Status = status;
                        reactivatedOrAdded++;
                    }
                }
                else
                {
                    dbContext.ClassStudents.Add(new ClassStudents
                    {
                        ClassStudentID = Guid.NewGuid(),
                        CurrentClassID = currentClassId,
                        StudentID      = studentId,
                        Status         = status,
                    });
                    reactivatedOrAdded++;
                }
            }

            if (reactivatedOrAdded > 0)
                await dbContext.SaveChangesAsync();

            var skippedNames = new List<string>();
            if (skippedIds.Any())
            {
                skippedNames = await dbContext.Students
                    .Where(s => skippedIds.Contains(s.StudentID))
                    .Select(s => s.StudentName)
                    .ToListAsync();
            }

            return (reactivatedOrAdded, skippedNames);
        }

        public async Task<ClassStudents?> DeleteAsync(Guid id)
        {
            var existing = await dbContext.ClassStudents.FindAsync(id);
            if (existing == null) return null;

            dbContext.ClassStudents.Remove(existing);
            await dbContext.SaveChangesAsync();
            return existing;
        }

        public async Task<List<ClassStudents>> GetAllAsync()
        {
            return await dbContext.ClassStudents
                .Include(cs => cs.Student)
                    .ThenInclude(s => s.Village)
                .Include(cs => cs.CurrentClass)
                    .ThenInclude(cc => cc.Class)
                        .ThenInclude(c => c.Course)
                .Include(cs => cs.CurrentClass)
                    .ThenInclude(cc => cc.Slot)
                .Include(cs => cs.CurrentClass)
                    .ThenInclude(cc => cc.Teacher)
                .Include(cs => cs.CurrentClass)
                    .ThenInclude(cc => cc.Section)
                .Include(cs => cs.CurrentClass)
                    .ThenInclude(cc => cc.Term)
                .OrderBy(cs => cs.CurrentClass.Class.ClassName)
                    .ThenBy(cs => cs.Student.StudentName)
                .ToListAsync();
        }

        /// <summary>
        /// Returns only enrolments whose CurrentClass belongs to the given term.
        /// </summary>
        public async Task<List<ClassStudents>> GetByTermAsync(Guid termId)
        {
            return await dbContext.ClassStudents
                .Include(cs => cs.Student)
                    .ThenInclude(s => s.Village)
                .Include(cs => cs.CurrentClass)
                    .ThenInclude(cc => cc.Class)
                        .ThenInclude(c => c.Course)
                .Include(cs => cs.CurrentClass)
                    .ThenInclude(cc => cc.Slot)
                .Include(cs => cs.CurrentClass)
                    .ThenInclude(cc => cc.Teacher)
                .Include(cs => cs.CurrentClass)
                    .ThenInclude(cc => cc.Section)
                .Include(cs => cs.CurrentClass)
                    .ThenInclude(cc => cc.Term)
                .Where(cs => cs.CurrentClass.TermID == termId)
                .OrderBy(cs => cs.CurrentClass.Class.ClassName)
                    .ThenBy(cs => cs.Student.StudentName)
                .ToListAsync();
        }

        public async Task<ClassStudents?> GetAsync(Guid id)
        {
            return await dbContext.ClassStudents
                .Include(cs => cs.Student)
                .Include(cs => cs.CurrentClass)
                    .ThenInclude(cc => cc.Class)
                .Include(cs => cs.CurrentClass)
                    .ThenInclude(cc => cc.Slot)
                .Include(cs => cs.CurrentClass)
                    .ThenInclude(cc => cc.Teacher)
                .Include(cs => cs.CurrentClass)
                    .ThenInclude(cc => cc.Section)
                .Include(cs => cs.CurrentClass)
                    .ThenInclude(cc => cc.Term)
                .FirstOrDefaultAsync(cs => cs.ClassStudentID == id);
        }

        public async Task<List<ClassStudents>> GetByClassAsync(Guid currentClassId)
        {
            return await dbContext.ClassStudents
                .Include(cs => cs.Student)
                .Where(cs => cs.CurrentClassID == currentClassId)
                .OrderBy(cs => cs.Student.StudentName)
                .ToListAsync();
        }

        public async Task<ClassStudents?> UpdateAsync(Guid id, ClassStudents classStudent)
        {
            var existing = await dbContext.ClassStudents.FindAsync(id);
            if (existing == null) return null;

            existing.StudentID      = classStudent.StudentID;
            existing.CurrentClassID = classStudent.CurrentClassID;
            existing.Status         = classStudent.Status;

            await dbContext.SaveChangesAsync();
            return existing;
        }

        /// <summary>
        /// Returns students who have no ClassStudents record for any CurrentClass
        /// belonging to the given term.
        /// </summary>
        public async Task<List<Students>> GetUnenrolledStudentsAsync(Guid termId)
        {
            var enrolledInTerm = await dbContext.ClassStudents
                .Where(cs => cs.CurrentClass.TermID == termId)
                .Select(cs => cs.StudentID)
                .Distinct()
                .ToListAsync();

            return await dbContext.Students
                .Where(s => !enrolledInTerm.Contains(s.StudentID))
                .OrderBy(s => s.StudentName)
                .ToListAsync();
        }
    }
}
