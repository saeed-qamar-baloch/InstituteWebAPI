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
        var active = await _db.Term.AsNoTracking().FirstOrDefaultAsync(t => t.IsActive, cancellationToken);
        if (active is null)
        {
            throw new InvalidOperationException("No active term found. Please activate a term first.");
        }

        return active;
    }
}
