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
            var duplicate = await dbContext.ClassStudents
                .AnyAsync(cs => cs.CurrentClassID == classStudent.CurrentClassID
                             && cs.StudentID       == classStudent.StudentID);

            if (duplicate)
                throw new InvalidOperationException("This student is already enrolled in this class.");

            await dbContext.ClassStudents.AddAsync(classStudent);
            await dbContext.SaveChangesAsync();
            return classStudent;
        }

        /// <summary>
        /// Assigns multiple students to a class in one call.
        /// Duplicates are silently skipped; the caller gets a summary of what was assigned vs skipped.
        /// </summary>
        public async Task<(int assigned, List<string> skippedNames)> BulkAddAsync(
            Guid currentClassId, List<Guid> studentIds, string status)
        {
            // Fetch already-enrolled student IDs for this class (single query)
            var alreadyEnrolled = await dbContext.ClassStudents
                .Where(cs => cs.CurrentClassID == currentClassId)
                .Select(cs => cs.StudentID)
                .ToHashSetAsync();

            // Fetch names for any duplicates so we can report them
            var skippedIds   = studentIds.Where(id => alreadyEnrolled.Contains(id)).ToList();
            var skippedNames = new List<string>();
            if (skippedIds.Any())
            {
                skippedNames = await dbContext.Students
                    .Where(s => skippedIds.Contains(s.StudentID))
                    .Select(s => s.StudentName)
                    .ToListAsync();
            }

            var toAdd = studentIds.Where(id => !alreadyEnrolled.Contains(id)).ToList();
            foreach (var studentId in toAdd)
            {
                dbContext.ClassStudents.Add(new ClassStudents
                {
                    ClassStudentID = Guid.NewGuid(),
                    CurrentClassID = currentClassId,
                    StudentID      = studentId,
                    Status         = status,
                });
            }

            if (toAdd.Any())
                await dbContext.SaveChangesAsync();

            return (toAdd.Count, skippedNames);
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
