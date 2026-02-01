using InstituteWebAPI.Data;
using InstituteWebApp.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Services.StudentMonthlyResults
{
    public sealed class StudentMonthlyResultService : IStudentMonthlyResultService
    {
        private readonly RozhnInstituteDbContext dbContext;

        public StudentMonthlyResultService(RozhnInstituteDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task RecalculateAsync(Guid termId, Guid currentClassId, Guid termMonthId, IEnumerable<Guid> studentIds)
        {
            var ids = studentIds.Distinct().ToList();
            if (ids.Count == 0) return;

            // All tests in this class+month.
            var tests = await dbContext.Tests
                .AsNoTracking()
                .Where(t => t.CurrentClassID == currentClassId && t.TermMonthID == termMonthId)
                .Select(t => new { t.TestID, t.TotalMarks })
                .ToListAsync();

            var testIds = tests.Select(t => t.TestID).ToList();
            var totalMarks = tests.Sum(t => t.TotalMarks);

            // If there are no tests, then monthly result should be cleared/zeroed.
            // We'll upsert rows with 0 totals/obtained.
            Dictionary<Guid, float> obtainedByStudent;
            if (testIds.Count == 0)
            {
                obtainedByStudent = ids.ToDictionary(x => x, _ => 0f);
            }
            else
            {
                obtainedByStudent = await dbContext.StudentMarks
                    .AsNoTracking()
                    .Where(sm => sm.TermID == termId && testIds.Contains(sm.TestID) && ids.Contains(sm.StudentID))
                    .GroupBy(sm => sm.StudentID)
                    .Select(g => new { StudentID = g.Key, Obtained = g.Sum(x => x.ObtainedMarks) })
                    .ToDictionaryAsync(x => x.StudentID, x => x.Obtained);

                // Ensure missing students exist with 0.
                foreach (var id in ids)
                {
                    if (!obtainedByStudent.ContainsKey(id))
                        obtainedByStudent[id] = 0f;
                }
            }

            var existing = await dbContext.StudentMonthlyResults
                .Where(x => x.TermID == termId && x.CurrentClassID == currentClassId && x.TermMonthID == termMonthId && ids.Contains(x.StudentID))
                .ToListAsync();

            var existingByStudent = existing.ToDictionary(x => x.StudentID, x => x);

            foreach (var studentId in ids)
            {
                var obtained = obtainedByStudent.TryGetValue(studentId, out var v) ? v : 0f;
                var pct = totalMarks <= 0 ? 0f : (obtained / totalMarks * 100f);

                if (existingByStudent.TryGetValue(studentId, out var row))
                {
                    row.TotalMarks = totalMarks;
                    row.ObtainedMarks = obtained;
                    row.Percentage = pct;
                    row.UpdatedOn = DateTime.UtcNow;
                }
                else
                {
                    dbContext.StudentMonthlyResults.Add(new StudentMonthlyResult
                    {
                        StudentMonthlyResultID = Guid.NewGuid(),
                        TermID = termId,
                        CurrentClassID = currentClassId,
                        TermMonthID = termMonthId,
                        StudentID = studentId,
                        TotalMarks = totalMarks,
                        ObtainedMarks = obtained,
                        Percentage = pct,
                        CreatedOn = DateTime.UtcNow
                    });
                }
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
