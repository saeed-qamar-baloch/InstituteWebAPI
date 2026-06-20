using InstituteWebAPI.Data;
using InstituteWebAPI.Repositories.IRepository;
using InstituteWebApp.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Repositories.Repository
{
    public class LearnerRepository : ILearnerRepository
    {
        private readonly RozhnInstituteDbContext dbContext;
        public LearnerRepository(RozhnInstituteDbContext dbContext) { this.dbContext = dbContext; }

        public async Task<Learner?> GetByIdAsync(Guid id) =>
            await dbContext.Learners.FirstOrDefaultAsync(l => l.LearnerID == id);

        public async Task<Learner?> GetByEmailAsync(string email)
        {
            var e = (email ?? string.Empty).Trim().ToLowerInvariant();
            return await dbContext.Learners.FirstOrDefaultAsync(l => l.Email != null && l.Email.ToLower() == e);
        }

        public async Task<Learner?> GetByGoogleSubjectAsync(string sub) =>
            await dbContext.Learners.FirstOrDefaultAsync(l => l.GoogleSubject == sub);

        public async Task<Learner> AddAsync(Learner learner)
        {
            learner.LearnerID = Guid.NewGuid();
            learner.CreatedAt = DateTime.UtcNow;
            if (learner.Email != null) learner.Email = learner.Email.Trim();
            await dbContext.Learners.AddAsync(learner);
            await dbContext.SaveChangesAsync();
            return learner;
        }

        public async Task UpdateAsync(Learner learner)
        {
            dbContext.Learners.Update(learner);
            await dbContext.SaveChangesAsync();
        }
    }

    /// <summary>Merge progress + compute a learning-day streak.</summary>
    public static class StreakCalc
    {
        /// <summary>Union two day-lists (YYYY-MM-DD), returns sorted unique list.</summary>
        public static List<string> UnionDays(IEnumerable<string> a, IEnumerable<string> b)
        {
            var set = new SortedSet<string>(StringComparer.Ordinal);
            foreach (var d in a.Concat(b))
            {
                if (DateTime.TryParse(d, out var dt)) set.Add(dt.ToString("yyyy-MM-dd"));
            }
            return set.ToList();
        }

        /// <summary>Current streak = consecutive days ending today or yesterday.</summary>
        public static int Current(List<string> sortedDays)
        {
            if (sortedDays.Count == 0) return 0;
            var days = new HashSet<string>(sortedDays);
            var today = DateTime.UtcNow.Date;
            // anchor: today if present, else yesterday if present, else no current streak
            DateTime cursor;
            if (days.Contains(today.ToString("yyyy-MM-dd"))) cursor = today;
            else if (days.Contains(today.AddDays(-1).ToString("yyyy-MM-dd"))) cursor = today.AddDays(-1);
            else return 0;

            int streak = 0;
            while (days.Contains(cursor.ToString("yyyy-MM-dd")))
            {
                streak++;
                cursor = cursor.AddDays(-1);
            }
            return streak;
        }

        /// <summary>Longest run of consecutive days in history.</summary>
        public static int Longest(List<string> sortedDays)
        {
            if (sortedDays.Count == 0) return 0;
            int best = 1, run = 1;
            for (int i = 1; i < sortedDays.Count; i++)
            {
                var prev = DateTime.Parse(sortedDays[i - 1]);
                var cur = DateTime.Parse(sortedDays[i]);
                if ((cur - prev).TotalDays == 1) run++;
                else run = 1;
                if (run > best) best = run;
            }
            return best;
        }
    }
}
