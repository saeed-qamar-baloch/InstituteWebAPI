using InstituteWebAPI.Data;
using InstituteWebApp.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace InstituteWebAPI.Services.TermContext;

public sealed class TermContext : ITermContext
{
    private readonly RozhnInstituteDbContext _db;

    public TermContext(RozhnInstituteDbContext db)
    {
        _db = db;
    }

    public async Task<Term> GetActiveTermAsync(CancellationToken cancellationToken = default)
    {
        // If more than one term is flagged active (legacy data), pick the most-recently-started
        // so the "active term" is deterministic across the whole app.
        var active = await _db.Term.AsNoTracking()
            .Where(t => t.IsActive)
            .OrderByDescending(t => t.TermStart)
            .FirstOrDefaultAsync(cancellationToken);
        if (active is null)
        {
            throw new InvalidOperationException("No active term found. Please activate a term first.");
        }

        return active;
    }
}
